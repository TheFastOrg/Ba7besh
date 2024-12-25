#!/bin/bash

# Default values
DB_HOST="localhost"
DB_PORT="5432"
DB_NAME="ba7besh"
DB_USER="ba7besh"
DB_PASSWORD="ba7besh_local_dev"
MIGRATIONS_PATH="./migrations/*.sql"

# Function to print usage
print_usage() {
    echo "Usage: ./migrate.sh [-h host] [-p port] [-d database] [-u user] [-w password]"
    echo "  -h    Database host (default: localhost)"
    echo "  -p    Database port (default: 5432)"
    echo "  -d    Database name (default: ba7besh)"
    echo "  -u    Database user (default: ba7besh)"
    echo "  -w    Database password (default: ba7besh_local_dev)"
}

# Parse command line arguments
while getopts h:p:d:u:w: flag
do
    case "${flag}" in
        h) DB_HOST=${OPTARG};;
        p) DB_PORT=${OPTARG};;
        d) DB_NAME=${OPTARG};;
        u) DB_USER=${OPTARG};;
        w) DB_PASSWORD=${OPTARG};;
        *) print_usage
           exit 1 ;;
    esac
done

# Construct connection string
CONNECTION_STRING="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};"

# Run migrations
echo "Running migrations..."
dotnet-badgie-migrator "$CONNECTION_STRING" "$MIGRATIONS_PATH" -i -d:Postgres