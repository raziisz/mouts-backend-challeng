[Back to README](../README.md)

### Users

`email` is the canonical authentication identifier and must be unique. `username` remains a required profile field and must also be unique. User updates validate all profile, contact, address, geolocation, status and role fields before persisting changes.

All user endpoints require the `Admin` role. User passwords are supplied only
when creating a user; authentication uses the user's email through the Auth API.

#### GET /api/users
- Description: Retrieve a list of all users
- Query Parameters:
  - `_page` (optional): Page number for pagination (default: 1)
  - `_size` (optional): Number of items per page (default: 10, maximum: 100)
  - `_order` (optional): Ordering of results (e.g., "username asc, email desc")
  - `username` (optional): Filter by username; supports partial matching with `*`
  - `email` (optional): Filter by email; supports partial matching with `*`
  - `phone` (optional): Filter by phone; supports partial matching with `*`
  - `status` (optional): Filter by status (`Active`, `Inactive`, `Suspended`)
  - `role` (optional): Filter by role (`Customer`, `Manager`, `Admin`)
- Response: 
  ```json
  {
    "data": [
      {
        "id": "integer",
        "email": "string",
        "username": "string",
        "name": {
          "firstname": "string",
          "lastname": "string"
        },
        "address": {
          "city": "string",
          "street": "string",
          "number": "integer",
          "zipcode": "string",
          "geolocation": {
            "lat": "string",
            "long": "string"
          }
        },
        "phone": "string",
        "status": "string (enum: Active, Inactive, Suspended)",
        "role": "string (enum: Customer, Manager, Admin)"
      }
    ],
    "totalItems": "integer",
    "currentPage": "integer",
    "totalPages": "integer"
  }
  ```

#### POST /api/users
- Description: Add a new user
- Request Body:
  ```json
  {
    "email": "string",
    "username": "string",
    "password": "string",
    "name": {
      "firstname": "string",
      "lastname": "string"
    },
    "address": {
      "city": "string",
      "street": "string",
      "number": "integer",
      "zipcode": "string",
      "geolocation": {
        "lat": "string",
        "long": "string"
      }
    },
    "phone": "string",
    "status": "string (enum: Active, Inactive, Suspended)",
    "role": "string (enum: Customer, Manager, Admin)"
  }
  ```
- Response: 
  ```json
  {
    "id": "integer",
    "email": "string",
    "username": "string",
    "name": {
      "firstname": "string",
      "lastname": "string"
    },
    "address": {
      "city": "string",
      "street": "string",
      "number": "integer",
      "zipcode": "string",
      "geolocation": {
        "lat": "string",
        "long": "string"
      }
    },
    "phone": "string",
    "status": "string (enum: Active, Inactive, Suspended)",
    "role": "string (enum: Customer, Manager, Admin)"
  }
  ```

#### GET /api/users/{id}
- Description: Retrieve a specific user by ID
- Path Parameters:
  - `id`: User ID
- Response: 
  ```json
  {
    "id": "integer",
    "email": "string",
    "username": "string",
    "name": {
      "firstname": "string",
      "lastname": "string"
    },
    "address": {
      "city": "string",
      "street": "string",
      "number": "integer",
      "zipcode": "string",
      "geolocation": {
        "lat": "string",
        "long": "string"
      }
    },
    "phone": "string",
    "status": "string (enum: Active, Inactive, Suspended)",
    "role": "string (enum: Customer, Manager, Admin)"
  }
  ```

#### PUT /api/users/{id}
- Description: Update a specific user
- Path Parameters:
  - `id`: User ID
- Request Body:
  ```json
  {
    "email": "string",
    "username": "string",
    "name": {
      "firstname": "string",
      "lastname": "string"
    },
    "address": {
      "city": "string",
      "street": "string",
      "number": "integer",
      "zipcode": "string",
      "geolocation": {
        "lat": "string",
        "long": "string"
      }
    },
    "phone": "string",
    "status": "string (enum: Active, Inactive, Suspended)",
    "role": "string (enum: Customer, Manager, Admin)"
  }
  ```
- Response: 
  ```json
  {
    "id": "integer",
    "email": "string",
    "username": "string",
    "name": {
      "firstname": "string",
      "lastname": "string"
    },
    "address": {
      "city": "string",
      "street": "string",
      "number": "integer",
      "zipcode": "string",
      "geolocation": {
        "lat": "string",
        "long": "string"
      }
    },
    "phone": "string",
    "status": "string (enum: Active, Inactive, Suspended)",
    "role": "string (enum: Customer, Manager, Admin)"
  }
  ```

#### DELETE /api/users/{id}
- Description: Delete a specific user
- Path Parameters:
  - `id`: User ID
- Response: 
  ```json
  {
    "id": "integer",
    "email": "string",
    "username": "string",
    "name": {
      "firstname": "string",
      "lastname": "string"
    },
    "address": {
      "city": "string",
      "street": "string",
      "number": "integer",
      "zipcode": "string",
      "geolocation": {
        "lat": "string",
        "long": "string"
      }
    },
    "phone": "string",
    "status": "string (enum: Active, Inactive, Suspended)",
    "role": "string (enum: Customer, Manager, Admin)"
  }
  ```
<br/>
<div style="display: flex; justify-content: space-between;">
  <a href="./sales-api.md">Previous: Sales API</a>
  <a href="./auth-api.md">Next: Auth API</a>
</div>
