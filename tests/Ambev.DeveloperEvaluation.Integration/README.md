# Integration test environment

The integration suite uses a dedicated PostgreSQL database and never connects to the development database by default.

Start the test database from the repository root:

```powershell
docker compose -f docker-compose.test.yml up -d
```

Run the integration tests from the repository root:

```powershell
dotnet test .\tests\Ambev.DeveloperEvaluation.Integration\Ambev.DeveloperEvaluation.Integration.csproj --no-restore
```

The default connection is `localhost:5433/developer_evaluation_test`. Override it with `INTEGRATION_TEST_CONNECTION_STRING` when needed.
