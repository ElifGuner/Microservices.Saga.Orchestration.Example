---------------------------------------------------------------------
		Saga Orchestration Pattern
					
			Elif Güner
							
		       28.02.2024
---------------------------------------------------------------------					

Saga Orchestration Pattern is implemented in this project.
3 WEBAPIs are used: OrderAPI, StockAPI, PaymentAPI
Also, a state machine called OrderStateMachine is managing the events.
MassTransit library, which uses RabbitMQ as message brocker, is used for asynchronous communication between WEBAPIs.
Entity Framework Core is used for MSSQL DB operations.

---------------------------------------------------------------------

1. Execute 'update-database' EF Core command should be executed on OrderAPI's Package Manager Console screen
   in order to create the tables:
      -  Orders
	  -  OrderItems
   in DB:
      -  SagaStateMachine_OrderAPIDB
	     
	  
2. Execute 'update-database' EF Core command should be executed on StockAPI's Package Manager Console screen
   in order to create the tables:
      -  Stocks
   in DB:
      -  SagaStateMachine_StockAPIDB
   with the following seed data:

		Id          ProductId   Count
		----------- ----------- -----------
		1           1           196
		2           2           294
		3           3           50
		4           4           20
		5           5           60

3. Execute 'update-database' EF Core command should be executed on SagaStateMachine.Service's Package Manager Console screen
   in order to create the tables:
	  -  OrderStateInstance : This table is used by State Machine in order to track the state of each order. 
   in DB:
      -  SagaStateMachineDB
   
4. When the project is started, SwaggerUI is opened for sending POST message to OrderAPI.

   Example message:
   
  {
  "buyerId": 1,
  "orderItems": [
    {
      "productId": 1,
      "count": 2,
      "price": 3
    },
    {
      "productId": 4,
      "count": 5,
      "price": 6
    }
  ]
}

5. Order is created at OrderAPI with the orders that comes via SwaggerUI message.
   OrderStartedEvent is created and published.
   
6. State Machine listens the queue : "state-machine-queue".
   OrderStartedEvent is the initial event for the OrderStateMachine and correlationId is generated.
   correlationId is written to the SagaStateMachineDB.OrderStateInstance table.
   Also, OrderInstance's state is transitioned to OrderCreated state.
   OrderCreatedEvent is created on the State Machine and sent to the queue "stock-order-created-event-queue"
   with the corresponding correlationId.
 
7. StockAPI consumes the queue "stock-order-created-event-queue" for OrderCreatedEvent.
   It captures the event, checks the SagaStateMachine_StockAPIDB.Stocks table if there enough stocks for
   the products that are in the order message.
   
8. If the stocks of the products are enough, Stocks.Count fields are decreased by the counts 
   in the Order message. stockReservedEvent is created and sent to the State Machine.
    
   If the stocks of the products are not enough, stockNotReservedEvent is created and sent to the State Machine.

9. When the OrderStateInstance is in OrderCreated state, if the stockReservedEvent is received by the State Machine,
   state is changed to StockReserved. PaymentStartedEvent is generated with the corresponding correlationId and 
   sent to the queue "payment-started-event-queue".
   
   When the OrderStateInstance is in OrderCreated state, if the stockNotReservedEvent is received by the State Machine,
   state is changed to StockNotReserved. OrderFailedEvent is generated and sent to the 
   queue "order-order-failed-event-queue".

10. PaymentAPI consumes PaymentStartedEvent, assumes that Payment Processes are successful and
    creates paymentCompletedEvent by default and sends this event.
   
    If Payment Processes are failed, than paymentFailedEvent is created and published.

11. When the OrderStateInstance is in StockReserved state, if the PaymentCompletedEvent is received by the State Machine,
    state is changed to PaymentCompleted. OrderCompletedEvent is generated and sent to the queue "order-order-completed-event-queue".
   
    When the OrderStateInstance is in StockReserved state, if the PaymentFailedEvent is received by the State Machine,
    state is changed to PaymentFailed. OrderFailedEvent is generated and sent to the queue "order-order-failed-event-queue".
    StockRollbackMessage is generated and sent to the queue "stock-rollback-message-queue"

12. If the transaction is successful, then, OrderStateInstance is deleted from the DB.
    If the transaction is fails, then, OrderStateInstance is not deleted from the DB for tracking purposes.
