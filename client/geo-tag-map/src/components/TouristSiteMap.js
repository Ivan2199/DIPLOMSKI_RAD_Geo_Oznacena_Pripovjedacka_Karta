import React, { useEffect, useState, useCallback, useMemo } from "react";
import * as Cesium from "cesium";
import ReactDOMServer from "react-dom/server";
import ReactDOM from "react-dom";
import TouristSiteService from "../services/TouristSiteService";
import TouristSiteMapTile from "./TouristSiteMapTile";
import TouristSite from "./TouristSite";
import MapPin from "../components/MapPin";
import MapTileSlider from "./MapTileSlider";
import { debounce } from "lodash";
import "../styles/TouristSiteMap.css";
import { useNavigate } from "react-router-dom";
import { IoFilter } from "react-icons/io5";
import { v4 as uuidv4 } from "uuid";
import Filter from "./Filter";

function TouristSiteMap({
  viewerRef,
  isTouristSite,
  selectedTouristSiteId,
  setSelectedTouristSiteId,
}) {
  const [hoveredEntity, setHoveredEntity] = useState(null);
  const [touristSites, setTouristSites] = useState([]);
  const [locations, setLocations] = useState([]);
  const [geoLocations, setGeoLocations] = useState([]);
  const [addedPins, setAddedPins] = useState([]);
  const [isFilter, setIsFilter] = useState(false);
  const [selectedTouristSite, setSelectedTouristSite] = useState({});

  const defaultFilterForm = useMemo(
    () => ({
      pageSize: 1600,
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

  const fetchSelectedTouristSite = useCallback(async () => {
    try {
      const response = await TouristSiteService.getTouristSiteById(
        selectedTouristSiteId
      );
      console.log(selectedTouristSite);
      setSelectedTouristSite(response);
    } catch (error) {
      console.log("Error occurred while fetching tourist site", error);
    }
  }, [selectedTouristSiteId]);

  const fetchAndFilterTouristSites = useCallback(async () => {
    try {
      const response = await TouristSiteService.getTouristSitesFiltered(
        filterForm
      );

      if (response && Array.isArray(response.List)) {
        const touristSitesList = response.List;
        setTouristSites(touristSitesList);

        const uniqueLocationsMap = new Map();
        touristSitesList.forEach((touristSite) => {
          if (!uniqueLocationsMap.has(touristSite.Location.Id)) {
            uniqueLocationsMap.set(touristSite.Location.Id, {
              ...touristSite.Location,
              TouristSites: [],
            });
          }
          uniqueLocationsMap
            .get(touristSite.Location.Id)
            .TouristSites.push(touristSite);
        });

        const uniqueLocations = Array.from(uniqueLocationsMap.values());
        setLocations(uniqueLocations);

        const geoLocations = touristSitesList.flatMap((touristSite) =>
          touristSite.GeoLocations.map((geoLocation) => ({
            ...geoLocation,
            Location: { ...touristSite.Location },
          }))
        );
        setGeoLocations(geoLocations);
      } else {
        setTouristSites([]);
        setGeoLocations([]);
      }
    } catch (error) {
      console.error("Error fetching tourist sites:", error);
      setTouristSites([]);
      setGeoLocations([]);
    }
  }, [filterForm]);

  useEffect(() => {
    if (selectedTouristSiteId) {
      fetchSelectedTouristSite();
    } else {
      fetchAndFilterTouristSites();
    }
  }, [
    fetchAndFilterTouristSites,
    fetchSelectedTouristSite,
    selectedTouristSiteId,
    isTouristSite,
  ]);

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
    [viewerRef, isTouristSite]
  );

  useEffect(() => {
    if (isTouristSite && viewerRef.current) {
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
  }, [isTouristSite, handleMouseMove, viewerRef]);

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
        const touristSites = hoveredEntity.TouristSites.map((touristSite) => (
          <TouristSiteMapTile
            key={touristSite.id}
            photos={touristSite.Photos}
            name={touristSite.Name}
            description={touristSite.Description}
            hoursOpen={touristSite.HoursOpen}
            siteCategories={touristSite.SiteCategories}
          />
        ));
        ReactDOM.render(
          <div className="popup" style={popupStyle}>
            <MapTileSlider slides={touristSites} />
          </div>,
          document.getElementById("popup-root")
        );
      }
    } else {
      ReactDOM.unmountComponentAtNode(document.getElementById("popup-root"));
    }
  }, [hoveredEntity, viewerRef, isTouristSite]);

  const addPinsToMap = useCallback(
    (locations) => {
      if (viewerRef.current) {
        const customPinSvg = ReactDOMServer.renderToStaticMarkup(<MapPin />);
        const pinDataUrl = `data:image/svg+xml;base64,${btoa(customPinSvg)}`;

        const pins = locations.flatMap((location) =>
          location.TouristSites.flatMap((touristSite) =>
            touristSite.GeoLocations.map((geoLocation) => ({
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
              TouristSites: [touristSite],
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
            pickedObject.id.TouristSites
          ) {
            const handleClose = () => {
              ReactDOM.unmountComponentAtNode(
                document.getElementById("tourist-site-container-root")
              );
              setSelectedTouristSite(null);
              setSelectedTouristSiteId(uuidv4());
            };
            const pickedTouristSites = pickedObject.id.TouristSites.map(
              (touristSite) => (
                <TouristSite
                  key={touristSite.Id}
                  touristSite={touristSite}
                  photos={touristSite.Photos}
                />
              )
            );

            ReactDOM.render(
              <div className="tourist-site-overlay">
                <TouristSite
                  slides={pickedTouristSites}
                  onClose={handleClose}
                  goToGallery={goToGallery}
                />
              </div>,
              document.getElementById("tourist-site-container-root")
            );
          }
        }, Cesium.ScreenSpaceEventType.LEFT_CLICK);

        viewerRef.current.screenSpaceEventHandler.removeInputAction(
          Cesium.ScreenSpaceEventType.LEFT_DOUBLE_CLICK
        );

        const zoomListener = () => {
          const zoom = viewerRef.current.camera.positionCartographic.height;
          addedPinsEntities.forEach((pin) => {
            pin.show = zoom < 50000000;
          });
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

  const addSelectedTouristSitePinToMap = useCallback(() => {
    if (viewerRef.current && selectedTouristSite) {
      const customPinSvg = ReactDOMServer.renderToStaticMarkup(<MapPin />);
      const pinDataUrl = `data:image/svg+xml;base64,${btoa(customPinSvg)}`;

      const geoLocation = selectedTouristSite.GeoLocations?.[0];
      if (!geoLocation) return;

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
        TouristSites: [selectedTouristSite],
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
          document.getElementById("tourist-site-container-root")
        );
        setSelectedTouristSite(null);
        setSelectedTouristSiteId(uuidv4());
        viewerRef.current.camera.flyTo({
          destination: Cesium.Cartesian3.fromDegrees(4, 20, 19000000),
        });
        const allPins = viewerRef.current.entities.values;
        setAddedPins(allPins);
      };

      const pickedTouristSites = [selectedTouristSite].map((touristSite) => (
        <TouristSite
          key={touristSite.Id}
          touristSite={touristSite}
          photos={touristSite.Photos}
        />
      ));

      ReactDOM.render(
        <div className="tourist-site-overlay">
          <TouristSite
            slides={pickedTouristSites}
            onClose={handleClose}
            goToGallery={goToGallery}
          />
        </div>,
        document.getElementById("tourist-site-container-root")
      );
    }
  }, [viewerRef, selectedTouristSite]);

  useEffect(() => {
    if (selectedTouristSiteId && selectedTouristSite) {
      addSelectedTouristSitePinToMap();
    } else if (locations.length > 0) {
      addedPins.forEach((pin) => viewerRef.current.entities.remove(pin));
      setAddedPins([]);
      addPinsToMap(locations);
    }
  }, [
    locations,
    addPinsToMap,
    isTouristSite,
    selectedTouristSiteId,
    selectedTouristSite,
    addSelectedTouristSitePinToMap,
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

  const handleSearchChange = (site) => {
    const { value } = site.target;
    setSearchInput(value);
    setFilterForm((prevForm) => ({ ...prevForm, searchKeyword: value }));
  };

  useEffect(() => {
    const debouncedFetchAndFilterTouristSites = debounce(
      fetchAndFilterTouristSites,
      300
    );
    debouncedFetchAndFilterTouristSites();
    return () => {
      debouncedFetchAndFilterTouristSites.cancel();
    };
  }, [fetchAndFilterTouristSites, filterForm.searchKeyword]);

  const navigate = useNavigate();

  const goToGallery = (id, imageOf) => {
    navigate(`/gallery/${id}`, { state: { imageOf } });
  };

  return (
    <div className="tourist-site-map-container">
      <div className="filter-tourist-sites-map">
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
          <div className="hovered-container">Filter Tourist Sites</div>
        </div>

        {isFilter && (
          <Filter
            isOpen={isFilter}
            filterOf={"sites"}
            toggleFilter={toggleFilter}
            onFilterChange={handleFilterChange}
            onClearFilter={handleClearFilter}
          />
        )}
      </div>
    </div>
  );
}

export default TouristSiteMap;
