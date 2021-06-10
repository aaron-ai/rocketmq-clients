package org.apache.rocketmq.client.remoting;

import apache.rocketmq.v1.AckMessageRequest;
import apache.rocketmq.v1.AckMessageResponse;
import apache.rocketmq.v1.EndTransactionRequest;
import apache.rocketmq.v1.EndTransactionResponse;
import apache.rocketmq.v1.HealthCheckRequest;
import apache.rocketmq.v1.HealthCheckResponse;
import apache.rocketmq.v1.HeartbeatRequest;
import apache.rocketmq.v1.HeartbeatResponse;
import apache.rocketmq.v1.MessagingServiceGrpc;
import apache.rocketmq.v1.NackMessageRequest;
import apache.rocketmq.v1.NackMessageResponse;
import apache.rocketmq.v1.PollOrphanTransactionRequest;
import apache.rocketmq.v1.PollOrphanTransactionResponse;
import apache.rocketmq.v1.QueryAssignmentRequest;
import apache.rocketmq.v1.QueryAssignmentResponse;
import apache.rocketmq.v1.QueryRouteRequest;
import apache.rocketmq.v1.QueryRouteResponse;
import apache.rocketmq.v1.ReceiveMessageRequest;
import apache.rocketmq.v1.ReceiveMessageResponse;
import apache.rocketmq.v1.SendMessageRequest;
import apache.rocketmq.v1.SendMessageResponse;
import com.google.common.util.concurrent.ListenableFuture;
import io.grpc.ManagedChannel;
import io.grpc.netty.shaded.io.grpc.netty.GrpcSslContexts;
import io.grpc.netty.shaded.io.grpc.netty.NettyChannelBuilder;
import io.grpc.netty.shaded.io.netty.handler.ssl.SslContext;
import io.grpc.netty.shaded.io.netty.handler.ssl.SslContextBuilder;
import io.grpc.netty.shaded.io.netty.handler.ssl.util.InsecureTrustManagerFactory;
import io.grpc.stub.StreamObserver;
import java.net.InetSocketAddress;
import java.util.List;
import java.util.concurrent.Executor;
import java.util.concurrent.TimeUnit;
import javax.net.ssl.SSLException;
import lombok.EqualsAndHashCode;
import lombok.extern.slf4j.Slf4j;
import org.apache.rocketmq.client.producer.OrphanTransactionCallback;

/**
 * A typical implementation for {@link RpcClient}
 */
@Slf4j
@EqualsAndHashCode
public class RpcClientImpl implements RpcClient {

    private final ManagedChannel channel;

    private final MessagingServiceGrpc.MessagingServiceBlockingStub blockingStub;
    private final MessagingServiceGrpc.MessagingServiceStub asyncStub;
    private final MessagingServiceGrpc.MessagingServiceFutureStub futureStub;

    public RpcClientImpl(RpcTarget rpcTarget, CredentialsObservable credentialsObservable) throws SSLException {
        final SslContextBuilder builder = GrpcSslContexts.forClient();
        // TODO: discard the insecure way.
        builder.trustManager(InsecureTrustManagerFactory.INSTANCE);
        SslContext sslContext = builder.build();

        final Endpoints endpoints = rpcTarget.getEndpoints();
        final NettyChannelBuilder channelBuilder =
                NettyChannelBuilder.forTarget(endpoints.getTarget())
                                   .intercept(new HeadersClientInterceptor(credentialsObservable))
                                   .sslContext(sslContext);

        if (rpcTarget.isAutoRetryEnabled()) {
            channelBuilder.enableRetry();
        } else {
            channelBuilder.disableRetry();
        }

        final List<InetSocketAddress> socketAddresses = endpoints.convertToSocketAddresses();
        if (null != socketAddresses) {
            final IpNameResolverFactory ipNameResolverFactory = new IpNameResolverFactory(socketAddresses);
            channelBuilder.nameResolverFactory(ipNameResolverFactory);
        }

        this.channel = channelBuilder.build();

        this.blockingStub = MessagingServiceGrpc.newBlockingStub(channel);
        this.asyncStub = MessagingServiceGrpc.newStub(channel);
        this.futureStub = MessagingServiceGrpc.newFutureStub(channel);
    }

    @Override
    public void shutdown() {
        if (null != channel) {
            channel.shutdown();
        }
    }

    @Override
    public SendMessageResponse sendMessage(SendMessageRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).sendMessage(request);
    }

    @Override
    public ListenableFuture<SendMessageResponse> sendMessage(
            SendMessageRequest request, Executor executor, long duration, TimeUnit unit) {
        return futureStub.withExecutor(executor).withDeadlineAfter(duration, unit).sendMessage(request);
    }

    @Override
    public QueryAssignmentResponse queryAssignment(
            QueryAssignmentRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).queryAssignment(request);
    }

    @Override
    public HealthCheckResponse healthCheck(HealthCheckRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).healthCheck(request);
    }

    @Override
    public ListenableFuture<ReceiveMessageResponse> receiveMessage(ReceiveMessageRequest request, Executor executor,
                                                                   long duration, TimeUnit unit) {
        return futureStub.withExecutor(executor).withDeadlineAfter(duration, unit).receiveMessage(request);
    }

    @Override
    public AckMessageResponse ackMessage(AckMessageRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).ackMessage(request);
    }

    @Override
    public ListenableFuture<AckMessageResponse> ackMessage(
            AckMessageRequest request, Executor executor, long duration, TimeUnit unit) {
        return futureStub.withExecutor(executor).withDeadlineAfter(duration, unit).ackMessage(request);
    }

    @Override
    public NackMessageResponse nackMessage(
            NackMessageRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).nackMessage(request);
    }

    @Override
    public ListenableFuture<NackMessageResponse> nackMessage(
            NackMessageRequest request, Executor executor, long duration, TimeUnit unit) {
        return futureStub.withExecutor(executor).withDeadlineAfter(duration, unit).nackMessage(request);
    }

    @Override
    public HeartbeatResponse heartbeat(HeartbeatRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).heartbeat(request);
    }

    @Override
    public QueryRouteResponse queryRoute(QueryRouteRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).queryRoute(request);
    }

    @Override
    public EndTransactionResponse endTransaction(EndTransactionRequest request, long duration, TimeUnit unit) {
        return blockingStub.withDeadlineAfter(duration, unit).endTransaction(request);
    }

    @Override
    public void pollOrphanTransaction(PollOrphanTransactionRequest request, final OrphanTransactionCallback callback,
                                      long duration, TimeUnit unit) {
        final StreamObserver<PollOrphanTransactionResponse> responseStreamObserver =
            new StreamObserver<PollOrphanTransactionResponse>() {

                @Override
                public void onNext(PollOrphanTransactionResponse response) {
                    callback.onOrphanTransaction(response);
                }

                @Override
                public void onError(Throwable t) {
                    callback.onError(t);
                }

                @Override
                public void onCompleted() {
                }
            };

        final StreamObserver<PollOrphanTransactionRequest> requestStreamObserver =
                asyncStub.withDeadlineAfter(duration, unit).pollOrphanTransaction(responseStreamObserver);
        requestStreamObserver.onNext(request);
        requestStreamObserver.onCompleted();
    }
}
