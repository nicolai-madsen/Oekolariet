# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props","."]
COPY ["Directory.Build.props","."]
COPY ["src/Oekolariet.Api/Oekolariet.Api.csproj","src/Oekolariet.Api/"]
COPY ["src/Oekolariet.Application/Oekolariet.Application.csproj","src/Oekolariet.Application/"]
COPY ["src/Oekolariet.Domain/Oekolariet.Domain.csproj","src/Oekolariet.Domain/"]
COPY ["src/Oekolariet.Shared/Oekolariet.Shared.csproj","src/Oekolariet.Shared/"]
COPY ["src/Oekolariet.Infrastructure/Oekolariet.Infrastructure.csproj","src/Oekolariet.Infrastructure/"]
RUN dotnet restore "./src/Oekolariet.Api/Oekolariet.Api.csproj"
COPY . .
WORKDIR "/src/src/Oekolariet.Api"
RUN dotnet build "./Oekolariet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Oekolariet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Oekolariet.Api.dll"]