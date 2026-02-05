import { createContext, useState, useContext, useCallback } from 'react';
import markerService from '../services/markerService';

const MarkerContext = createContext(null);

export const MarkerProvider = ({ children }) => {
  const [markers, setMarkers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    category: '',
    search: '',
    minLatitude: null,
    maxLatitude: null,
    minLongitude: null,
    maxLongitude: null,
    page: 1,
    pageSize: 100,
  });

  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  const fetchMarkers = useCallback(async (customFilters = null) => {
    setLoading(true);
    setError(null);

    try {
      const filterParams = customFilters || filters;
      const response = await markerService.getMarkers(filterParams);

      setMarkers(response.items);
      setTotalCount(response.totalCount);
      setTotalPages(response.totalPages);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to fetch markers');
      console.error('Error fetching markers:', err);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  const createMarker = async (markerData) => {
    try {
      const newMarker = await markerService.createMarker(markerData);
      setMarkers((prev) => [newMarker, ...prev]);
      return newMarker;
    } catch (err) {
      throw new Error(err.response?.data?.message || 'Failed to create marker');
    }
  };

  const updateMarker = async (id, markerData) => {
    try {
      const updatedMarker = await markerService.updateMarker(id, markerData);
      setMarkers((prev) =>
        prev.map((marker) => (marker.id === id ? updatedMarker : marker))
      );
      return updatedMarker;
    } catch (err) {
      throw new Error(err.response?.data?.message || 'Failed to update marker');
    }
  };

  const deleteMarker = async (id) => {
    try {
      await markerService.deleteMarker(id);
      setMarkers((prev) => prev.filter((marker) => marker.id !== id));
    } catch (err) {
      throw new Error(err.response?.data?.message || 'Failed to delete marker');
    }
  };

  const updateFilters = (newFilters) => {
    setFilters((prev) => ({ ...prev, ...newFilters }));
  };

  const updateBounds = (bounds) => {
    updateFilters({
      minLatitude: bounds.minLat,
      maxLatitude: bounds.maxLat,
      minLongitude: bounds.minLng,
      maxLongitude: bounds.maxLng,
    });
  };

  const value = {
    markers,
    loading,
    error,
    filters,
    totalCount,
    totalPages,
    fetchMarkers,
    createMarker,
    updateMarker,
    deleteMarker,
    updateFilters,
    updateBounds,
  };

  return <MarkerContext.Provider value={value}>{children}</MarkerContext.Provider>;
};

export const useMarkers = () => {
  const context = useContext(MarkerContext);
  if (!context) {
    throw new Error('useMarkers must be used within a MarkerProvider');
  }
  return context;
};

export default MarkerContext;
