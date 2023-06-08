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

from rocketmq.send_receipt import SendReceipt
from rocketmq.message import Message
from rocketmq.client_config import ClientConfig
from rocketmq.log import logger
from rocketmq.client import Client


class Producer(Client):
    def __init__(self, client_config: ClientConfig, max_attempts: int = 3):
        super().__init__(client_config)
        self.__client_config = client_config
        self.__max_attempts = max_attempts
        logger.info(f"Producer was created, client_id={self._client_id}")

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        pass

    def shutdown(self):
        logger.info(f"Producer was shutdown, client_id={self._client_id}")
        pass

    async def send(self, message: Message) -> SendReceipt:
        pass
