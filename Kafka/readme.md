# Learning Kafka

All the stuff I tried while learning up Kafka.

## Note

Everything is written for .net core and is using the Confluent.Kafka nuget library.

## Kafka Installation

If you do not have a kafka installation, there is a docker-compose.yml file provided for
you to get things up and running fast in a development environment.

## Samples

1. Basic                    - Getting started with writting a Producer and Consumer.
2. Multicast                - Multi-casting messages to 2 Consumers.
3. Partitions.Random        - Random assignment of messages to partitions.
4. Partitions.Round-robin   - Round-robin assignment of messages to partitions.
5. Partitions.Key           - Assigning messages to partitions by Key.
6. Manual.Commit            - Synchronous commit of messages.
7. Manual.Offset            - Manually determining when to store offsets.
8. Hosted                   - Consumer hosted in a .net core Worker.

## Disclaimer

Everything is just based on my own self learning and understanding. Please pardon any mistakes
and if you are taking these as a learning source, do be informed you are doing it at your own
risks. I will not be responsible if your code crashes and burn :p