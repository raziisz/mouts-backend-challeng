[Back to README](../README.md)

## General API Definitions

### Health Checks

Health check endpoints are anonymous and can be used by container orchestration
or monitoring systems:

- `GET /health`: Overall application health. Includes all registered checks.
- `GET /health/live`: Liveness probe. Confirms that the application process is running.
- `GET /health/ready`: Readiness probe. Confirms that the application is ready to receive traffic.

Healthy and degraded checks return HTTP `200 OK`. An unhealthy check returns
HTTP `503 Service Unavailable`.

Response format:

```json
{
  "status": "Healthy",
  "healthChecks": [
    {
      "name": "Liveness",
      "status": "Healthy",
      "description": null,
      "errorMessage": null,
      "hostEnvironment": "development"
    }
  ]
}
```

### Swagger / OpenAPI

The interactive Swagger UI is enabled in the Development environment. When
running the application through Docker Compose, access it at:

`http://localhost:8080/swagger`

The API is intentionally configured for HTTP during development. Swagger is
not enabled by the application in non-Development environments.

### Pagination

Pagination is supported for list endpoints using the following query parameters:

- `_page`: Page number (default: 1)
- `_size`: Number of items per page (default: 10, maximum: 100)

Example:
```
GET /api/products?_page=2&_size=20
```

### Ordering

When requesting a collection of a resource, you can also specify the order of the elements in the collection using the query parameter `_order`. Simply indicate the desired order: ascending (`asc`) or descending (`desc`). If not specified, the default order will be ascending.

**Note**

In the GET request, you must use the field names in the same format as the JSON response.

For example, consider the following Product resource:

```json
{
  "id": 1,
  "title": "Fjallraven - Foldsack No. 1 Backpack, Fits 15 Laptops",
  "price": 109.95,
  "description": "Your perfect pack for everyday use and walks in the forest. Stash your laptop (up to 15 inches) in the padded sleeve, your everyday",
  "category": "men's clothing",
  "image": "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
  "rating": {
    "rate": 3.9,
    "count": 120
  }
}
```

In this case, to retrieve a list of products ordered by price in descending order and then by title in ascending order, the request would look like this:

```
GET /api/products?_order="price desc, title asc"
```

or 

```
GET /api/products?_order="price desc, title"
```

### Filtering

Filters can be applied to list endpoints using the following query parameters:

- `field=value`: Filter by specific field value.

Example:

```
GET /api/products?category=men's clothing&price=109.95
```

**String Fields**

To filter partial matches for string fields, use an asterisk (`*`) before or after the value.

Example:

```
GET /api/products?title=Fjallraven*
GET /api/products?category=*clothing
```

**Numeric and Date Fields**

To filter numeric or date fields by range, use `_min` and `_max` prefixes before the field name.

Example:

```
GET /api/products?_minPrice=50
GET /api/products?_minPrice=50&_maxPrice=200
GET /api/carts?_minDate=2023-01-01
```

Logical Operators
When combining filters, use `&` (AND) between them.

Example:

```
GET /api/products?category=men's clothing&_minPrice=50
GET /api/products?title=Fjallraven*&category=men's clothing&_minPrice=100
```

*Note*
Even when filtering with "or" for different values in the same field, use `&` in the query.

## Error Handling

The API uses conventional HTTP response codes to indicate the success or failure of an API request. In general:

- 2xx range indicate success
- 4xx range indicate an error that failed given the information provided (e.g., a required parameter was omitted, etc.)
- 5xx range indicate an error with our servers

### Error Response Format

```json
{
  "type": "string",
  "error": "string",
  "detail": "string"
}
```

- `type`: A machine-readable error type identifier
- `error`: A short, human-readable summary of the problem
- `detail`: A human-readable explanation specific to this occurrence of the problem

Example error responses:

1. Resource Not Found
```json
{
  "type": "ResourceNotFound",
  "error": "Resource not found",
  "detail": "Product 12345 was not found."
}
```

2. Authentication Error
```json
{
  "type": "AuthenticationError",
  "error": "Authentication failed",
  "detail": "A valid authentication token is required."
}
```

3. Authorization Error
```json
{
  "type": "AuthorizationError",
  "error": "Access denied",
  "detail": "You do not have permission to access this resource."
}
```

4. Validation Error
```json
{
  "type": "ValidationError",
  "error": "Invalid input data",
  "detail": "The 'price' field must be a positive number"
}
```

5. Internal Server Error
```json
{
  "type": "InternalServerError",
  "error": "Internal server error",
  "detail": "An unexpected error occurred while processing the request."
}
```

For detailed error information, refer to the specific endpoint documentation.

### Authorization

All endpoints except `POST /api/auth/login` require a valid JWT bearer token.

| Role | Permissions |
|---|---|
| Customer | Read products and create/manage own carts |
| Manager | Customer permissions plus full Sales API access and sale item cancellation |
| Admin | User CRUD, product CRUD, full catalog access and full Sales API access |

Requests without a valid token return `401 AuthenticationError`. Authenticated
users without the required role or ownership return `403 AuthorizationError`.

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./frameworks.md">Previous: Frameworks</a>
  <a href="./products-api.md">Next: Products API</a>
</div>
