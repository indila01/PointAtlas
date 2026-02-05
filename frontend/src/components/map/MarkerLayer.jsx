import { useState } from 'react';
import { Marker, Popup } from 'react-leaflet';
import { useMarkers } from '../../contexts/MarkerContext';
import { useAuth } from '../../contexts/AuthContext';

const MarkerLayer = ({ onEditMarker }) => {
  const { markers, deleteMarker } = useMarkers();
  const { user, isAdmin } = useAuth();
  const [deletingId, setDeletingId] = useState(null);

  const canEdit = (marker) => {
    if (!user) return false;
    return marker.createdById === user.id || isAdmin();
  };

  const handleDelete = async (marker) => {
    if (window.confirm(`Are you sure you want to delete "${marker.title}"?`)) {
      try {
        setDeletingId(marker.id);
        await deleteMarker(marker.id);
        alert('Marker deleted successfully!');
      } catch (error) {
        alert(error.message || 'Failed to delete marker');
      } finally {
        setDeletingId(null);
      }
    }
  };

  const handleEdit = (marker) => {
    if (onEditMarker) {
      onEditMarker(marker);
    }
  };

  return (
    <>
      {markers.map((marker) => (
        <Marker
          key={marker.id}
          position={[marker.latitude, marker.longitude]}
        >
          <Popup>
            <div className="marker-popup">
              <h3>{marker.title}</h3>
              {marker.description && <p>{marker.description}</p>}
              <div className="marker-meta">
                <span className="marker-category">{marker.category}</span>
                <span className="marker-author">
                  by {marker.createdByDisplayName}
                </span>
              </div>
              {canEdit(marker) && (
                <div className="marker-actions">
                  <button
                    className="btn-sm btn-edit"
                    onClick={() => handleEdit(marker)}
                    disabled={deletingId === marker.id}
                  >
                    Edit
                  </button>
                  <button
                    className="btn-sm btn-delete"
                    onClick={() => handleDelete(marker)}
                    disabled={deletingId === marker.id}
                  >
                    {deletingId === marker.id ? 'Deleting...' : 'Delete'}
                  </button>
                </div>
              )}
            </div>
          </Popup>
        </Marker>
      ))}
    </>
  );
};

export default MarkerLayer;
