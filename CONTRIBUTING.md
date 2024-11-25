# Contributor Guide

If you want to run `Mimir.Worker`, please refer to [Run Mimir.Worker](#Run-Mimir.Worker).
If you want to run `Mimir`, please refer to [Run Mimir](#Run-Mimir).

## Run Mimir.Worker

### Run MongoDB
To run Mimir.Worker, MongoDB is required. While you can run MongoDB in any way you prefer, this guide recommends using Docker.  
First, install [Docker](https://www.docker.com/products/docker-desktop/) according to your OS settings. You also need to have [Docker Compose](https://docs.docker.com/compose/install/) installed.

```yml
services:
  mongo:
    image: mongo:latest
    hostname: mongo
    restart: always
    environment:
      MONGO_INITDB_ROOT_USERNAME: rootuser
      MONGO_INITDB_ROOT_PASSWORD: rootpass
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
      - mongo_config:/data/configdb

volumes:
  mongodb_data:
  mongo_config:
```

Save the content above as `docker-compose.yml` and run it with the `docker compose up -d` command.  
Ensure that the DB is set up correctly by checking the Docker logs or using [Compass](https://www.mongodb.com/products/tools/compass).

### Create appsettings.local.json
First, choose the planet you want to run. Then, select either heimdall or odin, and copy the `appsettings.heimdall-for-local.json` or `appsettings.odin-for-local.json` file, renaming it to `appsettings.local.json`.  
You can refer to [Configuration](./Mimir.Worker/Configuration.cs) for the defined values and modify them as needed.  
If you set up MongoDB using the Docker Compose file provided above, the `MongoDbConnectionString` will be `mongodb://rootuser:rootpass@localhost:27017`.  
If you have set up the database separately, configure the ConnectionString to connect to that database.

### Run
Now, run the Worker with the environment variable pointing to the appsettings file you configured.

```sh
WORKER_CONFIG_FILE=appsettings.local.json dotnet run --project Mimir.Worker
```

You will know the setup is successful when you start seeing logs requesting headless and data accumulating in the DB.

### Test
You can run tests using the following command:

```sh
dotnet test Mimir.Worker.Tests
```

## Run Mimir

### Create appsettings.local.json
First, copy `appsettings.json` and rename it to `appsettings.local.json`.  
You can review the related settings in [Options](./Mimir/Options/).  
Mimir requires a pre-populated MongoDB database. Make sure to run a MongoDB instance with data already loaded, or use `Mimir.Worker` to populate the data.  
Once you have a MongoDB instance with data, configure the appropriate ConnectionString and Database Name (heimdall, odin).

### Run
Now, run Mimir with the environment variable pointing to the appsettings file you configured.

```sh
ASPNETCORE_ENVIRONMENT=local dotnet run --project Mimir
```

## Bump Lib9c

Mimir is using [`Lib9c`][lib9c] to use their models. So you may need to bump `Lib9c` dependency to use new models in new release of it.

Then you can update `<Lib9cVersion>` property in `Directory.Build.props`, with the `Lib9c` release version you want.

## Bump Libplanet

Mimir is using [`Libplanet`][libplanet] to use crypto-related types (e.g., `Address`), to iterate trie when initializing database from snapshots. So you may need to bump `Libplanet` and `Libplanet.*` dependencies to follow `Lib9c`'s `Libplanet` version.

Then you can update `<LibplanetVersion>` property in `Directory.Build.props`, with the `Libplanet` release version you want.

You can see the `Libplanet` version used by `Lib9c`, in NuGet's dependencies view.

```
# 'lib9c-version' may be like 1.20.0
https://www.nuget.org/packages/Lib9c/<lib9c-version>#dependencies-body-tab
```

[lib9c]: https://github.com/planetarium/lib9c
[libplanet]: https://github.com/planetarium/libplanet
