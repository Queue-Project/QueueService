FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5005
EXPOSE 5006

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["LocalNuget/", "/src/LocalNuget/"]
COPY ["nuget.config", "/src/"]

COPY ["QueueService/QAPI/QAPI.csproj", "QueueService/QAPI/"]
COPY ["QueueService/QApplication/QApplication.csproj", "QueueService/QApplication/"]
COPY ["QueueService/QContracts/QContracts.csproj", "QueueService/QContracts/"]
COPY ["QueueService/QDomain/QDomain.csproj", "QueueService/QDomain/"]
COPY ["QueueService/QInfrastructure/QInfrastructure.csproj", "QueueService/QInfrastructure/"]

RUN dotnet restore "QueueService/QAPI/QAPI.csproj"
COPY QueueService/ /src/QueueService/
WORKDIR "/src/QueueService/QAPI"
RUN dotnet build "./QAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./QAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QAPI.dll"]
