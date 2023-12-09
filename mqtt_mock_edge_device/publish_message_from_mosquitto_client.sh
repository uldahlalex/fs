#!/usr/bin/env bash
# if mosquitto is installed publish a message to topic TimeSeries from the mosquitto client
if command -v mosquitto_pub >/dev/null 2>&1; then
    mosquitto_pub -h localhost -t TimeSeries -m '{
                                                   "datapoint": 55
                                                 }'
else
    echo "mosquitto is not installed"
fi