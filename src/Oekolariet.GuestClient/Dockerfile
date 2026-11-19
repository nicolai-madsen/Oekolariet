# Recipe for the GuestClient Docker image.
# docker-compose builds this image and runs it as a container.
# Stage 1: build the Blazor WASM app with the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared build settings and project files first (faster restore cache)
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Oekolariet.GuestClient/Oekolariet.GuestClient.csproj", "src/Oekolariet.GuestClient/"]
COPY ["src/Oekolariet.Shared/Oekolariet.Shared.csproj", "src/Oekolariet.Shared/"]
RUN dotnet restore "src/Oekolariet.GuestClient/Oekolariet.GuestClient.csproj"

# Copy the rest of the source and publish static files
COPY . .
RUN dotnet publish "src/Oekolariet.GuestClient/Oekolariet.GuestClient.csproj" -c Release -o /app/publish

# Stage 2: serve the static files with nginx (no .NET needed)
FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
