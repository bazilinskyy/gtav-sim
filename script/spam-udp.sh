#!/bin/bash
set -e

PORT=${1:-5001}
HOST=${2:-127.0.0.1}

for i in $(seq 0 100000); do
  echo -n "."
  echo "message ${i}" | nc -vuw 0 $HOST $PORT
done
