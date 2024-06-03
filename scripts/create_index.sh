#!/bin/bash

CONN_STRING=${1:-$MongoDbConnectionString}

DB_NAMES=("heimdall" "odin")

if ! command -v jq &>/dev/null; then
    echo "jq could not be found, please install jq to process JSON files."
    exit 1
fi
if ! command -v mongosh &>/dev/null; then
    echo "mongosh could not be found, please install mongosh."
    exit 1
fi


INDEX_FILE="scripts/indexes.json"
if [ ! -f "$INDEX_FILE" ]; then
    echo "Index file not found: $INDEX_FILE"
    exit 1
fi

for DB_NAME in "${DB_NAMES[@]}"; do
    echo "Creating indexes on database ${DB_NAME}..."

    for COLLECTION in $(jq -r 'keys[]' "$INDEX_FILE"); do
        jq -c ".${COLLECTION}[]" "$INDEX_FILE" | while read INDEX; do
            echo "Creating index for ${COLLECTION}: ${INDEX}"
            mongosh $CONN_STRING --eval "db.getSiblingDB('${DB_NAME}').${COLLECTION}.createIndex($INDEX)"
        done
    done

    echo "Index creation on database ${DB_NAME} completed."
done
