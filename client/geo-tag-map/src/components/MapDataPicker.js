import React from "react";
import "../styles/MapDataPicker.css";

function MapDataPicker({
  isEvent,
  setIsEvent,
  isTouristSite,
  setIsTouristSite,
  isStory,
  setIsStory,
}) {
  const handleEventClick = () => {
    setIsEvent(true);
    setIsTouristSite(false);
    setIsStory(false);
  };

  const handleTouristSiteClick = () => {
    setIsEvent(false);
    setIsTouristSite(true);
    setIsStory(false);
  };

  const handleStoryClick = () => {
    setIsEvent(false);
    setIsTouristSite(false);
    setIsStory(true);
  };

  return (
    <div className="map-data-picker-container">
      <div
        className={isEvent ? "picked-container" : "not-picked-container"}
        onClick={handleEventClick}
      >
        <img src="/assets/eventsMusicLogo.png" alt="Events Logo" />
        <div className="hovered-container">Događaji</div>
      </div>
      <div
        className={isTouristSite ? "picked-container" : "not-picked-container"}
        onClick={handleTouristSiteClick}
      >
        <img src="/assets/touristSitesLogo.png" alt="Tourist Sites Logo" />
        <div className="hovered-container">Turističke Lokacije</div>
      </div>
      <div
        className={isStory ? "picked-container" : "not-picked-container"}
        onClick={handleStoryClick}
      >
        <img src="/assets/storiesLogo.png" alt="Stories Logo" />
        <div className="hovered-container">Priče Korisnika</div>
      </div>
    </div>
  );
}

export default MapDataPicker;
