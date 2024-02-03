## Experimental playground sample system for Fullstack Development 2024, Business academy southwest

## Graphical system overview

![Diagram Description](fs.png)

#### Diagram explanation can be found in the bottom of README.

## What is it?

When settling on technologies, style and architecture, I spend some time experimenting to find a fit where I will create
a sample app - this is that.

Naturally, some of the contents end up not being part of the actual course contents, since I deem they don't fit into
the scope, but I might still leave the code in here.

### Try the deployed version:

The current version can be used here: (link coming soon)

## Quickstart: Run Locally

### With Docker and Docker Compose installed:

```
docker-compose up --build
```

### Without docker. With .NET 8 CLI:

(will use some default environment variables without certain 3rd party services like Azure
Cognitive Services):

```
cd backend/api && dotnet run --rebuild-db
```

## What does it do?

- Demonstration of realtime chat application + live data presentation from IoT edge devices

xml file for graphical representation: (draw.io): [fs.xml](fs.xml)

1. Directory: /frontend/, Angular Client application
2. Directory: /flutter_websocket_client_app/, Flutter mobile client application
3. Directory: /backend/ .NET 8 Websocket server + MQTT client
4. Directory: /backend/api/Mqtt/ The specific MQTT client segment of the .NET application
5. Directory: /mqtt_broker/ A bash script to start a mosquitto MQTT broker server locally
6. Directory: /mqtt_mock_edge_device/ A bash script to start an MQTT client using mosquitto and publishing a message
7. (no directory) Postgres DB to persist and query data from .NET using Npgsql + Dapper
8. (no directory) Azure Cognitive Services called with HTTP requests from .NET http client