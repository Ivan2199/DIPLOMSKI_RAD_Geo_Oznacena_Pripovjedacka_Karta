import React, { useEffect, useState, useCallback, useMemo } from "react";
import * as Cesium from "cesium";
import ReactDOMServer from "react-dom/server";
import ReactDOM from "react-dom";
import StoryService from "../services/StoryService";
import StoryMapTile from "./StoryMapTile";
import Story from "./Story";
import MapPin from "../components/MapPin";
import MapTileSlider from "./MapTileSlider";
import { debounce } from "lodash";
import { useNavigate } from "react-router-dom";
import "../styles/TouristSiteMap.css";
import CreateStoryForm from "./CreateStoryForm";
import "../styles/StoryMap.css";

function StoryMap({ viewerRef, isStory }) {
  const [hoveredEntity, setHoveredEntity] = useState(null);
  const [stories, setStories] = useState([]);
  const [locations, setLocations] = useState([]);
  const [geoLocations, setGeoLocations] = useState([]);
  const [addedPins, setAddedPins] = useState([]);
  const [isStoryActive, setIsStoryActive] = useState(false);
  const [pageSize, setPageSize] = useState(5000);
  const [pageNumber, setPageNumber] = useState(1);
  const defaultFilterForm = useMemo(
    () => ({
      orderBy: "Date",
      sortOrder: "DESC",
      pageSize: 5000,
      pageNumber: 1,
    }),
    []
  );

  const [filterForm, setFilterForm] = useState(defaultFilterForm);

  const fetchAndFilterStories = useCallback(async () => {
    try {
      const response = await StoryService.fetchStories(filterForm);

      if (response.List && Array.isArray(response.List)) {
        const storiesList = response.List;
        setStories(storiesList);

        const uniqueLocationsMap = new Map();
        storiesList.forEach((story) => {
          if (!uniqueLocationsMap.has(story.Location.Id)) {
            uniqueLocationsMap.set(story.Location.Id, {
              ...story.Location,
              Stories: [],
            });
          }
          uniqueLocationsMap.get(story.Location.Id).Stories.push(story);
        });
        const uniqueLocations = Array.from(uniqueLocationsMap.values());
        setLocations(uniqueLocations);

        const geoLocations = storiesList.flatMap((story) => {
          return story.GeoLocations.map((geoLocation) => ({
            ...geoLocation,
            Location: { ...story.Location },
          }));
        });
        setGeoLocations(geoLocations);
      } else {
        setStories([]);
        setGeoLocations([]);
      }
    } catch (error) {
      console.error("Error fetching stories:", error);
      setStories([]);
      setGeoLocations([]);
    }
  }, []);

  const renderCreateStoryForm = (longitude, latitude) => {
    const formContainer = document.getElementById(
      "create-story-form-container"
    );
    ReactDOM.render(
      <CreateStoryForm longitude={longitude} latitude={latitude} />,
      formContainer
    );
    formContainer.style.display = "block";
  };

  useEffect(() => {
    const handleMapClick = (click) => {
      if (!Cesium.defined(click.position)) {
        console.error("Click outside visible part of the map.");
        return;
      }

      if (isStoryActive) {
        alert("You must exit the story view to add a new story.");
        return;
      }

      const ray = viewerRef.current.camera.getPickRay(click.position);
      const intersection = viewerRef.current.scene.globe.pick(
        ray,
        viewerRef.current.scene
      );
      if (!Cesium.defined(intersection)) {
        console.error("Unable to retrieve coordinates.");
        return;
      }

      const cartographicPosition =
        Cesium.Cartographic.fromCartesian(intersection);
      const longitude = Cesium.Math.toDegrees(
        cartographicPosition.longitude
      ).toFixed(7);
      const latitude = Cesium.Math.toDegrees(
        cartographicPosition.latitude
      ).toFixed(7);

      console.log("Longitude:", longitude, "Latitude:", latitude);
      renderCreateStoryForm(longitude, latitude);
    };

    if (isStory && viewerRef.current) {
      const handler = new Cesium.ScreenSpaceEventHandler(
        viewerRef.current.scene.canvas
      );

      handler.setInputAction(
        handleMapClick,
        Cesium.ScreenSpaceEventType.LEFT_DOUBLE_CLICK
      );

      return () => {
        handler.destroy();
      };
    }
  }, [isStory, viewerRef, isStoryActive]);

  useEffect(() => {
    fetchAndFilterStories();
  }, [fetchAndFilterStories, isStory]);

  const handleMouseMove = useCallback(
    debounce((movement) => {
      if (viewerRef.current && viewerRef.current.scene) {
        const pickedObject = viewerRef.current.scene.pick(movement.endPosition);
        if (Cesium.defined(pickedObject) && pickedObject.id) {
          setHoveredEntity(pickedObject.id);
        } else {
          setHoveredEntity(null);
        }
      }
    }, 100),
    [viewerRef, isStory]
  );

  useEffect(() => {
    if (isStory && viewerRef.current) {
      const handler = new Cesium.ScreenSpaceEventHandler(
        viewerRef.current.scene.canvas
      );
      handler.setInputAction(
        handleMouseMove,
        Cesium.ScreenSpaceEventType.MOUSE_MOVE
      );

      return () => {
        handler.destroy();
      };
    }
  }, [isStory, handleMouseMove, viewerRef]);

  useEffect(() => {
    if (hoveredEntity && hoveredEntity.name === "Custom Pin") {
      const entityPosition =
        viewerRef.current.scene.cartesianToCanvasCoordinates(
          hoveredEntity.position._value
        );
      if (entityPosition) {
        const { x, y } = entityPosition;
        const popupStyle = {
          position: "absolute",
          top: y + "px",
          left: x + "px",
        };
        const stories = hoveredEntity.Stories.map((story) => (
          <StoryMapTile
            key={story.id}
            photos={story.Photos}
            text={story.Text}
            user={story.User}
            date={story.DateTime}
          />
        ));
        ReactDOM.render(
          <div className="popup" style={popupStyle}>
            <MapTileSlider slides={stories} />
          </div>,
          document.getElementById("popup-root")
        );
      }
    } else {
      ReactDOM.unmountComponentAtNode(document.getElementById("popup-root"));
    }
  }, [hoveredEntity, viewerRef, isStory]);

  const addPinsToMap = useCallback(
    (locations) => {
      if (viewerRef.current) {
        const customPinSvg = ReactDOMServer.renderToStaticMarkup(<MapPin />);
        const pinDataUrl = `data:image/svg+xml;base64,${btoa(customPinSvg)}`;

        const pins = locations.flatMap((location) =>
          location.Stories.flatMap((story) =>
            story.GeoLocations.map((geoLocation) => ({
              id: Cesium.createGuid(),
              name: "Custom Pin",
              position: Cesium.Cartesian3.fromDegrees(
                geoLocation.Longitude,
                geoLocation.Latitude
              ),
              billboard: {
                image: pinDataUrl,
                verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
              },
              Stories: [story],
            }))
          )
        );

        const addedPinsEntities = pins.map((pin) =>
          viewerRef.current.entities.add(pin)
        );
        setAddedPins(addedPinsEntities);

        viewerRef.current.selectionIndicator.viewModel.selectionIndicatorElement.style.visibility =
          "hidden";

        const handler = new Cesium.ScreenSpaceEventHandler(
          viewerRef.current.scene.canvas
        );

        handler.setInputAction((click) => {
          const pickedObject = viewerRef.current.scene.pick(click.position);
          if (
            Cesium.defined(pickedObject) &&
            pickedObject.id &&
            pickedObject.id.Stories
          ) {
            setIsStoryActive(true);

            const handleClose = () => {
              setIsStoryActive(false);
              ReactDOM.unmountComponentAtNode(
                document.getElementById("story-container-root")
              );
            };
            const pickedStories = pickedObject.id.Stories.map((story) => (
              <Story key={story.id} story={story} photos={story.Photos} />
            ));

            ReactDOM.render(
              <div className="story-overlay">
                <Story
                  slides={pickedStories}
                  onClose={handleClose}
                  resetPins={addPinsToMap}
                  goToGallery={goToGallery}
                />
              </div>,
              document.getElementById("story-container-root")
            );
          }
        }, Cesium.ScreenSpaceEventType.LEFT_CLICK);

        viewerRef.current.screenSpaceEventHandler.removeInputAction(
          Cesium.ScreenSpaceEventType.LEFT_DOUBLE_CLICK
        );

        const zoomListener = () => {
          const zoom = viewerRef.current.camera.positionCartographic.height;
          if (zoom < 50000000) {
            addedPinsEntities.forEach((pin) => {
              pin.show = true;
            });
          } else {
            addedPinsEntities.forEach((pin) => {
              pin.show = false;
            });
          }
        };

        viewerRef.current.camera.moveEnd.addEventListener(zoomListener);

        return () => {
          handler.destroy();
          viewerRef.current.camera.moveEnd.removeEventListener(zoomListener);
        };
      }
    },
    [viewerRef]
  );

  useEffect(() => {
    if (locations.length > 0) {
      addPinsToMap(locations);
    }
  }, [locations, addPinsToMap, isStory]);

  const navigate = useNavigate();

  const goToGallery = (id, imageOf) => {
    navigate(`/gallery/${id}`, { state: { imageOf } });
  };

  return (
    <div className="story-map-container">
      <div
        id="create-story-form-container"
        className="create-story-form-container"
      ></div>
      <div id="story-container-root"></div>
    </div>
  );
}

export default StoryMap;
