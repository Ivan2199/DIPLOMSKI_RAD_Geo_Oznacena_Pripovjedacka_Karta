import React, { useEffect, useRef, useState, useCallback } from "react";
import * as Cesium from "cesium";
import { useNavigate, useLocation } from "react-router-dom";
import "../styles/Background3DMap.css";
import EventMap from "../components/EventMap";
import MapDataPicker from "./MapDataPicker";
import TouristSiteMap from "./TouristSiteMap";
import StoryMap from "./StoryMap";

const Background3DMap = ({
  setIsEvent,
  isEvent,
  setIsTouristSite,
  isTouristSite,
  setIsStory,
  isStory,
  selectedEventId,
  setSelectedEventId,
  selectedTouristSiteId,
  setSelectedTouristSiteId,
  mapType,
}) => {
  const mapContainer = useRef(null);
  const viewerRef = useRef(null);
  const [isViewerReady, setIsViewerReady] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const initializeMap = async () => {
      Cesium.Ion.defaultAccessToken = process.env.REACT_APP_CESIUM_MAPS_API_KEY;

      viewerRef.current = new Cesium.Viewer(mapContainer.current, {
        animation: false,
        baseLayerPicker: false,
        fullscreenButton: false,
        geocoder: false,
        homeButton: false,
        infoBox: false,
        sceneModePicker: false,
        selectionIndicator: true,
        timeline: false,
        navigationHelpButton: false,
        navigationInstructionsInitiallyVisible: false,
      });

      try {
        const imageryProvider = await Cesium.IonImageryProvider.fromAssetId(3);
        viewerRef.current.imageryLayers.addImageryProvider(imageryProvider);
      } catch (error) {
        console.error("Error adding imagery provider:", error);
      }

      viewerRef.current.camera.setView({
        destination: Cesium.Cartesian3.fromDegrees(4, 20, 19000000),
      });

      startEarthRotationAnimation();

      setIsViewerReady(true);
    };

    initializeMap();

    if (mapType === "site") {
      setIsEvent(false);
      setIsStory(false);
      setIsTouristSite(true);
    } else if (mapType === "event") {
      setIsEvent(true);
      setIsStory(false);
      setIsTouristSite(false);
    }

    navigate(location.pathname, { replace: true });

    return () => {
      if (viewerRef.current) {
        viewerRef.current.destroy();
        viewerRef.current = null;
      }
    };
  }, [
    mapType,
    navigate,
    setIsEvent,
    setIsStory,
    setIsTouristSite,
    location.pathname,
  ]);

  useEffect(() => {
    if (!isEvent || !isTouristSite || !isStory) {
      removeAllPinsFromMap();
      navigate(location.pathname, { replace: true });
    }
  }, [isEvent, isTouristSite, isStory, navigate, location.pathname]);

  const removeAllPinsFromMap = useCallback(() => {
    if (viewerRef.current) {
      viewerRef.current.entities.removeAll();
    }
  }, []);

  const startEarthRotationAnimation = () => {
    if (viewerRef.current) {
      const clock = viewerRef.current.clock;
      clock.shouldAnimate = true;
      clock.clockRange = Cesium.ClockRange.UNBOUNDED;
      clock.multiplier = 150;
    }
  };

  return (
    <div className="map-container" ref={mapContainer}>
      <MapDataPicker
        isEvent={isEvent}
        setIsEvent={setIsEvent}
        isTouristSite={isTouristSite}
        setIsTouristSite={setIsTouristSite}
        isStory={isStory}
        setIsStory={setIsStory}
      />
      {isEvent && isViewerReady && (
        <div>
          <EventMap
            isEvent={isEvent}
            viewerRef={viewerRef}
            selectedEventId={selectedEventId}
            setSelectedEventId={setSelectedEventId}
          />
          <div id="popup-root"></div>
          <div id="event-container-root"></div>
        </div>
      )}
      {isTouristSite && isViewerReady && (
        <div>
          <TouristSiteMap
            isTouristSite={isTouristSite}
            viewerRef={viewerRef}
            selectedTouristSiteId={selectedTouristSiteId}
            setSelectedTouristSiteId={setSelectedTouristSiteId}
          />
          <div id="popup-root"></div>
          <div id="tourist-site-container-root"></div>
        </div>
      )}
      {isStory && isViewerReady && (
        <div>
          <StoryMap isStory={isStory} viewerRef={viewerRef} />
          <div id="popup-root"></div>
          <div id="story-container-root"></div>
        </div>
      )}
    </div>
  );
};

export default Background3DMap;
