# CLAUDE.md

Guidance for Claude Code / AI coding assistants working in this repository.

## Project overview

Mimir indexes Nine Chronicles blockchain state into MongoDB and exposes it through a
GraphQL API, so clients can query live chain data, rankings, and statistics far faster
than reading the chain directly. A worker continuously fetches state from a Nine
Chronicles Headless node and writes it to MongoDB; the GraphQL host reads from that
MongoDB. Most GraphQL types mirror lib9c's on-chain state models
(`Nekoyume.Model.State.*`) serialized into BSON.

See [README.md](README.md) and [CONTRIBUTING.md](CONTRIBUTING.md) for the user-facing
description and local setup. lib9c is the public game-logic/state library:
https://github.com/planetarium/lib9c (libplanet: https://github.com/planetarium/libplanet).

Public GraphQL endpoints (documented in the README):
- Odin: `https://mimir.nine-chronicles.dev/odin/graphql/`
- Heimdall: `https://mimir.nine-chronicles.dev/heimdall/graphql/`

## Architecture

```
Nine Chronicles chain (Headless GraphQL)  ->  Mimir.Worker  ->  MongoDB  ->  Mimir (GraphQL API)
```

- **Mimir.Worker** polls the chain and writes documents to MongoDB.
- **Mimir** serves GraphQL by reading those MongoDB collections.
- **MongoDB** is the single source the API reads from; one database per planet
  (`odin`, `heimdall`, `thor`).

## Module map

| Project | Role |
|---|---|
| [`Mimir/`](Mimir/) | GraphQL API host (ASP.NET Core + HotChocolate). Entry point [`Mimir/Program.cs`](Mimir/Program.cs). |
| [`Mimir.Worker/`](Mimir.Worker/) | Background service that syncs chain state into MongoDB. Entry point [`Mimir.Worker/Program.cs`](Mimir.Worker/Program.cs). |
| [`Mimir.MongoDB/`](Mimir.MongoDB/) | BSON document models, repositories, collection-name mapping, BSON serializers. |
| [`Mimir.Shared/`](Mimir.Shared/) | Cross-cutting pieces: `PlanetType`, options, Headless GraphQL client, state services. |
| [`Mimir.Initializer/`](Mimir.Initializer/) | One-off DB initialization / migration runner (e.g. seeding from snapshots). |
| [`Mimir.Scripts/`](Mimir.Scripts/) | Standalone maintenance / data-migration scripts. |
| [`Lib9c.Models/`](Lib9c.Models/) | lib9c state types modeled for BSON storage and GraphQL exposure. |
| [`Lib9c.GraphQL/`](Lib9c.GraphQL/) | GraphQL type definitions / scalars for the `Lib9c.Models` types. |
| `*.Tests/` | xUnit test projects: `Mimir.Tests`, `Mimir.Worker.Tests`, `Mimir.MongoDB.Tests`, `Lib9c.Models.Tests`, `Lib9c.GraphQL.Tests`. |

Solution file: [`Mimir.sln`](Mimir.sln).

## Target framework & shared versions

- `Mimir`, `Mimir.Worker`, `Mimir.Initializer`, `Mimir.Scripts`: **.NET 8** (`net8.0`).
- `Lib9c.Models`, `Lib9c.GraphQL`: `net6.0`.
- HotChocolate `14.1.0`, MongoDB.Driver `3.x`.
- lib9c / libplanet versions are pinned in
  [`Directory.Build.props`](Directory.Build.props) as `<Lib9cVersion>` and
  `<LibplanetVersion>`. Bump these together when upgrading models (see CONTRIBUTING.md
  "Bump Lib9c" / "Bump Libplanet").

## Build / test / run

```sh
# Build
dotnet build Mimir.sln

# Test (CI runs these per-project)
dotnet test Mimir.Tests
dotnet test Mimir.Worker.Tests
dotnet test Mimir.MongoDB.Tests
dotnet test Lib9c.Models.Tests

# Local MongoDB (+ Redis) for development
docker compose up -d   # see docker-compose.yml

# Run the GraphQL API (needs a populated MongoDB; copy appsettings.json -> appsettings.local.json)
ASPNETCORE_ENVIRONMENT=local dotnet run --project Mimir

# Run the worker (copy appsettings.<planet>-for-local.json -> appsettings.local.json)
WORKER_CONFIG_FILE=appsettings.local.json dotnet run --project Mimir.Worker
```

