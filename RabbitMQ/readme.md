# Learning RabbitMQ

All the stuff I tried while learning up RabbitMQ. I have tried my best to keep the
unrelated console UI code away in the samples as much as possible so that the key
items of RabbitMQ can be observed.

## Note

Everything is written for .net core and is using the RabbitMQ.Client nuget library.

## RabbitMQ Installation

Unfortunatly I did not use docker to install RabbitMQ and so, I did not include any
docker compose file. Installation of RabbitMQ is pretty easy. Just search the 
Internet for RabbitMQ installation guides. 

## Samples

1. **Basic**                    - Getting started with writting a Producer and Consumer.
2. **Round-robin**              - Round-robin message dispatch to 2 or more Consumers.
3. **Manual.AckNack**           - Manually acknowledging messages.
4. **Requeue**                  - Requeuing messages that have been negatively acknowledged.
5. **Priority**                 - Setting and reading messages with priorities.
6. **Priority.Requeue**         - Requeuing messages with priorities.
7. **Exchange.Fanout**          - Demonstrating Fanout exchange.
8. **Exchange.Direct**          - Demonstrating Direct exchange.
9. **Exchange.Topic**           - Demonstrating Topic exchange.
10. **Exchange.Headers**        - Demonstrating Headers exchange.
11. **DeadLetter**              - Dead lettering using Direct exchange.
12. **DeadLetter.Headers**      - Dead lettering using Headers exchange.
13. **Poison.Messages**         - Handling poison messages in dead-letter queues.
14. **Rpc**                     - Using RabbitMQ Rpc features.
15. **Custom.Classes**          - Creating custom classes to make the implementation cleaner.

## Disclaimer

Everything is just based on my own self learning and understanding. Please pardon any mistakes
and if you are taking these as a learning source, do be informed you are doing it at your own
risks. I will not be responsible if your code crashes and burn :p