
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development

WORKDIR /app

COPY ["api.csproj", "."]
RUN dotnet restore "api.csproj"

COPY . .

WORKDIR /app

EXPOSE 8181

#I use dotnet run because this is primarily for development
CMD ["dotnet", "run"] 
