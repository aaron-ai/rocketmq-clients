/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Proto = Apache.Rocketmq.V2;
using grpc = Grpc.Core;
using NLog;

namespace Org.Apache.Rocketmq
{
    public abstract class Client : IClient
    {
        private static readonly Logger Logger = MqLogManager.Instance.GetCurrentClassLogger();

        private static readonly TimeSpan HeartbeatScheduleDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan HeartbeatSchedulePeriod = TimeSpan.FromSeconds(10);
        private readonly CancellationTokenSource _heartbeatCts;

        private static readonly TimeSpan TopicRouteUpdateScheduleDelay = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan TopicRouteUpdateSchedulePeriod = TimeSpan.FromSeconds(30);
        private readonly CancellationTokenSource _topicRouteUpdateCtx;

        private static readonly TimeSpan SettingsSyncScheduleDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan SettingsSyncSchedulePeriod = TimeSpan.FromMinutes(5);
        private readonly CancellationTokenSource _settingsSyncCtx;

        protected readonly ClientConfig ClientConfig;
        protected readonly IClientManager ClientManager;
        protected readonly string ClientId;

        protected readonly ConcurrentDictionary<Endpoints, bool> Isolated;
        private readonly ConcurrentDictionary<string, TopicRouteData> _topicRouteCache;
        private readonly CancellationTokenSource _telemetryCts;

        private readonly Dictionary<Endpoints, Session> _sessionsTable;
        private readonly ReaderWriterLockSlim _sessionLock;

        protected Client(ClientConfig clientConfig)
        {
            ClientConfig = clientConfig;
            ClientId = Utilities.GetClientId();

            ClientManager = new ClientManager(this);
            Isolated = new ConcurrentDictionary<Endpoints, bool>();
            _topicRouteCache = new ConcurrentDictionary<string, TopicRouteData>();

            _topicRouteUpdateCtx = new CancellationTokenSource();
            _settingsSyncCtx = new CancellationTokenSource();
            _heartbeatCts = new CancellationTokenSource();
            _telemetryCts = new CancellationTokenSource();

            _sessionsTable = new Dictionary<Endpoints, Session>();
            _sessionLock = new ReaderWriterLockSlim();
        }

        public virtual async Task Start()
        {
            Logger.Debug($"Begin to start the rocketmq client, clientId={ClientId}");
            ScheduleWithFixedDelay(UpdateTopicRouteCache, TopicRouteUpdateScheduleDelay, TopicRouteUpdateSchedulePeriod,
                _topicRouteUpdateCtx.Token);
            ScheduleWithFixedDelay(Heartbeat, HeartbeatScheduleDelay, HeartbeatSchedulePeriod, _heartbeatCts.Token);
            ScheduleWithFixedDelay(SyncSettings, SettingsSyncScheduleDelay, SettingsSyncSchedulePeriod,
                _settingsSyncCtx.Token);
            foreach (var topic in GetTopics())
            {
                await FetchTopicRoute(topic);
            }

            Logger.Debug($"Start the rocketmq client successfully, clientId={ClientId}");
        }

        public virtual async Task Shutdown()
        {
            Logger.Debug($"Begin to shutdown rocketmq client, clientId={ClientId}");
            _topicRouteUpdateCtx.Cancel();
            _heartbeatCts.Cancel();
            _telemetryCts.Cancel();
            await ClientManager.Shutdown();
            Logger.Debug($"Shutdown the rocketmq client successfully, clientId={ClientId}");
        }

        private (bool, Session) GetSession(Endpoints endpoints)
        {
            _sessionLock.EnterReadLock();
            try
            {
                // Session exists, return in advance.
                if (_sessionsTable.TryGetValue(endpoints, out var session))
                {
                    return (false, session);
                }
            }
            finally
            {
                _sessionLock.ExitReadLock();
            }

            _sessionLock.EnterWriteLock();
            try
            {
                // Session exists, return in advance.
                if (_sessionsTable.TryGetValue(endpoints, out var session))
                {
                    return (false, session);
                }

                var stream = ClientManager.Telemetry(endpoints);
                var created = new Session(endpoints, stream, this);
                _sessionsTable.Add(endpoints, created);
                return (true, created);
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }
        }

        protected abstract ICollection<string> GetTopics();

        protected abstract Proto::HeartbeatRequest WrapHeartbeatRequest();


        protected abstract void OnTopicRouteDataUpdated0(string topic, TopicRouteData topicRouteData);


        private async Task OnTopicRouteDataFetched(string topic, TopicRouteData topicRouteData)
        {
            var routeEndpoints = new HashSet<Endpoints>();
            foreach (var mq in topicRouteData.MessageQueues)
            {
                routeEndpoints.Add(mq.Broker.Endpoints);
            }

            var existedRouteEndpoints = GetTotalRouteEndpoints();
            var newEndpoints = routeEndpoints.Except(existedRouteEndpoints);

            foreach (var endpoints in newEndpoints)
            {
                var (created, session) = GetSession(endpoints);
                if (created)
                {
                    await session.SyncSettings(true);
                }
            }

            _topicRouteCache[topic] = topicRouteData;
            OnTopicRouteDataUpdated0(topic, topicRouteData);
        }


