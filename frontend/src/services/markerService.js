import api from './api';

const markerService = {
  async getMarkers(filters = {}) {
    const params = new URLSearchParams();

    if (filters.category) params.append('category', filters.category);
    if (filters.search) params.append('search', filters.search);
    if (filters.minLatitude) params.append('minLatitude', filters.minLatitude);
    if (filters.maxLatitude) params.append('maxLatitude', filters.maxLatitude);
    if (filters.minLongitude) params.append('minLongitude', filters.minLongitude);
    if (filters.maxLongitude) params.append('maxLongitude', filters.maxLongitude);
    if (filters.page) params.append('page', filters.page);
    if (filters.pageSize) params.append('pageSize', filters.pageSize);

    const response = await api.get(`/api/markers?${params.toString()}`);
    return response.data;
  },

  async getMarkerById(id) {
    const response = await api.get(`/api/markers/${id}`);
    return response.data;
  },

  async createMarker(markerData) {
    const response = await api.post('/api/markers', markerData);
    return response.data;
  },

  async updateMarker(id, markerData) {
    const response = await api.put(`/api/markers/${id}`, markerData);
    return response.data;
  },

  async deleteMarker(id) {
    await api.delete(`/api/markers/${id}`);
  },
};

export default markerService;
