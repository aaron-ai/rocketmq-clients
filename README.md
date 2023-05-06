# RocketMQ Clients - Collection of Client Bindings for Apache RocketMQ

[![License][license-image]][license-url] [![CPP][cpp-image]][cpp-url] [![C#][csharp-image]][csharp-url] [![Java][java-image]][java-url] [![Golang][golang-image]][golang-url] [![Rust][rust-image]][rust-url]

English | [简体中文](README-CN.md) | [RocketMQ Website](https://rocketmq.apache.org/)

## Overview

Client bindings for [Apache RocketMQ](https://rocketmq.apache.org/), as known as RocketMQ 5.x SDK. All of them follow the specification of [rocketmq-apis](https://github.com/apache/rocketmq-apis), replacing 4.x remoting-based counterparts. Clients in this repository are built on top of [Protocol Buffers](https://developers.google.com/protocol-buffers) and [gRPC](https://grpc.io/).

## Goal

Provide cloud-native and robust solutions for Java, C++, C#, Golang, Rust and all other mainstream programming languages.

## Features and Status

* Ready - ✅
* Work in progress - 🚧

| Feature                                        | Java  | C/C++ |  C#   | Golang | Rust  | Python | Node.js |  PHP  |
| ---------------------------------------------- | :---: | :---: | :---: | :----: | :---: | :----: | :-----: | :---: |
| Producer with standard messages                |   ✅   |   ✅   |   ✅   |   ✅    |   ✅   |   🚧    |    🚧    |   🚧   |
| Producer with FIFO messages                    |   ✅   |   ✅   |   ✅   |   ✅    |   ✅   |   🚧    |    🚧    |   🚧   |
| Producer with timed/delay messages             |   ✅   |   ✅   |   ✅   |   ✅    |   ✅   |   🚧    |    🚧    |   🚧   |
| Producer with transactional messages           |   ✅   |   ✅   |   ✅   |   ✅    |   🚧   |   🚧    |    🚧    |   🚧   |
| Simple consumer                                |   ✅   |   ✅   |   ✅   |   ✅    |   ✅   |   🚧    |    🚧    |   🚧   |
| Push consumer with concurrent message listener |   ✅   |   ✅   |   🚧   |   🚧    |   🚧   |   🚧    |    🚧    |   🚧   |
| Push consumer with FIFO message listener       |   ✅   |   ✅   |   🚧   |   🚧    |   🚧   |   🚧    |    🚧    |   🚧   |

## Prerequisite and Build

This project is organized as a monorepo, with each language's bindings containing specific instructions in their respective subdirectories for building the project. As the [rocketmq-apis](https://github.com/apache/rocketmq-apis) submodule is incorporated into the project and could be utilized by some bindings, we highly advise cloning the repository by executing the command below:

```sh
git clone --recursive git@github.com:apache/rocketmq-clients.git
```

## Contributing

Similar to other projects of Apache RocketMQ, any attempt to make this project better is welcome, including but not limited to filing a bug report, correcting type error or document writing to complete feature implementation. Do not hesitate to make a pull request if this project catches your attention.

## Related

* [rocketmq](https://github.com/apache/rocketmq): The implementation of server-side.
* [rocketmq-apis](https://github.com/apache/rocketmq-apis): Common communication protocol between server and client.
* [RIP-37: New and Unified APIs](https://shimo.im/docs/m5kv92OeRRU8olqX): RocketMQ proposal of new and unified APIs crossing different languages.
* [RIP-39: Support gRPC protocol](https://shimo.im/docs/gXqmeEPYgdUw5bqo): RocketMQ proposal of gRPC protocol support.

## License

[Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0.html) Copyright (C) Apache Software Foundation

[license-image]: https://img.shields.io/badge/license-Apache%202-4EB1BA.svg
[license-url]: https://www.apache.org/licenses/LICENSE-2.0.html
[cpp-image]: https://github.com/apache/rocketmq-clients/actions/workflows/cpp_build.yml/badge.svg
[cpp-url]: https://github.com/apache/rocketmq-clients/actions/workflows/cpp_build.yml
[csharp-image]: https://github.com/apache/rocketmq-clients/actions/workflows/csharp_build.yml/badge.svg
[csharp-url]: https://github.com/apache/rocketmq-clients/actions/workflows/csharp_build.yml
[java-image]: https://github.com/apache/rocketmq-clients/actions/workflows/java_build.yml/badge.svg
[java-url]: https://github.com/apache/rocketmq-clients/actions/workflows/java_build.yml
[golang-image]: https://github.com/apache/rocketmq-clients/actions/workflows/golang_build.yml/badge.svg
[golang-url]: https://github.com/apache/rocketmq-clients/actions/workflows/golang_build.yml
[rust-image]: https://github.com/apache/rocketmq-clients/actions/workflows/rust_build.yml/badge.svg
[rust-url]: https://github.com/apache/rocketmq-clients/actions/workflows/rust_build.yml
