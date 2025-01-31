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

FROM base AS final
WORKDIR /app
COPY ["IgroCehApp/localhost.pfx", "/app/localhost.pfx"]
COPY ["IgroCehApp/localhost.crt", "/usr/local/share/ca-certificates/localhost.crt"]
RUN update-ca-certificates
RUN mkdir -p /app/Upload
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]