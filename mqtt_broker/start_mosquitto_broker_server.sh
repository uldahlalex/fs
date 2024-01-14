#!/usr/bin/env bash
#if mosquitto is installed, start the server (you can also run with docker compose)
if command -v mosquitto >/dev/null 2>&1; then
    mosquitto
else
    echo "mosquitto is not installed"
fi