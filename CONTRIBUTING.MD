
## Build

```sh
dotnet tool restore
dotnet graphql generate Mimir
dotnet graphql generate Mimir.Worker
dotnet build
```

## Test

```sh
dotnet test
```
