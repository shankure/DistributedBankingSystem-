#!/bin/sh

HOST="$1"
PORT="$2"
shift 2

echo "⏳ Waiting for RabbitMQ at $HOST:$PORT..."
while ! nc -z "$HOST" "$PORT"; do
  sleep 1
done

echo "✅ RabbitMQ is up at $HOST:$PORT – starting app..."
exec "$@"