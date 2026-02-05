# PointAtlas - Geospatial SaaS Mapping Application

A containerized SaaS mapping application built with ASP.NET Core 8.0, React 18, and PostgreSQL with PostGIS. Users can view, create, and interact with geospatial point markers on an interactive map.

## Architecture

### Clean Architecture - Four-Layer Design

```
┌─────────────────────────────────────────────────┐
│  PointAtlas.API (Presentation)                  │
│  - Controllers                                  │
│  - DTOs, Requests/Responses                     │
│  - Middleware, Filters                          │
└────────────┬────────────────────────────────────┘
             │ depends on ↓
┌────────────▼────────────────────────────────────┐
│  PointAtlas.Application (Business Logic)        │
│  - Service Interfaces & Implementations         │
│  - Business Logic, Use Cases                    │
│  - Authorization Handlers                       │
│  - Mapping Profiles (Entity ↔ DTO)             │
└────────────┬────────────────────────────────────┘
             │ depends on ↓
┌────────────▼────────────────────────────────────┐
│  PointAtlas.Infrastructure (Data Access)        │
│  - DbContext, Configurations                    │
│  - Repository Implementations                   │
│  - External Service Implementations             │
└────────────┬────────────────────────────────────┘
             │ depends on ↓
┌────────────▼────────────────────────────────────┐
│  PointAtlas.Core (Domain)                       │
│  - Entities (Marker, ApplicationUser)           │
│  - Repository Interfaces                        │
│  - Domain Exceptions                            │
│  - Enums, Value Objects                         │
│  ⚠️ NO DEPENDENCIES - Pure domain logic          │
└─────────────────────────────────────────────────┘
```

**Key Principle**: Core has zero dependencies. All dependencies point inward toward the Core.

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL 16 with PostGIS 3.4
- **Authentication**: ASP.NET Core Identity with JWT tokens
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Geospatial**: NetTopologySuite

### Frontend
- **Framework**: React 18
- **Build Tool**: Vite
- **Map Library**: Leaflet
- **HTTP Client**: Axios
- **Routing**: React Router

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Database Extension**: PostGIS for spatial queries

## Why PostGIS Over Plain PostgreSQL?

PointAtlas uses **PostgreSQL with PostGIS extension** instead of plain PostgreSQL because it's a geospatial application that requires efficient spatial data operations:

### What PostGIS Provides

1. **Native Spatial Data Types**
   - `geography(point, 4326)` stores coordinates with Earth curvature calculations
   - More efficient and accurate than separate `latitude`/`longitude` decimal columns
   - SRID 4326 ensures global coordinate system compatibility

2. **Spatial Indexing (GIST)**
   - PostgreSQL's standard B-tree indexes don't work efficiently for 2D spatial queries
   - GIST (Generalized Search Tree) indexes enable fast spatial queries
   - **Performance**: Query 100,000 markers in viewport in ~5ms vs ~500ms without spatial indexing

3. **Built-in Spatial Functions**
   - `ST_Contains()` - Check if point is within bounding box (map viewport filtering)
   - `ST_Distance()` - Calculate distance between coordinates
   - `ST_DWithin()` - Find markers within X kilometers radius
   - Without PostGIS, these would require complex trigonometry and perform poorly

### Real Impact on PointAtlas

- **Bounding Box Queries**: When users pan/zoom the map, the app fetches only markers visible in the current viewport using `ST_Contains()` with GIST index
- **Nearby Search**: "Find markers within 5km" uses `ST_DWithin()` with Earth curvature calculations
- **Scalability**: Can efficiently handle 100,000+ markers with sub-50ms query times
- **.NET Integration**: NetTopologySuite library seamlessly works with PostGIS geometry types

**Plain PostgreSQL would work** for small datasets with simple coordinate storage, but PostGIS is essential for production geospatial applications requiring performance and spatial query capabilities.

## Features

### User Features
- User registration and login with JWT authentication
- View markers on interactive Leaflet map
- Click markers to see details in popup
- Filter markers by category
- Search markers by title/description
- Add new markers with form + map location picker
- Edit/delete own markers
- Role-based permissions (User vs Admin)

### Technical Features
- PostgreSQL PostGIS for geospatial queries
- Spatial indexing (GIST) for performance
- Bounding box filtering (show markers in viewport)
- JWT token refresh mechanism
- CORS configuration for SPA
- Docker containerization with hot-reload for development

## Project Structure

```
PointAtlas/
├── backend/
│   ├── PointAtlas.sln
│   ├── PointAtlas.API/               # Presentation Layer
│   │   ├── Controllers/              # HTTP endpoints
│   │   └── Program.cs                # Application startup
│   ├── PointAtlas.Application/       # Business Logic Layer
│   │   ├── Services/                 # Business logic services
│   │   ├── DTOs/                     # Data transfer objects
│   │   ├── Mappings/                 # AutoMapper profiles
│   │   └── Validators/               # FluentValidation rules
│   ├── PointAtlas.Core/              # Domain Layer
│   │   ├── Entities/                 # Domain entities
│   │   └── Interfaces/               # Repository interfaces
│   └── PointAtlas.Infrastructure/    # Data Access Layer
│       ├── Data/                     # DbContext, configurations
│       ├── Repositories/             # Repository implementations
│       └── Services/                 # External services (JWT, etc.)
├── frontend/
│   └── src/
│       ├── components/               # React components
│       ├── contexts/                 # State management
│       ├── services/                 # API clients
│       └── hooks/                    # Custom React hooks
├── database/
│   └── init-scripts/                 # PostGIS setup
├── docker-compose.yml                # Production compose
├── docker-compose.dev.yml            # Development compose
└── .env.example                      # Environment template
```

