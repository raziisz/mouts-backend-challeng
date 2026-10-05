[Back to README](../README.md)


### Authentication

#### POST /api/auth/login
- Description: Authenticate a user
- The email is the canonical authentication identifier and is required for login. The `username` field remains part of the user profile and is not accepted as a login identifier.
- Request Body:
  ```json
  {
    "email": "string",
    "password": "string"
  }
  ```
- Response: 
  ```json
  {
    "token": "string"
  }
  ```

<br/>
<div style="display: flex; justify-content: space-between;">
  <a href="./users-api.md">Previous: Users API</a>
  <a href="./project-structure.md">Next: Project Structure</a>
</div>
