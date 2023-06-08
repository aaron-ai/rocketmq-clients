# Licensed to the Apache Software Foundation (ASF) under one or more
# contributor license agreements.  See the NOTICE file distributed with
# this work for additional information regarding copyright ownership.
# The ASF licenses this file to You under the Apache License, Version 2.0
# (the "License"); you may not use this file except in compliance with
# the License.  You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from rocketmq.client_config import ClientConfig
from rocketmq.client_id_encoder import ClientIdEncoder
from rocketmq.log import logger
from rocketmq.topic_route_data import TopicRouteData

from apscheduler.executors.pool import ThreadPoolExecutor, ProcessPoolExecutor
from multiprocessing import cpu_count


class Client:
    def __init__(self, client_config: ClientConfig):
        self._client_id = ClientIdEncoder.generate()
        self.__client_config = client_config
        self.__endpoints = client_config.endpoints
        self.__topic_route_cache = {}
        logger.info(f"Client was created, client_id={self._client_id}")

    @property
    def client_config(self):
        return self.__client_config

    async def __fetch_topic_route(self, topic: str) -> TopicRouteData:
        pass

    async def __fetch_topic_route0(self, topic: str):
        pass

    async def __update_route_cache(self):
        logger.info(
            f"Start to update route cache for a new round, client_id={self._client_id}"
        )
        for topic in self.__topic_route_cache.keys():
            topic_route_data = await self.__fetch_topic_route(topic)
