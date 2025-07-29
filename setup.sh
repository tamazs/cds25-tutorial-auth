#!/bin/sh
if [ $# -eq 0 ]; then
  echo "No arguments provided."
  echo "Usage: $0 <password>"
  echo "Please provide a value to use as default password as argument."
  exit 1
fi
# Discard the postgres container instance and create a new
docker compose down --volumes --remove-orphans && docker compose up -d
# Run code found in DbSeeder.cs to insert some testdata in the database
dotnet run --project server/Api --watch setup "$0"
