FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServerMonitoring.EventConsumer/ServerMonitoring.EventConsumer.csproj", "ServerMonitoring.EventConsumer/"]
COPY ["ServerMonitoring.SignalR/ServerMonitoring.SignalR.csproj", "ServerMonitoring.SignalR/"]
RUN dotnet restore "ServerMonitoring.EventConsumer/ServerMonitoring.EventConsumer.csproj"
COPY . .
WORKDIR "/src/ServerMonitoring.EventConsumer"
RUN dotnet build "./ServerMonitoring.EventConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServerMonitoring.EventConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerMonitoring.EventConsumer.dll"]
