[Back to README](../README.md)

## Project Structure

The repository contains the backend application at the repository root:

```
root/
├── .doc/                         # Project and API documentation
├── src/                          # Production code
│   ├── Ambev.DeveloperEvaluation.Application/
│   ├── Ambev.DeveloperEvaluation.Common/
│   ├── Ambev.DeveloperEvaluation.Domain/
│   ├── Ambev.DeveloperEvaluation.IoC/
│   ├── Ambev.DeveloperEvaluation.ORM/
│   └── Ambev.DeveloperEvaluation.WebApi/
├── tests/                        # Automated tests
│   ├── Ambev.DeveloperEvaluation.Unit/
│   ├── Ambev.DeveloperEvaluation.Integration/
│   └── Ambev.DeveloperEvaluation.Functional/
├── docker-compose.yml            # Development stack
├── docker-compose.test.yml       # Isolated integration database
├── Ambev.DeveloperEvaluation.sln
└── README.md
```

The HTTP integration tests are located in
`tests/Ambev.DeveloperEvaluation.Functional` and run against
the HTTP API exposed by the development Compose stack.
