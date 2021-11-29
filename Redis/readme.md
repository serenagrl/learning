# Learning Redis

Contains some of the stuff I tried while learning up Redis. I have tried my best to 
keep the code as simple as possible in the samples but sometimes I do get carried away
a little bit, so please pardon the mess.

## Note

Everything is written for .net 6 and is using the StackExchange.Redis nuget library.

## Redis Installation (Docker)

You can quickly get Redis up on docker with the following command in powershell

```
docker run -d --rm -p:6379:6379 --name redis redis:latest 
```

## Samples

1. **Caching.Basic**            - Caching in asp.net core web api.
3. **PubSub.Ordered**           - Publish & Subscribe with ordered messages.
4. **PubSub.Unordered**         - Publish & Subscribe with unordered messages.
5. **Queue.Basic**              - Basic Queue example.
6. **Queue.Blocking.Read**      - Simple Blocking Read solution for reading Queues.
7. **Streams.Basic**            - Basic Streams example.
8. **Streams.Multiple**         - Reading from multiple streams.
9. **Streams.Tail**             - Reading new messages only from streams.
10. **Streams.Group**           - Reading with Consumer Groups.
11. **Streams.MultiGroup**      - Multiple Consumer Groups example.
12. **Streams.Ack**             - Manual acknoledgement of pending messages.
13. **Streams.Claim**           - Using separate program to claim pending messages from Consumers.
14. **Streams.Partner**         - Using Consumers to claim pending messages from specific Consumers.
15. **Streams.Requeue**         - Requeue pending messages to be processed by any Consumers.
16. **Streams.Blocking.Read**   - Simple Blocking Read solution for reading streams.
17. **Streams.Auto**            - Complete example with automatic requeue and poison message handling.
18. **Streams.Trim**            - Trim streams length.

## Disclaimer

Everything is based on my own self learning and understanding. Please pardon any mistakes
and if you are taking these as a learning source, do be informed you are doing it at your own
risks. I will not be responsible if your code crashes and burn :p