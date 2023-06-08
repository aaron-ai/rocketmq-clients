from log import logger
from grpc import aio
from google.protobuf.json_format import MessageToJson


# noinspection PyProtectedMember
class LoggingInterceptor(
    aio.UnaryUnaryClientInterceptor,
    aio.UnaryStreamClientInterceptor,
    aio.StreamUnaryClientInterceptor,
    aio.StreamStreamClientInterceptor,
):
    async def intercept_unary_unary(self, continuation, call_details, request):
        logger.debug(f"Sending request: {MessageToJson(request)}")
        call = await continuation(call_details, request)
        response = await call._call_response
        logger.debug(f"Received call: {response}")
        return call

    async def intercept_unary_stream(self, continuation, call_details, request):
        logger.debug(f"Sending request: {request}")
        async for response in continuation(call_details, request):
            logger.debug(f"Received response: {response}")
            yield response

    async def intercept_stream_unary(
        self, continuation, call_details, request_iterator
    ):
        logger.debug(f"Sending stream request")
        response = await continuation(call_details, request_iterator)
        logger.debug(f"Received response: {response}")
        return response

    async def intercept_stream_stream(
        self, continuation, call_details, request_iterator
    ):
        logger.debug(f"Sending stream request")
        async for response in continuation(call_details, request_iterator):
            logger.debug(f"Received response: {response}")
            yield response
