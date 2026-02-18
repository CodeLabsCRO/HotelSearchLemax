# Hotel Search REST API

A JSON REST web service for hotel search built with ASP.NET Core 9.0. Hotels are ranked by a combination of price and distance from the user's location (cheaper + closer = better).

## Features

- **CRUD API** for hotel management (name, price, geo location)
- **Search API** with intelligent ranking based on price and distance
- **Geolocation** support with radius-based filtering
- **Pagination** and sorting options
- **Google Maps** integration for visual hotel search
- **Swagger/OpenAPI** documentation

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)

## Setup

### 1. Configure the database connection

Update `appsettings.json` in the `HotelSearchLemax` project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLExpress;Database=HotelSearchDb;User Id=sa;Password=SQL;TrustServerCertificate=True;"
  }
}
```

### 2. Build and run

```bash
# Build the solution
dotnet build HotelSearchLemax.sln

# Run the application
dotnet run --project HotelSearchLemax/HotelSearchLemax.csproj
```