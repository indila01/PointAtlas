-- Enable PostGIS extension
CREATE EXTENSION IF NOT EXISTS postgis;

-- Enable UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable trigram similarity for text search
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Verify PostGIS installation
SELECT PostGIS_version();
