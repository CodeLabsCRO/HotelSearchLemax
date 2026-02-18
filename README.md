# Hotel Search REST API

A JSON REST web service for hotel search built with ASP.NET Core 9.0. Hotels are ranked by a combination of price and distance from the user's location (cheaper + closer = better).

## Features

- **CRUD API** for hotel management (name, price, geo location)
- **Search API** with intelligent ranking based on price and distance
- **JWT Authentication** - all endpoints require authentication
- **Geolocation** support with radius-based filtering
- **Pagination** and sorting options
- **Google Maps** integration for visual hotel search
- **Swagger/OpenAPI** documentation with JWT support
- **Health checks** for production monitoring
- **Unit tests** with comprehensive coverage (22 tests)
- **CI/CD** with GitHub Actions

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

The application will:
- Automatically apply EF Core migrations
- Seed the database with 20 sample hotels in Zagreb, Croatia

### 3. Access the application

- **Web UI**: https://localhost:7084
- **Swagger API**: https://localhost:7084/swagger
- **Health Check**: https://localhost:7084/health

## Testing

Run the unit tests:

```bash
dotnet test
```

### Test Coverage

The test suite includes 22 tests covering:

- **CRUD Operations**: Create, Read, Update, Delete hotels
- **Search Functionality**: Ranking algorithm, filtering, pagination
- **Edge Cases**: Empty results, single hotel, same location/price, boundary conditions

## Authentication

All API endpoints (except `/api/auth/*`) require JWT authentication. Unauthenticated requests will receive a `401 Unauthorized` response.

### Register a new user

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "password123"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "johndoe",
  "password": "password123"
}
```

### Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "johndoe",
  "expiresAt": "2024-01-02T12:00:00Z"
}
```

### Using the token

Include the JWT token in the `Authorization` header:

```http
GET /api/hotels
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

In Swagger UI, click the "Authorize" button and enter: `Bearer <your-token>`

## API Endpoints

### Authentication (No Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and get JWT token |

### Hotels CRUD (Requires Authentication)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/hotels` | Get all hotels |
| GET | `/api/hotels/{id}` | Get hotel by ID |
| POST | `/api/hotels` | Create a new hotel |
| PUT | `/api/hotels/{id}` | Update a hotel |
| DELETE | `/api/hotels/{id}` | Delete a hotel |

### Hotel Search (Requires Authentication)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/hotels/search` | Search hotels with ranking |

#### Search Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userLatitude` | double | Yes | User's latitude (-90 to 90) |
| `userLongitude` | double | Yes | User's longitude (-180 to 180) |
| `radiusKm` | double | No | Filter by radius in kilometers |
| `searchKeywords` | string | No | Filter by hotel name |
| `sortBy` | enum | No | Score (default), Name, Price, Distance |
| `sortDirection` | enum | No | Asc (default), Desc |
| `pageNumber` | int | No | Page number (default: 1) |
| `pageSize` | int | No | Items per page (default: 10, max: 100) |

#### Example Request

```
GET /api/hotels/search?userLatitude=45.8150&userLongitude=15.9819&radiusKm=5&pageSize=10
```

#### Example Response

```json
{
  "items": [
    {
      "id": 1,
      "name": "Hotel Esplanade Zagreb",
      "price": 180.00,
      "latitude": 45.8050,
      "longitude": 15.9780,
      "distanceKm": 1.23,
      "score": 0.25
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 15
}
```

### Health Check

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Application and database health status |

## Ranking Algorithm

Hotels are ranked using a combined score:

```
score = (normalizedPrice + normalizedDistance) / 2
```

Where:
- `normalizedPrice` = (price - minPrice) / (maxPrice - minPrice)
- `normalizedDistance` = (distance - minDistance) / (maxDistance - minDistance)

**Lower score = better** (cheaper and closer to user)

## Project Structure

```
HotelSearchLemax/
├── HotelSearchLemax.Core/           # Domain layer
│   ├── Entities/                    # Domain entities (Hotel)
│   ├── Interfaces/                  # Repository interfaces
│   └── DTOs/                        # Data transfer objects
│
├── HotelSearchLemax.DataAccess/     # Data layer
│   ├── Data/                        # DbContext, Seeder
│   ├── Configurations/              # EF Core configurations
│   └── Repositories/                # Repository implementations
│
├── HotelSearchLemax.Services/       # Business logic layer
│   ├── Interfaces/                  # Service interfaces
│   ├── Implementations/             # Service implementations
│   └── Mapping/                     # Entity-DTO mappings
│
├── HotelSearchLemax/                # Web layer (API + MVC)
│   ├── Controllers/Api/             # REST API controllers
│   ├── Controllers/                 # MVC controllers
│   ├── Views/                       # Razor views
│   └── wwwroot/                     # Static files
│
├── HotelSearchLemax.Tests/          # Unit tests
│   └── Services/                    # Service tests
│
└── .github/workflows/               # CI/CD
    └── ci.yml                       # GitHub Actions workflow
```

## Web Interface

The application includes MVC views for hotel management:

- `/HotelsView` - Hotel list with pagination, sorting, and search
- `/HotelsView/Create` - Create a new hotel
- `/HotelsView/Edit/{id}` - Edit a hotel
- `/HotelsView/Details/{id}` - View hotel details
- `/HotelsView/Delete/{id}` - Delete confirmation
- `/HotelsView/Search` - Google Maps search interface

## Technologies

- ASP.NET Core 9.0
- Entity Framework Core 9.0
- SQL Server
- xUnit, Moq, FluentAssertions
- Google Maps JavaScript API
- Bootstrap 5
- Swagger/OpenAPI
- GitHub Actions CI/CD

## Security Considerations

- **JWT Authentication** - All API endpoints require valid JWT token
- **Authorization** - Unauthenticated users cannot access any hotel operations
- **Password hashing** - User passwords are hashed using SHA256
- **Input validation** on all API endpoints (coordinate ranges, string lengths)
- **Parameterized queries** via Entity Framework (SQL injection prevention)
- **HTTPS enforced** in production
- **No sensitive data** in responses

## License

This project is a take-home assignment for Lemax.
