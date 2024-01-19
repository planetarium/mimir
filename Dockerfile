FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build-env
WORKDIR /app
ARG TARGETPLATFORM

# Copy everything else and build
COPY . ./
RUN <<EOF
#!/bin/bash
if [[ "$TARGETPLATFORM" = "linux/amd64" ]]
then
  dotnet publish NineChroniclesUtilBackend/NineChroniclesUtilBackend.csproj \
    -c Release \
    -r linux-x64 \
    -o out \
    --self-contained
elif [[ "$TARGETPLATFORM" = "linux/arm/v8" ]]
then
  dotnet publish NineChroniclesUtilBackend/NineChroniclesUtilBackend.csproj \
    -c Release \
    -r linux-arm64 \
    -o out \
    --self-contained
else
  echo "Not supported target platform: '$TARGETPLATFORM'."
  exit -1
fi
EOF

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy
WORKDIR /app
RUN apt-get update && apt-get install -y libc6-dev
COPY --from=build-env /app/out .

# Install native deps & utilities for production
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev jq curl \
     && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "NineChroniclesUtilBackend.dll"]
