#!/bin/bash

# Check if sqlite3 is installed
if ! command -v sqlite3 &> /dev/null; then
    echo "sqlite3 is not installed. Please install it first."
    exit 1
fi

# Check if the database file exists
if [ ! -f "restaurant.db" ]; then
    echo "Database file not found. Please make sure you're in the correct directory."
    exit 1
fi

# Run the SQL file
echo "Initializing selection groups..."
sqlite3 restaurant.db < init_selection_groups.sql

# Check if the command was successful
if [ $? -eq 0 ]; then
    echo "Selection groups initialized successfully."
else
    echo "Error initializing selection groups."
    exit 1
fi 