FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["RestaurantApi.csproj", "./"]
RUN dotnet restore --no-cache

# Copy the rest of the code
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create App_Data directory with proper permissions
RUN mkdir -p /app/App_Data && \
    chmod -R 755 /app/App_Data

# Copy published files
COPY --from=publish /app/publish .

# Copy database file
COPY App_Data/restaurant.db /app/App_Data/restaurant.db

# Set environment variables
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RestaurantApi.dll"] 