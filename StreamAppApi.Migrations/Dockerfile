FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5432

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["StreamAppApi.Migrations.csproj", "StreamAppApi.Migrations/"]
COPY ["../StreamAppApi.Bll/StreamAppApi.Bll.csproj", "StreamAppApi.Bll/"]
COPY ["../StreamAppApi.Contracts/StreamAppApi.Contracts.csproj", "StreamAppApi.Contracts/"]
RUN dotnet restore "StreamAppApi.Migrations/StreamAppApi.Migrations.csproj"

WORKDIR "/src/StreamAppApi.Migrations"
COPY ["../StreamAppApi.Bll/*", "StreamAppApi.Bll/"]
COPY ["../StreamAppApi.Contracts/*", "StreamAppApi.Contracts/"]
COPY . .

RUN dotnet build "StreamAppApi.Migrations.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StreamAppApi.Migrations.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StreamAppApi.Migrations.dll"]
