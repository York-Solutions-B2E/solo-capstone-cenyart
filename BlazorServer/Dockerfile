# BlazorServer Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 5002
# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BlazorServer/BlazorServer.csproj", "BlazorServer/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "BlazorServer/BlazorServer.csproj"
COPY . .
WORKDIR "/src/BlazorServer"
RUN dotnet publish "BlazorServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorServer.dll"]
