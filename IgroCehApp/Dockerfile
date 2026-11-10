#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IgroCehApp/API.csproj", "IgroCehApp/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Persistence/Persistence.csproj", "Persistence/"]
RUN dotnet restore "./IgroCehApp/./API.csproj"
COPY . .
WORKDIR "/src/IgroCehApp"
RUN dotnet build "./API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Server stage: plain http on 8080 behind the nginx of the server, which holds the certificate
FROM base AS production
WORKDIR /app
RUN mkdir -p /app/Upload
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]

# Local stage: the local certificate for https on 8081
FROM production AS final
COPY ["IgroCehApp/gamews-quest.pfx", "/app/gamews-quest.pfx"]
COPY ["IgroCehApp/gamews-quest.crt", "/usr/local/share/ca-certificates/gamews-quest.crt"]
RUN update-ca-certificates