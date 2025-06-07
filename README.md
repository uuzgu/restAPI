# Restaurant API

## Environment Variables

The following environment variables need to be set for the application to function properly:

### Required Variables
- `DATABASE_CONNECTION_STRING`: SQLite database connection string
  - Development: `Data Source=App_Data/restaurant.db`
  - Production: `Data Source=/app/App_Data/restaurant.db`

- `Stripe:SecretKey`: Your Stripe secret key
- `Stripe:WebhookSecret`: Your Stripe webhook signing secret
- `FrontendUrl`: The URL of your frontend application
- `ALLOWED_ORIGINS`: Comma-separated list of allowed CORS origins (e.g., "https://yourdomain.com,https://api.yourdomain.com")

### Optional Variables
- `ASPNETCORE_ENVIRONMENT`: Set to "Production" for production environment
- `ASPNETCORE_URLS`: The URLs the application should listen on (e.g., "http://+:80;https://+:443")

## Database Setup

1. The application will automatically create the database file in the `App_Data` directory if it doesn't exist
2. Ensure the `App_Data` directory has write permissions
3. The database will be automatically migrated on startup

## Deployment Steps

1. Set up the required environment variables
2. Ensure the `App_Data` directory exists and has proper permissions
3. Build the application:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Security Considerations

- The application includes security headers to protect against common web vulnerabilities
- CORS is configured to only allow specific origins
- HTTPS redirection is enabled
- Authentication and authorization middleware is in place

## API Documentation

API documentation is available at `/swagger` when running in development mode.

## Logging

The application uses console and debug logging. Logs include:
- Database initialization and migration
- API requests and responses
- Error details
- Stripe webhook events 