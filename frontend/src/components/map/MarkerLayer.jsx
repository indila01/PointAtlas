import { Marker, Popup } from 'react-leaflet';
import { useMarkers } from '../../contexts/MarkerContext';
import { useAuth } from '../../contexts/AuthContext';

const MarkerLayer = () => {
  const { markers } = useMarkers();
  const { user, isAdmin } = useAuth();

  const canEdit = (marker) => {
    if (!user) return false;
    return marker.createdById === user.id || isAdmin();
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
                    onClick={() => {
                      // Will implement edit functionality
                      console.log('Edit marker:', marker.id);
                    }}
                  >
                    Edit
                  </button>
                  <button
                    className="btn-sm btn-delete"
                    onClick={() => {
                      // Will implement delete functionality
                      console.log('Delete marker:', marker.id);
                    }}
                  >
                    Delete
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
