#!/bin/bash

# Create App_Data directory if it doesn't exist
mkdir -p App_Data

# Copy the database file if it exists
if [ -f "App_Data/restaurant.db" ]; then
    echo "Database file found, copying to deployment directory..."
    cp App_Data/restaurant.db App_Data/restaurant.db.bak
fi

# Build the application
dotnet build -c Release

# Restore the database file if it was backed up
if [ -f "App_Data/restaurant.db.bak" ]; then
    echo "Restoring database file..."
    mv App_Data/restaurant.db.bak App_Data/restaurant.db
fi

# Publish the application
dotnet publish -c Release -o publish 