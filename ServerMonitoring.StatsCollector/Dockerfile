FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServerMonitoring.StatsCollector/ServerMonitoring.StatsCollector.csproj", "ServerMonitoring.StatsCollector/"]
COPY ["ServerMonitoring.MessageQueue/ServerMonitoring.MessageQueue.csproj", "ServerMonitoring.MessageQueue/"]
RUN dotnet restore "ServerMonitoring.StatsCollector/ServerMonitoring.StatsCollector.csproj"
COPY . .
WORKDIR "/src/ServerMonitoring.StatsCollector"
RUN dotnet build "./ServerMonitoring.StatsCollector.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServerMonitoring.StatsCollector.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerMonitoring.StatsCollector.dll"]
