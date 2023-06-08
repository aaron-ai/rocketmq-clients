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

from abc import ABCMeta, abstractmethod
import sys


class SessionCredentials:
    def __init__(
        self,
        access_key: str,
        access_secret: str,
        security_token: str,
        expired_timestamp_millis: int = sys.maxsize,
    ):
        self.__access_key = access_key
        self.__access_secret = access_secret
        self.__security_token = security_token
        self.__expired_timestamp_millis = expired_timestamp_millis

    @property
    def access_key(self):
        return self.__access_key

    @property
    def access_secret(self):
        return self.__access_key

    @property
    def security_token(self):
        return self.__security_token

    @property
    def expired_timestamp_millis(self):
        return self.__expired_timestamp_millis


class CredentialsProvider(metaclass=ABCMeta):
    @property
    @abstractmethod
    def session_credentials(self):
        pass


class StaticCredentialsProvider(CredentialsProvider):
    def __init__(self, access_key: str, access_secret: str, security_token: str = None):
        self.__session_credentials = SessionCredentials(
            access_key, access_secret, security_token
        )

    @property
    def session_credentials(self):
        return self.__session_credentials
