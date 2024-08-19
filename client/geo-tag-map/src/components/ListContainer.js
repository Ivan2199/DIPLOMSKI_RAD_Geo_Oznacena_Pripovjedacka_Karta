import React, { useEffect, useState, useCallback } from "react";
import PropTypes from "prop-types";
import EventTile from "./EventTile";
import TouristLocationTile from "./TouristLocationTile";
import Filter from "./Filter";
import "../styles/ListContainer.css";
import { IoFilter } from "react-icons/io5";
import debounce from "lodash/debounce";

function ListContainer({
  list = [],
  listOf,
  filters,
  onFilterChange,
  showFilter,
  toggleFilter,
  setSelectedEventId,
  setSelectedTouristSiteId,
}) {
  const [filteredData, setFilteredData] = useState(list);
  const [searchInput, setSearchInput] = useState(filters.searchKeyword || "");

  const debouncedFilterData = useCallback(
    debounce((updatedFilters) => {
      let data = Array.isArray(list) ? [...list] : [];

      if (listOf === "events") {
        if (updatedFilters.name) {
          data = data.filter((item) =>
            (item.Name ?? "")
              .toLowerCase()
              .includes(updatedFilters.name.toLowerCase())
          );
        }

        if (updatedFilters.startDate) {
          data = data.filter(
            (item) =>
              new Date(item.StartDate) >= new Date(updatedFilters.startDate)
          );
        }

        if (updatedFilters.type) {
          data = data.filter((item) =>
            (item.Type ?? "")
              .toLowerCase()
              .includes(updatedFilters.type.toLowerCase())
          );
        }

        if (updatedFilters.isAccessibleForFree) {
          data = data.filter(
            (item) =>
              item.IsAccessibleForFree === updatedFilters.isAccessibleForFree
          );
        }

        if (updatedFilters.searchKeyword) {
          data = data.filter(
            (item) =>
              (item.Name ?? "")
                .toLowerCase()
                .includes(updatedFilters.searchKeyword.toLowerCase()) ||
              (item.Description ?? "")
                .toLowerCase()
                .includes(updatedFilters.searchKeyword.toLowerCase())
          );
        }

        if (updatedFilters.country) {
          data = data.filter((item) =>
            (item.Location?.Country ?? "")
              .toLowerCase()
              .includes(updatedFilters.country.toLowerCase())
          );
        }

        if (updatedFilters.city) {
          data = data.filter((item) =>
            (item.Location?.City ?? "")
              .toLowerCase()
              .includes(updatedFilters.city.toLowerCase())
          );
        }

        if (updatedFilters.orderBy) {
          data.sort((a, b) => {
            if (updatedFilters.orderBy === "StartDate") {
              return updatedFilters.sortOrder === "ASC"
                ? new Date(a.StartDate) - new Date(b.StartDate)
                : new Date(b.StartDate) - new Date(a.StartDate);
            } else {
              return updatedFilters.sortOrder === "ASC"
                ? (a[updatedFilters.orderBy] ?? "").localeCompare(
                    b[updatedFilters.orderBy] ?? ""
                  )
                : (b[updatedFilters.orderBy] ?? "").localeCompare(
                    a[updatedFilters.orderBy] ?? ""
                  );
            }
          });
        }
      } else if (listOf === "sites") {
        if (updatedFilters.name) {
          data = data.filter((item) =>
            (item.Name ?? "")
              .toLowerCase()
              .includes(updatedFilters.name.toLowerCase())
          );
        }

        if (updatedFilters.popularity) {
          data = data.filter(
            (item) => item.Popularity >= parseInt(updatedFilters.popularity, 10)
          );
        }

        if (updatedFilters.searchKeyword) {
          data = data.filter(
            (item) =>
              (item.Name ?? "")
                .toLowerCase()
                .includes(updatedFilters.searchKeyword.toLowerCase()) ||
              (item.Description ?? "")
                .toLowerCase()
                .includes(updatedFilters.searchKeyword.toLowerCase())
          );
        }

        if (updatedFilters.country) {
          data = data.filter((item) =>
            (item.Location?.Country ?? "")
              .toLowerCase()
              .includes(updatedFilters.country.toLowerCase())
          );
        }

        if (updatedFilters.city) {
          data = data.filter((item) =>
            (item.Location?.City ?? "")
              .toLowerCase()
              .includes(updatedFilters.city.toLowerCase())
          );
        }

        if (updatedFilters.orderBy) {
          data.sort((a, b) => {
            if (updatedFilters.orderBy === "Popularity") {
              return updatedFilters.sortOrder === "ASC"
                ? a.Popularity - b.Popularity
                : b.Popularity - a.Popularity;
            } else {
              return updatedFilters.sortOrder === "ASC"
                ? (a[updatedFilters.orderBy] ?? "").localeCompare(
                    b[updatedFilters.orderBy] ?? ""
                  )
                : (b[updatedFilters.orderBy] ?? "").localeCompare(
                    a[updatedFilters.orderBy] ?? ""
                  );
            }
          });
        }
      }

      setFilteredData(data);
    }, 300),
    [list, listOf]
  );

  useEffect(() => {
    debouncedFilterData(filters);
    return () => {
      debouncedFilterData.cancel();
    };
  }, [filters, debouncedFilterData]);

  const handleSearchChange = (event) => {
    const { value } = event.target;
    setSearchInput(value);
    onFilterChange({ ...filters, searchKeyword: value });
  };

  const renderEvents = () => (
    <div className="list-container">
      <div className="list-title">
        <p>{listOf === "likedevents" ? "Most Popular Events" : "Events"}</p>
        {listOf === "events" && (
          <div className="search-box-filter">
            <div className="search-box">
              <label>
                Search
                <input
                  type="text"
                  name="searchKeyword"
                  value={searchInput}
                  onChange={handleSearchChange}
                />
              </label>
            </div>
            <div className="filter-button" onClick={toggleFilter}>
              <IoFilter /> Filter
            </div>
          </div>
        )}
      </div>
      <Filter
        isOpen={showFilter}
        toggleFilter={toggleFilter}
        onFilterChange={onFilterChange}
        filterOf={"events"}
      />
      {filteredData.length > 0 ? (
        filteredData.map((event) => (
          <div key={event.Id} className="list-element-card">
            <EventTile
              id={event.Id}
              name={event.Name}
              url={event.Url}
              image={event.Image}
              startDate={event.StartDate}
              endDate={event.EndDate}
              type={event.Type}
              country={event.Location?.Country}
              city={event.Location?.City}
              address={event.Location?.Address}
              setSelectedEventId={setSelectedEventId}
            />
          </div>
        ))
      ) : (
        <div>There are no events!</div>
      )}
    </div>
  );

  const renderTouristSites = () => (
    <div className="list-container">
      <div className="list-title">
        <p>
          {listOf === "likedsites"
            ? "Most Popular Tourist Sites"
            : "Tourist Sites"}
        </p>
        {listOf === "sites" && (
          <div className="search-box-filter">
            <div className="search-box">
              <label>
                Search
                <input
                  type="text"
                  name="searchKeyword"
                  value={searchInput}
                  onChange={handleSearchChange}
                />
              </label>
            </div>
            <div className="filter-button" onClick={toggleFilter}>
              <IoFilter /> Filter
            </div>
          </div>
        )}
      </div>
      <Filter
        isOpen={showFilter}
        toggleFilter={toggleFilter}
        onFilterChange={onFilterChange}
        filterOf={"sites"}
      />
      {filteredData.length > 0 ? (
        filteredData.map((touristSite) => (
          <div key={touristSite.Id} className="list-element-card">
            <TouristLocationTile
              id={touristSite.Id}
              name={touristSite.Name}
              images={touristSite.Photos}
              popularity={touristSite.Popularity}
              description={touristSite.Description}
              country={touristSite.Location?.Country}
              city={touristSite.Location?.City}
              address={touristSite.Location?.Address}
              hours={touristSite.HoursOpen}
              setSelectedTouristSiteId={setSelectedTouristSiteId}
            />
          </div>
        ))
      ) : (
        <div>There are no tourist sites!</div>
      )}
    </div>
  );

  return (
    <div>
      {listOf === "likedevents" || listOf === "events"
        ? renderEvents()
        : renderTouristSites()}
    </div>
  );
}

ListContainer.propTypes = {
  list: PropTypes.array.isRequired,
  listOf: PropTypes.string.isRequired,
  filters: PropTypes.object.isRequired,
  onFilterChange: PropTypes.func.isRequired,
  showFilter: PropTypes.bool.isRequired,
  toggleFilter: PropTypes.func.isRequired,
};

export default ListContainer;
