## Example system for Fullstack Development 2024, Business academy southwest

## Quickstart:
If you have Docker and Docker Compose installed:
```
docker-compose up --build
```
If you just want to run all tests: 
```
docker-compose up --build
```

## What does it do?

- Demonstration of realtime chat application + live data presentation from IoT edge devices

## Graphical system overview

![Diagram Description](fs.png)

xml file for graphical representation: (draw.io): [fs.xml](fs.xml)

1. Directory: /frontend/, Angular Client application
2. Directory: /flutter_websocket_client_app/, Flutter mobile client application
3. Directory: /backend/ .NET 8 Websocket server + MQTT client
4. Directory: /backend/api/Mqtt/ The specific MQTT client segment of the .NET application
5. Directory: /mqtt_broker/ A bash script to start a mosquitto MQTT broker server locally
6. Directory: /mqtt_mock_edge_device/ A bash script to start an MQTT client using mosquitto and publishing a message
7. (no directory) Postgres DB to persist and query data from .NET using Npgsql + Dapper
8. (no directory) Azure Cognitive Services called with HTTP requests from .NET http client