# DC-JobContextManager

The Job Context Manager acts as the interface between the Service Fabric (Service Bus) infrastructure and the hosting services' logic. Job Status, Auditing, error reporting and retry logic are handled for the developer.

Interface and implementation provided as separate packages.

To use the following dependencies are required:
- IMapper<JobContextMessage, [YourServiceMessageType T]>
- IQueueSubscriptionService\<JobContextMessage>
- IQueuePublishService\<JobContextMessage>
- IAuditor
- ILogger

You will also need your own implementation of the following func in your service and make it the entry point of any processing for the topic.
```
Func<[YourServiceMessageType T], CancellationToken, Task<bool>>
```
This will pass the mapped message to your method, along with a cancellation token. The method must return true for the service to be considered as successfull. Returning false, or throwing an exception will cause the queued message to be rejected back to the queue. Returning true will cause the message topic pointer to be incremented and the message published to the next topic. Service starting and stopping is audited and tracked automatically by this library using the relevant libraries.