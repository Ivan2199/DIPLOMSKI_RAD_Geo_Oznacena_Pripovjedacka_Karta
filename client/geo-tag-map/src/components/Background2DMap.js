import React, { useState, useEffect } from "react";
import BingMapsReact from "bingmaps-react";
import "../styles/Background2DMap.css";

function Background2DMap() {
  const [userLocation, setUserLocation] = useState(null);

  const pushPins = userLocation ? [{ center: userLocation }] : [];

  return (
    <div className="map-container">
      <BingMapsReact
        bingMapsKey={process.env.REACT_APP_BING_MAPS_API_KEY}
        className="map"
        mapOptions={{
          navigationBarMode: "square",
        }}
        viewOptions={{
          mapTypeId: "aerial",
        }}
        pushPins={pushPins}
      />
    </div>
  );
}

export default Background2DMap;