CI: [`.github/workflows/build.yaml`](.github/workflows/build.yaml) runs `dotnet test`
on `Mimir.Worker.Tests`, `Lib9c.Models.Tests`, `Mimir.MongoDB.Tests`, `Mimir.Tests`
(setup-dotnet 8.0.x). [`.github/workflows/docker.yaml`](.github/workflows/docker.yaml)
builds multi-arch images from [`Dockerfile`](Dockerfile) (API) and
[`Dockerfile.Worker`](Dockerfile.Worker) (worker).

### Configuration

- The worker reads config from the file named by `WORKER_CONFIG_FILE` (default
  `appsettings.json`) plus env vars prefixed `WORKER_`. Key fields are in
  [`Mimir.Worker/Configuration.cs`](Mimir.Worker/Configuration.cs): `PollerType`,
  `MongoDbConnectionString`, `PlanetType`, `HeadlessEndpoints`, `EnableInitializing`,
  JWT fields.
- The API reads the `Database` section (see
  [`Mimir.Shared/Options/DatabaseOption.cs`](Mimir.Shared/Options/DatabaseOption.cs):
  `ConnectionString`, `PlanetType`, `CAFile`) and other options under
  [`Mimir/Options/`](Mimir/Options/) (rate limit, JWT, Hangfire, Headless, WNCG price).
- When using the bundled `docker compose`, the Mongo connection string is
  `mongodb://rootuser:rootpass@localhost:27017`.
- MongoDB indexes can be created with [`scripts/create_index.sh`](scripts/create_index.sh)
  (uses [`scripts/indexes.json`](scripts/indexes.json), needs `mongosh` + `jq`).

## How sync works (Mimir.Worker)

The worker registers a set of background handlers selected by `PollerType`
(see [`Mimir.Worker/HostApplicationBuilderExtensions.cs`](Mimir.Worker/HostApplicationBuilderExtensions.cs)):

- **BlockPoller** — block/transaction indexing.
- **TxPoller** — handlers driven by observed actions (item slot, pet, pledge, products,
  raider, stake, rune slot, table sheets, world boss, …) in
  [`Mimir.Worker/ActionHandler/`](Mimir.Worker/ActionHandler/).
- **DiffPoller** — account-state diff handlers (avatar, agent, inventory, collection,
  CP rankings, world information, balances, infinite tower, …) in
  [`Mimir.Worker/Handler/`](Mimir.Worker/Handler/).

Supporting pieces:
[`Mimir.Worker/StateDocumentConverter/`](Mimir.Worker/StateDocumentConverter/) converts
raw lib9c state into BSON documents, and
[`Mimir.Worker/CollectionUpdaters/`](Mimir.Worker/CollectionUpdaters/) applies updates to
collections. Optional initializers (gated by `EnableInitializing`) seed table sheets etc.

## MongoDB data model

- Document models live in [`Mimir.MongoDB/Bson/`](Mimir.MongoDB/Bson/); repositories in
  [`Mimir.MongoDB/Repositories/`](Mimir.MongoDB/Repositories/).
- The authoritative state-type/address → collection-name map is
  [`Mimir.MongoDB/CollectionNames.cs`](Mimir.MongoDB/CollectionNames.cs) (e.g. `avatar`,
  `inventory`, `agent`, `balance_ncg`, `world_information`, `transaction`, `metadata`,
  `infinite_tower_info`). Use this when adding a new indexed state type.
- Custom BSON serializers for lib9c / libplanet types are under
  [`Mimir.MongoDB/Bson/Serialization/`](Mimir.MongoDB/Bson/Serialization/).

## GraphQL surface

The query type is split between a class and a descriptor:
- [`Mimir/GraphQL/Queries/Query.cs`](Mimir/GraphQL/Queries/Query.cs) — resolver methods
  (single-object lookups).
- [`Mimir/GraphQL/Types/QueryType.cs`](Mimir/GraphQL/Types/QueryType.cs) — paginated /
  ranking fields configured via descriptors.
- Additional fields via type extensions in
  [`Mimir/GraphQL/TypeExtensions/`](Mimir/GraphQL/TypeExtensions/) (`sheet`, `sheetNames`).
- GraphQL object types in [`Mimir/GraphQL/Types/`](Mimir/GraphQL/Types/); lib9c-mirrored
  types come from [`Lib9c.GraphQL/`](Lib9c.GraphQL/).

Query categories:
- **Rankings** (offset-paged): `worldInformationRanking`, `adventureCpRanking`,
  `arenaCpRanking`, `raidCpRanking`, plus per-user variants `myWorldInformationRanking`,
  `myAdventureCpRanking`, `myArenaCpRanking`, `myRaidCpRanking`.
