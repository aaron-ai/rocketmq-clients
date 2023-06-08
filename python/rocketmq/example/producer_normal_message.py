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

import asyncio
from rocketmq.client_config import ClientConfig
from rocketmq.session_credentials import StaticCredentialsProvider
from rocketmq.producer import Producer
from rocketmq.message import Message


async def send_normal_message():
    credentials_provider = StaticCredentialsProvider("yourAccessKey", "yourSecretKey")
    client_config = ClientConfig("foobar:8081", credentials_provider, True)
    topic = "yourNormalTopic"
    body = bytes("This is a NORMAL message of Apache RocketMQ", encoding="UTF-8")
    message = Message(topic, body)
    # producer = Producer(client_config)
    # send_receipt = await producer.send(message)
    with Producer(client_config) as producer:
        send_receipt = await producer.send(message)


if __name__ == "__main__":
    asyncio.run(send_normal_message())
