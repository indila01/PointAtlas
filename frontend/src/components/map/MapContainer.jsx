import { useEffect, useRef } from 'react';
import { MapContainer, TileLayer, useMap, useMapEvents } from 'react-leaflet';
import { useMarkers } from '../../contexts/MarkerContext';
import MarkerLayer from './MarkerLayer';
import 'leaflet/dist/leaflet.css';
import './Map.css';

// Fix for default marker icon in production builds
import L from 'leaflet';
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';

let DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

L.Marker.prototype.options.icon = DefaultIcon;

const BoundsUpdater = () => {
  const { updateBounds } = useMarkers();
  const map = useMap();

  useMapEvents({
    moveend: () => {
      const bounds = map.getBounds();
      updateBounds({
        minLat: bounds.getSouth(),
        maxLat: bounds.getNorth(),
        minLng: bounds.getWest(),
        maxLng: bounds.getEast(),
      });
    },
  });

  // Update bounds on initial load
  useEffect(() => {
    const bounds = map.getBounds();
    updateBounds({
      minLat: bounds.getSouth(),
      maxLat: bounds.getNorth(),
      minLng: bounds.getWest(),
      maxLng: bounds.getEast(),
    });
  }, [map, updateBounds]);

  return null;
};

const Map = ({ onMapClick }) => {
  const defaultCenter = [40.7128, -74.006]; // New York City
  const defaultZoom = 13;

  return (
    <div className="map-wrapper">
      <MapContainer
        center={defaultCenter}
        zoom={defaultZoom}
        style={{ height: '100%', width: '100%' }}
        onClick={onMapClick}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <BoundsUpdater />
        <MarkerLayer />
      </MapContainer>
    </div>
  );
};

export default Map;
