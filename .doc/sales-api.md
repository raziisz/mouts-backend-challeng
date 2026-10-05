[Back to README](../README.md)

### Sales

All sales endpoints require the `Admin` or `Manager` role. The `customer` field
is an external identity belonging to the sale and is not the authenticated API
user. Customers do not access the Sales API.

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
        "products": [
          {
            "id": "integer",
            "productId": "integer",
            "productDescription": "string",
            "quantity": "integer",
            "unitPrice": "number",
            "discountRate": "number",
            "discountAmount": "number",
            "totalAmount": "number",
            "isCancelled": "boolean"
          }
        ]
      }
    ],
    "totalItems": "integer",
    "currentPage": "integer",
    "totalPages": "integer"
  }
  ```

#### POST /api/sales

- Description: Create a sale
- Permissions: `Admin` or `Manager`
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
- Permissions: `Admin` or `Manager`
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

Each sale item contains the original unit price and the calculated discount.
The `discountRate` is `0`, `0.10` or `0.20`, according to the quantity rules:

- Fewer than 4 items: no discount.
- From 4 to 9 items: 10% discount.
- From 10 to 20 items: 20% discount.

`discountAmount` is the discount applied to the item's gross amount, and
`totalAmount` is the item's final amount after the discount. When an item is
cancelled, `isCancelled` is `true` and it is excluded from the sale's
`totalAmount`.

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./carts-api.md">Previous: Carts API</a>
  <a href="./users-api.md">Next: Users API</a>
</div>
