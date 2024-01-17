#Use the test compose fille to allow docker-in-docker for test containers
#Don't run with just "docker run"
FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY . .
WORKDIR /app/Tests

ENTRYPOINT ["dotnet", "test"]