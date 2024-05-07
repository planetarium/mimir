# mimir
A backend service that provides 9c-related utility APIs.

## Build

```
dotnet tool restore
dotnet graphql generate Mimir
dotnet graphql generate Mimir.Worker
dotnet build
```
