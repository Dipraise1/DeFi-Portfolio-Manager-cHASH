FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj files and restore as distinct layers
COPY DefiPortfolioManager.sln .
COPY src/Core/Core.csproj ./src/Core/
COPY src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
COPY src/Services/Services.csproj ./src/Services/
COPY src/Api/Api.csproj ./src/Api/

# Restore packages
RUN dotnet restore

# Copy all the source code and build
COPY . .
RUN dotnet build -c Release --no-restore

# Publish the API project
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish --no-build

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose the port
EXPOSE 80

# Set entry point
ENTRYPOINT ["dotnet", "DefiPortfolioManager.Api.dll"] 