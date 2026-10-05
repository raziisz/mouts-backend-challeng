[Back to README](../README.md)

### Carts

All cart endpoints require authentication. Customers can only create, read,
update and delete their own carts. Admins and Managers can access carts across
users.

#### GET /api/carts
- Description: Retrieve a list of all carts
- Query Parameters:
  - `_page` (optional): Page number for pagination (default: 1)
  - `_size` (optional): Number of items per page (default: 10, maximum: 100)
  - `_order` (optional): Ordering of results (e.g., "id desc, userId asc")
  - `userId` (optional): Filter by user ID; ignored for non-privileged users
  - `date` (optional): Filter by exact date
  - `_minDate` (optional): Minimum date
  - `_maxDate` (optional): Maximum date
- Response: 
  ```json
  {
    "data": [
      {
        "id": "integer",
        "userId": "integer",
        "date": "string (date)",
        "products": [
          {
            "productId": "integer",
            "quantity": "integer"
          }
        ]
      }
    ],
    "totalItems": "integer",
    "currentPage": "integer",
    "totalPages": "integer"
  }
  ```

#### POST /api/carts
- Description: Add a new cart
- Request Body:
  ```json
  {
    "userId": "integer",
    "date": "string (date)",
    "products": [
      {
        "productId": "integer",
        "quantity": "integer"
      }
    ]
  }
  ```
- Response: 
  ```json
  {
    "id": "integer",
    "userId": "integer",
    "date": "string (date)",
    "products": [
      {
        "productId": "integer",
        "quantity": "integer"
      }
    ]
  }
  ```

#### GET /api/carts/{id}
- Description: Retrieve a specific cart by ID
- Path Parameters:
  - `id`: Cart ID
- Response: 
  ```json
  {
    "id": "integer",
    "userId": "integer",
    "date": "string (date)",
    "products": [
      {
        "productId": "integer",
        "quantity": "integer"
      }
    ]
  }
  ```

#### PUT /api/carts/{id}
- Description: Update a specific cart
- Path Parameters:
  - `id`: Cart ID
- Request Body:
  ```json
  {
    "userId": "integer",
    "date": "string (date)",
    "products": [
      {
        "productId": "integer",
        "quantity": "integer"
      }
    ]
  }
  ```
- Response: 
  ```json
  {
    "id": "integer",
    "userId": "integer",
    "date": "string (date)",
    "products": [
      {
        "productId": "integer",
        "quantity": "integer"
      }
    ]
  }
  ```

#### DELETE /api/carts/{id}
- Description: Delete a specific cart
- Path Parameters:
  - `id`: Cart ID
- Response: 
  ```json
  {
    "message": "string"
  }
  ```


<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./products-api.md">Previous: Products API</a>
  <a href="./sales-api.md">Next: Sales API</a>
</div>
