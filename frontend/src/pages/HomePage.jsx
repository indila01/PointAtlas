import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useMarkers } from '../contexts/MarkerContext';
import Map from '../components/map/MapContainer';
import MarkerForm from '../components/markers/MarkerForm';
import './HomePage.css';

const HomePage = () => {
  const { user, logout } = useAuth();
  const { fetchMarkers, markers, loading, updateFilters, filters } = useMarkers();
  const [showMarkerForm, setShowMarkerForm] = useState(false);
  const [clickedPosition, setClickedPosition] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');

  useEffect(() => {
    fetchMarkers();
  }, [filters]);

  const handleMapClick = (e) => {
    if (e.latlng) {
      setClickedPosition(e.latlng);
      setShowMarkerForm(true);
    }
  };

  const handleMarkerCreated = () => {
    setShowMarkerForm(false);
    setClickedPosition(null);
    fetchMarkers();
  };

  const handleSearch = (e) => {
    e.preventDefault();
    updateFilters({ search: searchTerm });
  };

  const handleCategoryChange = (e) => {
    const category = e.target.value;
    setSelectedCategory(category);
    updateFilters({ category });
  };

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="home-page">
      <header className="app-header">
        <div className="header-left">
          <h1>PointAtlas</h1>
          <span className="marker-count">{markers.length} markers</span>
        </div>

        <div className="header-center">
          <form onSubmit={handleSearch} className="search-form">
            <input
              type="text"
              placeholder="Search markers..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
            />
            <button type="submit" className="search-btn">Search</button>
          </form>

          <select
            value={selectedCategory}
            onChange={handleCategoryChange}
            className="category-filter"
          >
            <option value="">All Categories</option>
            <option value="Restaurant">Restaurant</option>
            <option value="Park">Park</option>
            <option value="Museum">Museum</option>
            <option value="Landmark">Landmark</option>
            <option value="Shop">Shop</option>
            <option value="Other">Other</option>
          </select>
        </div>

        <div className="header-right">
          <span className="user-info">👤 {user?.displayName}</span>
          <button onClick={handleLogout} className="btn-logout">
            Logout
          </button>
        </div>
      </header>

      <div className="map-container">
        {loading && (
          <div className="loading-overlay">
            <div className="loading-spinner">Loading markers...</div>
          </div>
        )}
        <Map onMapClick={handleMapClick} />
      </div>

      <button
        className="btn-add-marker"
        onClick={() => setShowMarkerForm(true)}
        title="Click map to add marker at location, or click this button"
      >
        + Add Marker
      </button>

      {showMarkerForm && (
        <MarkerForm
          initialPosition={clickedPosition}
          onSuccess={handleMarkerCreated}
          onCancel={() => {
            setShowMarkerForm(false);
            setClickedPosition(null);
          }}
        />
      )}
    </div>
  );
};

export default HomePage;
