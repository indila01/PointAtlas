import { useState } from 'react';
import { useMarkers } from '../../contexts/MarkerContext';
import './Markers.css';

const MarkerForm = ({ initialPosition, onSuccess, onCancel }) => {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    latitude: initialPosition?.lat || '',
    longitude: initialPosition?.lng || '',
    category: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const { createMarker } = useMarkers();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await createMarker({
        title: formData.title,
        description: formData.description || null,
        latitude: parseFloat(formData.latitude),
        longitude: parseFloat(formData.longitude),
        category: formData.category,
        properties: {},
      });

      onSuccess && onSuccess();
    } catch (err) {
      setError(err.message || 'Failed to create marker');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="marker-form-overlay">
      <div className="marker-form">
        <h2>Add New Marker</h2>

        {error && <div className="error-message">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              type="text"
              id="title"
              name="title"
              value={formData.title}
              onChange={handleChange}
              required
              minLength={3}
              maxLength={200}
              placeholder="Enter marker title"
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              maxLength={2000}
              rows={3}
              placeholder="Enter marker description (optional)"
            />
          </div>

          <div className="form-group">
            <label htmlFor="category">Category *</label>
            <select
              id="category"
              name="category"
              value={formData.category}
              onChange={handleChange}
              required
            >
              <option value="">Select a category</option>
              <option value="Restaurant">Restaurant</option>
              <option value="Park">Park</option>
              <option value="Museum">Museum</option>
              <option value="Landmark">Landmark</option>
              <option value="Shop">Shop</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="latitude">Latitude *</label>
              <input
                type="number"
                id="latitude"
                name="latitude"
                value={formData.latitude}
                onChange={handleChange}
                required
                step="any"
                min="-90"
                max="90"
                placeholder="40.7128"
              />
            </div>

            <div className="form-group">
              <label htmlFor="longitude">Longitude *</label>
              <input
                type="number"
                id="longitude"
                name="longitude"
                value={formData.longitude}
                onChange={handleChange}
                required
                step="any"
                min="-180"
                max="180"
                placeholder="-74.0060"
              />
            </div>
          </div>

          <div className="form-actions">
            <button type="button" onClick={onCancel} className="btn-secondary">
              Cancel
            </button>
            <button type="submit" disabled={loading} className="btn-primary">
              {loading ? 'Creating...' : 'Create Marker'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default MarkerForm;