- **Avatar-scoped state**: `avatar`, `inventory`, `collection`, `combinationSlots`,
  `itemSlot`, `runeSlot`, `runes`, `pet`, `actionPoint`, `worldInformation`,
  `dailyRewardReceivedBlockIndex`.
- **Agent / balance**: `agent`, `balance`, `stake`, `pledge`.
- **Market**: `products` (paged), `product`, `productIds`.
- **Block / transaction**: `block`, `blocks` (paged), `transaction`, `transactions`
  (paged), `actionTypes`.
- **World boss / infinite tower**: `worldBoss`, `worldBossRaider`,
  `worldBossKillRewardRecord`, `infiniteTowerInfo`, `infiniteTowerInfos` (paged).
- **Util / meta**: `metadata`, `sheet`, `sheetNames`, `dailyActiveUsers`, `wncgPrice`.

### Paging behavior

Paged fields use HotChocolate `UseOffsetPaging` with `(skip, take)` arguments and return
`{ items, pageInfo { hasNextPage, hasPreviousPage } }`. There is **no `totalCount`**.
Page size defaults to **100**, max **300** (configured in
[`Mimir/Program.cs`](Mimir/Program.cs) via `SetPagingOptions`). The GraphQL schema also
sets `MaxFieldCost`/`MaxTypeCost` limits and an unauthenticated rate limit.

### Sync-lag / metadata caveat

Indexed data can **lag behind the chain tip**. Each collection records how far it has
synced; check it before relying on freshness:

```graphql
query {
  metadata(collectionName: "avatar") {
    latestBlockIndex
  }
}
```

## Relationship to lib9c

`Lib9c.Models` re-models lib9c on-chain state types for BSON storage and GraphQL
exposure; `Lib9c.GraphQL` defines the GraphQL types/scalars for them. When lib9c changes
its state shape, expect to update both projects and bump `<Lib9cVersion>` in
[`Directory.Build.props`](Directory.Build.props). Reference the upstream models at
https://github.com/planetarium/lib9c .

## Conventions

- The canonical collection-name string for each state type lives in
  [`Mimir.MongoDB/CollectionNames.cs`](Mimir.MongoDB/CollectionNames.cs); reuse those
  constants rather than hard-coding collection names.
- Adding a new indexed state generally means: a document model in `Mimir.MongoDB/Bson/`,
  a repository in `Mimir.MongoDB/Repositories/`, an entry in `CollectionNames.cs`, a
  worker handler/converter in `Mimir.Worker/`, and a GraphQL field in `Mimir/GraphQL/`.
- License: AGPL-3.0 (see [LICENSE](LICENSE)); the logo image is excluded.

## Where to look for X

| Need | Path |
|---|---|
| GraphQL host startup / paging / rate limit | [`Mimir/Program.cs`](Mimir/Program.cs) |
| Single-object query resolvers | [`Mimir/GraphQL/Queries/Query.cs`](Mimir/GraphQL/Queries/Query.cs) |
| Paged / ranking query fields | [`Mimir/GraphQL/Types/QueryType.cs`](Mimir/GraphQL/Types/QueryType.cs) |
| `sheet` / `sheetNames` fields | [`Mimir/GraphQL/TypeExtensions/`](Mimir/GraphQL/TypeExtensions/) |
| Collection names ↔ state types | [`Mimir.MongoDB/CollectionNames.cs`](Mimir.MongoDB/CollectionNames.cs) |
| BSON document models / repositories | [`Mimir.MongoDB/Bson/`](Mimir.MongoDB/Bson/), [`Mimir.MongoDB/Repositories/`](Mimir.MongoDB/Repositories/) |
| Chain → DB sync handlers | [`Mimir.Worker/Handler/`](Mimir.Worker/Handler/), [`Mimir.Worker/ActionHandler/`](Mimir.Worker/ActionHandler/) |
| State → BSON conversion | [`Mimir.Worker/StateDocumentConverter/`](Mimir.Worker/StateDocumentConverter/) |
| Worker config keys / poller types | [`Mimir.Worker/Configuration.cs`](Mimir.Worker/Configuration.cs) |
| Planet enum / shared options | [`Mimir.Shared/`](Mimir.Shared/) |
| lib9c-mirrored models / GraphQL types | [`Lib9c.Models/`](Lib9c.Models/), [`Lib9c.GraphQL/`](Lib9c.GraphQL/) |
| MongoDB index definitions | [`scripts/indexes.json`](scripts/indexes.json), [`scripts/create_index.sh`](scripts/create_index.sh) |
| Local setup walkthrough | [`CONTRIBUTING.md`](CONTRIBUTING.md) |
