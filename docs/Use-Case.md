## Use Case
The use case for demo purposes used here is explained as follows. The system we are designing is an e-commerce discount calculation system. 

### Rules
The rules for the discount calculation are –

1.	Give the user a discount of 10% over MRP if the following conditions are followed – 
    * The user’s country is India.
    * The user’s loyalty factor is less than or equal to 2.
    * All the orders purchased by the user so far should amount to more than 5,000.
    * User should have at least made more than two successful orders. 
    * The user should have visited the site more than two times every month.
2.	Give the user a discount of 20% over MRP if the following conditions are followed – 
    * The user’s country is India.
    * The user’s loyalty factor is equal to 3.
    * All the orders purchased by the user so far should amount to more than 10,000.
    * User should have at least made more than two successful orders. 
    * The user should have visited the site more than two times every month.
3.	Give the user a discount of 25% over MRP if the following conditions are followed – 
    * The user’s country is not India.
    * The user’s loyalty factor is greater than or equal to 2.
    * All the orders purchased by the user so far should amount to more than 10,000.
    * User should have at least made more than two successful orders. 
    * The user should have visited the site more than five times every month.
4.	Give the user a discount of 30% over MRP if the following conditions are followed – 
    * The user’s loyalty factor is greater than 3.
    * All the orders purchased by the user so far should amount to more than 50,000 but less than 100,000.
    * User should have at least made more than five successful orders. 
    * The user should have visited the site more than fifteen times every month.
5.	Give the user a discount of 30% over MRP if the following conditions are followed – 
    * The user’s loyalty factor is greater than 3.
    * All the orders purchased by the user so far should amount to more than 100,000.
    * User should have at least made more than fifteen successful orders. 
    * The user should have visited the site more than 25 times every month.
6.	Give 0% discount in any other case.

### Inputs
Here the inputs will be of three different types as they are coming from three different data sources/APIs. 
#### User Basic Info
This input has information like –
* Name
* Country
* Email
* Credit history
* Loyalty factor
* Sum of the purchases made by the user till date.

#### Users Order Information
This input is a summarization of the orders made by the user so far. This input has information like – 
* Total number of orders
* Recurring items in those orders if any

#### Users Telemetry Information
This input is a summarization of the telemetry information collected based on the user’s visit to the site. This input has information like – 
* Number of visits to the site per month
* Percentage of the number of times the user purchased something to the number of times the user visited
