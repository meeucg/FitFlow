FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FeedCore/FeedCore.csproj", "FeedCore/"]
COPY ["FeedCore.Application/FeedCore.Application.csproj", "FeedCore.Application/"]
COPY ["FeedCore.Core/FeedCore.Core.csproj", "FeedCore.Core/"]
COPY ["FeedCore.Infrastructure/FeedCore.Infrastructure.csproj", "FeedCore.Infrastructure/"]
COPY ["FeedCore.Persistence/FeedCore.Persistence.csproj", "FeedCore.Persistence/"]
COPY ["FeedCoreGrpcContracts/FeedCoreGrpcContracts.csproj", "FeedCoreGrpcContracts/"]
COPY ["FeedCoreGrpcContracts/Protos/feedcore.proto", "FeedCoreGrpcContracts/Protos/"]
RUN dotnet restore "FeedCore/FeedCore.csproj"
COPY . .
WORKDIR "/src/FeedCore"
RUN dotnet build "FeedCore.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FeedCore.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FeedCore.dll"]
