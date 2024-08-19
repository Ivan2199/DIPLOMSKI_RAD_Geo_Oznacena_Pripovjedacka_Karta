import React, { useEffect, useState, useCallback, useMemo } from "react";
import * as Cesium from "cesium";
import ReactDOMServer from "react-dom/server";
import ReactDOM from "react-dom";
import EventMapTile from "../components/EventMapTile";
import EventService from "../services/EventService";
import MapPin from "../components/MapPin";
import MapTileSlider from "./MapTileSlider";
import Event from "./Event";
import { debounce } from "lodash";
import { IoFilter } from "react-icons/io5";
import Filter from "./Filter";
import { v4 as uuidv4 } from "uuid";
import "../styles/EventMap.css";

const EventMap = ({
  viewerRef,
  isEvent,
  selectedEventId,
  setSelectedEventId,
  showingMapType,
}) => {
  const [hoveredEntity, setHoveredEntity] = useState(null);
  const [events, setEvents] = useState([]);
  const [locations, setLocations] = useState([]);
  const [geoLocations, setGeoLocations] = useState([]);
  const [addedPins, setAddedPins] = useState([]);
  const [isFilter, setIsFilter] = useState(false);
  const [selectedEvent, setSelectedEvent] = useState(null);

  const defaultFilterForm = useMemo(
    () => ({
      pageSize: 3100,
      orderBy: "StartDate",
      sortOrder: "ASC",
      name: "",
      startDate: null,
      type: "",
      isAccessibleForFree: false,
      searchKeyword: "",
      country: "",
      city: "",
    }),
    []
  );

  const [filterForm, setFilterForm] = useState(defaultFilterForm);

  const [searchInput, setSearchInput] = useState(
    filterForm.searchKeyword || ""
  );

  const fetchSelectedEvent = useCallback(async () => {
    try {
      console.log(selectedEventId);
      const response = await EventService.getEventById(selectedEventId);
      setSelectedEvent(response);
    } catch (error) {
      console.log("Error occurred while fetching an event", error);
    }
  }, [selectedEventId]);

  const fetchAndFilterEvents = useCallback(async () => {
    try {
      const response = await EventService.getEventsFiltered(filterForm);

      if (response && Array.isArray(response.List)) {
        const eventsList = response.List;
        setEvents(eventsList);

        const uniqueLocationsMap = new Map();
        eventsList.forEach((event) => {
          if (!uniqueLocationsMap.has(event.Location.Id)) {
            uniqueLocationsMap.set(event.Location.Id, {
              ...event.Location,
              Events: [],
            });
          }
          uniqueLocationsMap.get(event.Location.Id).Events.push(event);
        });
        const uniqueLocations = Array.from(uniqueLocationsMap.values());
        setLocations(uniqueLocations);

        const geoLocations = eventsList.flatMap((event) => {
          return event.GeoLocations.map((geoLocation) => ({
            ...geoLocation,
            Location: { ...event.Location },
          }));
        });
        setGeoLocations(geoLocations);
      } else {
        setEvents([]);
        setGeoLocations([]);
      }
    } catch (error) {
      console.error("Error fetching events:", error);
      setEvents([]);
      setGeoLocations([]);
    }
  }, [filterForm]);

  useEffect(() => {
    if (selectedEventId) {
      fetchSelectedEvent();
    } else {
      fetchAndFilterEvents();
    }
  }, [fetchAndFilterEvents, fetchSelectedEvent, selectedEventId, isEvent]);

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
    [viewerRef, isEvent]
  );

  useEffect(() => {
    if (isEvent && viewerRef.current) {
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
  }, [isEvent, handleMouseMove, viewerRef]);

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
        const events = hoveredEntity.Events.map((event) => (
          <EventMapTile
            key={event.id}
            name={event.Name}
            type={event.Type}
            startDate={event.StartDate}
            endDate={event.EndDate}
            image={event.Image}
          />
        ));
        ReactDOM.render(
          <div className="popup" style={popupStyle}>
            <MapTileSlider slides={events} />
          </div>,
          document.getElementById("popup-root")
        );
      }
    } else {
      ReactDOM.unmountComponentAtNode(document.getElementById("popup-root"));
    }
  }, [hoveredEntity, viewerRef, isEvent]);

  const addPinsToMap = useCallback(
    (locations) => {
      if (viewerRef.current) {
        const customPinSvg = ReactDOMServer.renderToStaticMarkup(<MapPin />);
        const pinDataUrl = `data:image/svg+xml;base64,${btoa(customPinSvg)}`;

        const pins = locations.flatMap((location) =>
          location.Events.flatMap((event) =>
            event.GeoLocations.map((geoLocation) => ({
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
              Events: [event],
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
            pickedObject.id.Events
          ) {
            const handleClose = () => {
              ReactDOM.unmountComponentAtNode(
                document.getElementById("event-container-root")
              );
            };
            const pickedEvents = pickedObject.id.Events.map((event) => (
              <Event key={event.id} event={event} />
            ));

            ReactDOM.render(
              <div className="event-overlay">
                <Event slides={pickedEvents} onClose={handleClose} />
              </div>,
              document.getElementById("event-container-root")
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

  const addSelectedEventPinToMap = useCallback(() => {
    if (viewerRef.current && selectedEvent) {
      const customPinSvg = ReactDOMServer.renderToStaticMarkup(<MapPin />);
      const pinDataUrl = `data:image/svg+xml;base64,${btoa(customPinSvg)}`;

      const geoLocation = selectedEvent.GeoLocations[0];

      const pin = {
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
        Events: [selectedEvent],
      };

      const addedPinEntity = viewerRef.current.entities.add(pin);
      setAddedPins([addedPinEntity]);

      viewerRef.current.selectionIndicator.viewModel.selectionIndicatorElement.style.visibility =
        "hidden";

      viewerRef.current.camera.flyTo({
        destination: Cesium.Cartesian3.fromDegrees(
          geoLocation.Longitude,
          geoLocation.Latitude,
          1000
        ),
      });

      const handleClose = () => {
        ReactDOM.unmountComponentAtNode(
          document.getElementById("event-container-root")
        );

        setSelectedEvent(null);
        setSelectedEventId(uuidv4());

        viewerRef.current.camera.flyTo({
          destination: Cesium.Cartesian3.fromDegrees(4, 20, 19000000),
        });

        const allPins = viewerRef.current.entities.values;
        setAddedPins(allPins);
      };

      const pickedEvent = (
        <Event key={selectedEvent.id} event={selectedEvent} />
      );

      ReactDOM.render(
        <div className="event-overlay">
          <Event slides={pickedEvent} onClose={handleClose} />
        </div>,
        document.getElementById("event-container-root")
      );
    }
  }, [viewerRef, selectedEvent]);

  useEffect(() => {
    if (selectedEventId && selectedEvent) {
      addSelectedEventPinToMap();
    } else if (locations.length > 0) {
      addedPins.forEach((pin) => viewerRef.current.entities.remove(pin));
      setAddedPins([]);
      addPinsToMap(locations);
    }
  }, [
    locations,
    addPinsToMap,
    isEvent,
    selectedEventId,
    selectedEvent,
    addSelectedEventPinToMap,
  ]);

  const toggleFilter = () => {
    setIsFilter(!isFilter);
  };

  const handleFilterChange = (updatedFilters) => {
    setFilterForm(updatedFilters);
  };

  const handleClearFilter = () => {
    setFilterForm(defaultFilterForm);
  };

  const handleSearchChange = (event) => {
    const { value } = event.target;
    setSearchInput(value);
    setFilterForm((prevForm) => ({ ...prevForm, searchKeyword: value }));
  };

  useEffect(() => {
    const debouncedFetchAndFilterEvents = debounce(fetchAndFilterEvents, 300);
    debouncedFetchAndFilterEvents();
    return () => {
      debouncedFetchAndFilterEvents.cancel();
    };
  }, [fetchAndFilterEvents, filterForm.searchKeyword]);

  return (
    <div className="event-map-container">
      <div className="filter-events-map">
        <div className="search-box-map">
          <label>
            <input
              type="text"
              name="searchKeyword"
              placeholder="Search..."
              value={searchInput}
              onChange={handleSearchChange}
            />
          </label>
        </div>
        <div className="filter-button" onClick={toggleFilter}>
          <IoFilter className="filter-logo" />
          <div className="hovered-container">Filter Events</div>
        </div>

        {isFilter && (
          <Filter
            isOpen={isFilter}
            filterOf={"events"}
            toggleFilter={toggleFilter}
            onFilterChange={handleFilterChange}
            onClearFilter={handleClearFilter}
          />
        )}
      </div>
    </div>
  );
};

export default EventMap;
