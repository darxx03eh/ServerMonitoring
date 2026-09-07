FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServerMonitoring.Processor/ServerMonitoring.Processor.csproj", "ServerMonitoring.Processor/"]
COPY ["ServerMonitoring.MessageQueue/ServerMonitoring.MessageQueue.csproj", "ServerMonitoring.MessageQueue/"]
RUN dotnet restore "ServerMonitoring.Processor/ServerMonitoring.Processor.csproj"
COPY . .
WORKDIR "/src/ServerMonitoring.Processor"
RUN dotnet build "./ServerMonitoring.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServerMonitoring.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerMonitoring.Processor.dll"]
