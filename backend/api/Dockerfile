# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development

# Set the working directory inside the container
WORKDIR /app

# Copy csproj and restore any dependencies (via nuget)
COPY ["api.csproj", "."]
RUN dotnet restore "api.csproj"

# Copy the local code to the container
COPY . .

# Set the working directory to the API project directory
WORKDIR /app

# Open up the port your application is configured to listen on
EXPOSE 8181

# Start the application using dotnet run
CMD ["dotnet", "run"]