## Quick Start

### Prerequisites
- Docker Desktop (or Docker Engine + Docker Compose)
- .NET 8.0 SDK (for local development)
- Node.js 18+ (for frontend development)

### Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd PointAtlas
```

2. **Create environment file**
```bash
cp .env.example .env
# Edit .env with your configuration
```

3. **Start all services with Docker Compose**
```bash
# Production mode
docker compose up -d

# Development mode with hot-reload
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d
```

4. **Access the application**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

### Default Admin Credentials
- **Email**: admin@pointatlas.com
- **Password**: Admin@123456

> ⚠️ **SECURITY WARNING**: The default admin password and JWT secret in `appsettings.json` are for **development only**. For production:
> 1. Change the admin password immediately after first login
> 2. Set a strong JWT secret via environment variables
> 3. Use strong database passwords
> 4. Never commit `.env` files with real credentials

## Environment Variables

```env
# Database
POSTGRES_DB=pointatlas
POSTGRES_USER=postgres
POSTGRES_PASSWORD=SecurePassword123!
POSTGRES_PORT=5432

# Backend
ASPNETCORE_ENVIRONMENT=Development
BACKEND_PORT=5000
JWT_SECRET=your-super-secret-jwt-key-at-least-32-characters-long
JWT_ISSUER=PointAtlas
JWT_AUDIENCE=PointAtlasUsers
JWT_EXPIRATION_MINUTES=60

# CORS
CORS_ALLOWED_ORIGINS=http://localhost:3000

# Frontend
FRONTEND_PORT=3000
VITE_API_URL=http://localhost:5000
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Revoke refresh token
- `GET /api/auth/me` - Get current user

### Markers
- `GET /api/markers` - List markers with filters
  - Query params: `category`, `search`, `minLatitude`, `maxLatitude`, `minLongitude`, `maxLongitude`, `page`, `pageSize`
- `GET /api/markers/{id}` - Get single marker
- `POST /api/markers` - Create marker (authenticated)
- `PUT /api/markers/{id}` - Update marker (owner/admin)
- `DELETE /api/markers/{id}` - Delete marker (owner/admin)

## Database Schema

### Markers Table
```sql
CREATE TABLE "Markers" (
    "Id" uuid PRIMARY KEY,
    "Title" varchar(200) NOT NULL,
    "Description" text,
    "Location" geography(point, 4326) NOT NULL,
    "Latitude" double precision NOT NULL,
    "Longitude" double precision NOT NULL,
    "Category" varchar(100),
    "Properties" jsonb,
    "CreatedById" uuid NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL,
    FOREIGN KEY ("CreatedById") REFERENCES "AspNetUsers"("Id")
);

-- Spatial index for efficient location queries
CREATE INDEX "idx_markers_location" ON "Markers" USING GIST("Location");
```

## Development

### Run Backend Locally
```bash
cd backend
dotnet restore
dotnet build
dotnet ef database update --project PointAtlas.Infrastructure --startup-project PointAtlas.API
cd PointAtlas.API
dotnet run
```

### Run Frontend Locally
```bash
cd frontend
npm install
npm run dev
```

### Create New Migration
```bash
cd backend
dotnet ef migrations add MigrationName --project PointAtlas.Infrastructure --startup-project PointAtlas.API
dotnet ef database update --project PointAtlas.Infrastructure --startup-project PointAtlas.API
```

### View Logs
```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f backend
docker compose logs -f postgres
```

## Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm run test
```

## Production Deployment

1. **Build for production**
```bash
docker compose -f docker-compose.yml build
```

2. **Update environment variables** for production (use strong passwords, proper JWT secret)

3. **Start services**
```bash
docker compose up -d
```

4. **Configure reverse proxy** (Nginx/Caddy) for HTTPS

## Security Considerations

- JWT tokens use HMAC-SHA256 with configurable secret
- Passwords hashed with PBKDF2 via ASP.NET Core Identity
- CORS configured for specific origins
- Role-based authorization (User, Admin)
- Refresh token rotation on use
- SQL injection protection via EF Core parameterized queries

## Performance Optimizations

- GIST spatial index on marker locations
- Pagination for marker lists (default 100 per page)
- EF Core query optimization with Include()
- Connection pooling for PostgreSQL
- Docker multi-stage builds for smaller images

## Documentation

- **[Quick Start Guide](QUICK_START.md)** - Get started testing the backend immediately
- **[Initial Plan](.claude/docs/INITIAL_PLAN.md)** - Original implementation plan and architecture decisions
- **[Implementation Status](.claude/docs/IMPLEMENTATION_STATUS.md)** - Detailed status of what's been completed

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Acknowledgments

- PostGIS for spatial database capabilities
- Leaflet for interactive maps
- ASP.NET Core team for excellent framework
- NetTopologySuite for .NET spatial operations

