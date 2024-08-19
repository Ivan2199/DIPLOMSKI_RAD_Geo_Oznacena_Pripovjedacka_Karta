import React, { useState } from "react";
import "../styles/Filter.css";

function Filter({ isOpen, toggleFilter, onFilterChange, filterOf }) {
  const filterClassName = isOpen ? "slide-in" : "slide-out";

  const initialEventFilters = {
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
  };

  const initialSiteFilters = {
    pageSize: 1600,
    orderBy: "Popularity",
    sortOrder: "ASC",
    name: "",
    popularity: "",
    rating: null,
    searchKeyword: "",
    country: "",
    city: "",
  };

  const [eventFilters, setEventFilters] = useState(initialEventFilters);
  const [siteFilters, setSiteFilters] = useState(initialSiteFilters);
  const [tempName, setTempName] = useState(initialEventFilters.name);

  const handleFilterChange = (e) => {
    const { name, value, type, checked } = e.target;
    const newValue = type === "checkbox" ? checked : value;

    if (name === "popularity" && (newValue < 0 || newValue > 1)) {
      return;
    }

    if (filterOf === "events") {
      setEventFilters((prevFilters) => {
        const updatedFilters = { ...prevFilters, [name]: newValue };
        onFilterChange(updatedFilters);
        return updatedFilters;
      });
    } else if (filterOf === "sites") {
      setSiteFilters((prevFilters) => {
        const updatedFilters = { ...prevFilters, [name]: newValue };
        onFilterChange(updatedFilters);
        return updatedFilters;
      });
    }
  };

  const handleTempNameChange = (e) => {
    setTempName(e.target.value);
  };

  const handleSetName = () => {
    if (filterOf === "events") {
      setEventFilters((prevFilters) => {
        const updatedFilters = { ...prevFilters, name: tempName };
        onFilterChange(updatedFilters);
        return updatedFilters;
      });
    } else if (filterOf === "sites") {
      setSiteFilters((prevFilters) => {
        const updatedFilters = { ...prevFilters, name: tempName };
        onFilterChange(updatedFilters);
        return updatedFilters;
      });
    }
  };

  const handleReset = () => {
    if (filterOf === "events") {
      setEventFilters(initialEventFilters);
      onFilterChange(initialEventFilters);
    } else if (filterOf === "sites") {
      setSiteFilters(initialSiteFilters);
      onFilterChange(initialSiteFilters);
    }
    setTempName(""); // Reset tempName as well
  };

  const renderEventFilter = () => (
    <div className="events-filter">
      <label>
        Poredaj po:
        <select
          name="orderBy"
          value={eventFilters.orderBy}
          onChange={handleFilterChange}
        >
          <option value="StartDate">Datum Početka</option>
          <option value="Name">Naziv Događaja</option>
          <option value="Status">Status</option>
        </select>
      </label>
      <label>
        Sortiraj:
        <select
          name="sortOrder"
          value={eventFilters.sortOrder}
          onChange={handleFilterChange}
        >
          <option value="ASC">Uzlazno</option>
          <option value="DESC">Silazno</option>
        </select>
      </label>
      <label>
        Naziv Događaja:
        <input
          type="text"
          name="name"
          value={tempName}
          onChange={handleTempNameChange}
        />
        <button className="set-name-button" onClick={handleSetName}>
          Postavi
        </button>
      </label>
      <label>
        Datum Početka:
        <input
          type="date"
          name="startDate"
          value={eventFilters.startDate || ""}
          onChange={handleFilterChange}
        />
      </label>
      <label>
        Tip Događaja:
        <select
          name="type"
          value={eventFilters.type}
          onChange={handleFilterChange}
        >
          <option value="">Odaberi tip</option>
          <option value="Concert">Koncert</option>
          <option value="Festival">Festival</option>
        </select>
      </label>
      <label>
        Ulaz Besplatan:
        <input
          type="checkbox"
          name="isAccessibleForFree"
          checked={eventFilters.isAccessibleForFree}
          onChange={handleFilterChange}
        />
      </label>
      <label>
        Država:
        <input
          type="text"
          name="country"
          value={eventFilters.country}
          onChange={handleFilterChange}
        />
      </label>
      <label>
        Grad:
        <input
          type="text"
          name="city"
          value={eventFilters.city}
          onChange={handleFilterChange}
        />
      </label>
      <label>
        Pretraži:
        <input
          type="text"
          name="searchKeyword"
          value={eventFilters.searchKeyword}
          onChange={handleFilterChange}
        />
      </label>
    </div>
  );

  const renderTouristSitesFilter = () => (
    <div className="tourist-site-filter">
      <label>
        Poredaj po:
        <select
          name="orderBy"
          value={siteFilters.orderBy}
          onChange={handleFilterChange}
        >
          <option value="Popularity">Popularity</option>
          <option value="Name">Name</option>
        </select>
      </label>
      <label>
        Sortiraj:
        <select
          name="sortOrder"
          value={siteFilters.sortOrder}
          onChange={handleFilterChange}
        >
          <option value="ASC">Ascending</option>
          <option value="DESC">Descending</option>
        </select>
      </label>
      <label>
        Naziv Turističke Lokacije:
        <input
          type="text"
          name="name"
          value={tempName}
          onChange={handleTempNameChange}
        />
        <button onClick={handleSetName}>Set</button>
      </label>
      <label>
        Popularnost:
        <input
          type="number"
          name="popularity"
          step="0.001"
          min="0"
          max="1"
          value={siteFilters.popularity || ""}
          onChange={handleFilterChange}
        />
      </label>
      <label>
        Pretraži:
        <input
          type="text"
          name="searchKeyword"
          value={siteFilters.searchKeyword}
          onChange={handleFilterChange}
        />
      </label>
      <label>
        Država:
        <input
          type="text"
          name="country"
          value={siteFilters.country}
          onChange={handleFilterChange}
        />
      </label>
      <label>
        Grad:
        <input
          type="text"
          name="city"
          value={siteFilters.city}
          onChange={handleFilterChange}
        />
      </label>
    </div>
  );

  return (
    <div className={`filter-container ${filterClassName}`}>
      <button className="close-button" onClick={toggleFilter}>
        X
      </button>
      {filterOf === "events" ? renderEventFilter() : renderTouristSitesFilter()}
      <button className="reset-button" onClick={handleReset}>
        Resetiraj
      </button>
    </div>
  );
}

export default Filter;
