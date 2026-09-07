FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServerMonitoring.NotificationService/ServerMonitoring.NotificationService.csproj", "ServerMonitoring.NotificationService/"]
COPY ["ServerMonitoring.SignalR/ServerMonitoring.SignalR.csproj", "ServerMonitoring.SignalR/"]
RUN dotnet restore "ServerMonitoring.NotificationService/ServerMonitoring.NotificationService.csproj"
COPY . .
WORKDIR "/src/ServerMonitoring.NotificationService"
RUN dotnet build "./ServerMonitoring.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServerMonitoring.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerMonitoring.NotificationService.dll"]
