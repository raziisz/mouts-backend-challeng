[Back to README](../README.md)

### Sales

All sales endpoints require authentication. Customers can create sales only for
themselves and can list or retrieve only their own sales. Admins and Managers
can access all sales and can update or cancel them. Sale item cancellation is
also restricted to Admins and Managers.

#### GET /api/sales

- Description: Retrieve sales visible to the authenticated user
- Query Parameters:
  - `_page` (optional): Page number (default: 1)
  - `_size` (optional): Number of items per page (default: 10, maximum: 100)
  - `_order` (optional): Ordering of results (for example, `date desc`)
  - `saleNumber` (optional): Filter by sale number; supports partial matching with `*`
  - `status` (optional): Filter by status (`Active`, `Cancelled`)
  - `date` (optional): Filter by exact date
  - `_minDate` (optional): Minimum date
  - `_maxDate` (optional): Maximum date
- Response:
  ```json
  {
    "data": [
      {
        "id": "integer",
        "saleNumber": "string",
        "date": "string (date)",
        "customer": { "id": "integer", "description": "string" },
        "branch": { "id": "integer", "description": "string" },
        "status": "string (enum: Active, Cancelled)",
        "totalAmount": "number",
        "products": []
      }
    ],
    "totalItems": "integer",
    "currentPage": "integer",
    "totalPages": "integer"
  }
  ```

#### POST /api/sales

- Description: Create a sale
- Permissions: Authenticated users; Customers must use their own customer ID
- Request Body:
  ```json
  {
    "saleNumber": "string",
    "date": "string (date)",
    "customer": { "id": "integer", "description": "string" },
    "branch": { "id": "integer", "description": "string" },
    "products": [
      {
        "productId": "integer",
        "productDescription": "string",
        "unitPrice": "number",
        "quantity": "integer"
      }
    ]
  }
  ```
- Response: Created sale with HTTP `201 Created`.

#### GET /api/sales/{id}

- Description: Retrieve a sale by ID
- Permissions: Customers can retrieve only their own sale; Admins and Managers can retrieve any sale
- Path Parameters:
  - `id`: Sale ID
- Response: A sale object in the same format returned by the list endpoint.

#### PUT /api/sales/{id}

- Description: Replace a sale and its items
- Permissions: `Admin` or `Manager`
- Path Parameters:
  - `id`: Sale ID
- Request Body: Same format as `POST /api/sales`.
- Response: Updated sale object.

#### DELETE /api/sales/{id}

- Description: Cancel a sale and its items
- Permissions: `Admin` or `Manager`
- Path Parameters:
  - `id`: Sale ID
- Response: The cancelled sale object with `status` equal to `Cancelled`.

#### PATCH /api/sales/{saleId}/items/{itemId}/cancel

- Description: Cancel a single sale item
- Permissions: `Admin` or `Manager`
- Path Parameters:
  - `saleId`: Sale ID
  - `itemId`: Sale item ID
- Response: The sale with the selected item marked as cancelled.

Sale quantities must be positive and cannot exceed 20 identical items. Invalid
payloads return `400 ValidationError`; domain rule violations return
`409 BusinessRuleViolation`. Other standard responses are described in the
[General API documentation](./general-api.md).

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./carts-api.md">Previous: Carts API</a>
  <a href="./users-api.md">Next: Users API</a>
</div>
