import React, { useState } from "react";
import Background2DMap from "./Background2DMap";
import Background3DMap from "./Background3DMap";
import "../styles/ToggleMaps.css";

function ToggleMaps({
  setIsEvent,
  isEvent,
  setIsTouristSite,
  isTouristSite,
  setIsStory,
  isStory,
}) {
  const [showMap2D, setShowMap2D] = useState(false);

  const toggle = () => {
    setShowMap2D((prevState) => !prevState);
  };

  return (
    <div className="map-toggle-container">
      <label className="switch">
        <input type="checkbox" checked={showMap2D} onChange={toggle} />
        <span className="slider round"></span>
      </label>
      {showMap2D ? (
        <Background2DMap
          setIsEvent={setIsEvent}
          isEvent={isEvent}
          setIsTouristSite={setIsTouristSite}
          isTouristSite={isTouristSite}
          setIsStory={setIsStory}
          isStory={isStory}
        />
      ) : (
        <Background3DMap
          setIsEvent={setIsEvent}
          isEvent={isEvent}
          setIsTouristSite={setIsTouristSite}
          isTouristSite={isTouristSite}
          setIsStory={setIsStory}
          isStory={isStory}
        />
      )}
    </div>
  );
}

export default ToggleMaps;