        /**
         * Return all endpoints of brokers in route table.
         */
        private HashSet<Endpoints> GetTotalRouteEndpoints()
        {
            var endpoints = new HashSet<Endpoints>();
            foreach (var item in _topicRouteCache)
            {
                foreach (var endpoint in item.Value.MessageQueues.Select(mq => mq.Broker.Endpoints))
                {
                    endpoints.Add(endpoint);
                }
            }

            return endpoints;
        }

        private async void UpdateTopicRouteCache()
        {
            Logger.Info($"Start to update topic route cache for a new round, clientId={ClientId}");
            foreach (var topic in GetTopics())
            {
                await FetchTopicRoute(topic);
            }
        }

        private async void SyncSettings()
        {
            var totalRouteEndpoints = GetTotalRouteEndpoints();
            foreach (var (_, session) in totalRouteEndpoints.Select(GetSession))
            {
                await session.SyncSettings(false);
            }
        }

        private void ScheduleWithFixedDelay(Action action, TimeSpan delay, TimeSpan period, CancellationToken token)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay, token);
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"Failed to execute scheduled task, ClientId={ClientId}");
                    }
                    finally
                    {
                        await Task.Delay(period, token);
                    }
                }
            }, token);
        }

        protected async Task<TopicRouteData> GetRouteData(string topic)
        {
            if (_topicRouteCache.TryGetValue(topic, out var topicRouteData))
            {
                return topicRouteData;
            }

            topicRouteData = await FetchTopicRoute(topic);
            return topicRouteData;
        }

        private async Task<TopicRouteData> FetchTopicRoute(string topic)
        {
            var topicRouteData = await FetchTopicRoute0(topic);
            await OnTopicRouteDataFetched(topic, topicRouteData);
            Logger.Info(
                $"Fetch topic route successfully, clientId={ClientId}, topic={topic}, topicRouteData={topicRouteData}");
            return topicRouteData;
        }


        private async Task<TopicRouteData> FetchTopicRoute0(string topic)
        {
            var request = new Proto::QueryRouteRequest
            {
                Topic = new Proto::Resource
                {
                    Name = topic
                },
                Endpoints = ClientConfig.Endpoints.ToProtobuf()
            };

            var response =
                await ClientManager.QueryRoute(ClientConfig.Endpoints, request, ClientConfig.RequestTimeout);
            var code = response.Status.Code;
            if (!Proto.Code.Ok.Equals(code))
            {
                Logger.Error($"Failed to fetch topic route, clientId={ClientId}, topic={topic}, code={code}, " +
                             $"statusMessage={response.Status.Message}");
            }

            StatusChecker.Check(response.Status, request);

            var messageQueues = response.MessageQueues.ToList();
            return new TopicRouteData(messageQueues);
        }

        private async void Heartbeat()
        {
            var endpoints = GetTotalRouteEndpoints();
            var request = WrapHeartbeatRequest();
            Dictionary<Endpoints, Task<Proto.HeartbeatResponse>> responses = new();
            // Collect task into a map.
            foreach (var item in endpoints)
            {
                var task = ClientManager.Heartbeat(item, request, ClientConfig.RequestTimeout);
                responses[item] = task;
            }

            foreach (var item in responses.Keys)
            {
                var response = await responses[item];
                var code = response.Status.Code;

                if (code.Equals(Proto.Code.Ok))
                {
                    Logger.Info($"Send heartbeat successfully, endpoints={item}, clientId={ClientId}");
                    if (Isolated.TryRemove(item, out _))
                    {
                        Logger.Info(
                            $"Rejoin endpoints which was isolate before, endpoints={item}, clientId={ClientId}");
                    }

                    return;
                }

                var statusMessage = response.Status.Message;
                Logger.Info(
                    $"Failed to send heartbeat, endpoints={item}, code={code}, statusMessage={statusMessage}, clientId={ClientId}");
            }
        }


        public grpc.Metadata Sign()
        {
            var metadata = new grpc::Metadata();
            Signature.Sign(this, metadata);
            return metadata;
        }

        public async void NotifyClientTermination(Proto.Resource group)
        {
            var endpoints = GetTotalRouteEndpoints();
            var request = new Proto::NotifyClientTerminationRequest
            {
                Group = group
            };
            foreach (var item in endpoints)
            {
                var response = await ClientManager.NotifyClientTermination(item, request, ClientConfig.RequestTimeout);
                try
                {
                    StatusChecker.Check(response.Status, request);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed to notify client's termination, clientId=${ClientId}, " +
                                    $"endpoints=${item}");
                }
            }
        }

        public CancellationTokenSource TelemetryCts()
        {
            return _telemetryCts;
        }

        public abstract Settings GetSettings();

        public string GetClientId()
        {
            return ClientId;
        }

        public ClientConfig GetClientConfig()
        {
            return ClientConfig;
        }

        public void OnRecoverOrphanedTransactionCommand(Endpoints endpoints,
            Proto.RecoverOrphanedTransactionCommand command)
        {
        }

        public void OnVerifyMessageCommand(Endpoints endpoints, Proto.VerifyMessageCommand command)
        {
        }

        public void OnPrintThreadStackTraceCommand(Endpoints endpoints, Proto.PrintThreadStackTraceCommand command)
        {
        }

        public void OnSettingsCommand(Endpoints endpoints, Proto.Settings settings)
        {
            GetSettings().Sync(settings);
        }
    }
}