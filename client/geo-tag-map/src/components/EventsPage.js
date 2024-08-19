import React, { useEffect, useState, useMemo } from "react";
import EventService from "../services/EventService";
import LikeService from "../services/LikeService";
import ListContainer from "./ListContainer";
import "../styles/EventsPage.css";
import LikedEvents from "./LikedEvents";
import { FaChevronLeft, FaChevronRight } from "react-icons/fa";

function EventsPage({ setSelectedEventId }) {
  const [events, setEvents] = useState([]);
  const [totalNumberOfEvents, setTotalNumberOfEvents] = useState(0);
  const [pageSize, setPageSize] = useState(12);
  const [currentPage, setCurrentPage] = useState(1);
  const [lastLikedEvents, setLastLikedEvents] = useState([]);
  const [mostLikedEvents, setMostLikedEvents] = useState([]);
  const [showFilter, setShowFilter] = useState(false);
  const [filters, setFilters] = useState({
    orderBy: "StartDate",
    sortOrder: "ASC",
    name: "",
    startDate: null,
    type: "",
    isAccessibleForFree: false,
    searchKeyword: "",
    country: "",
    city: "",
  });

  const toggleFilter = () => {
    setShowFilter(!showFilter);
  };

  const handleFilterChange = (updatedFilters) => {
    setFilters(updatedFilters);
  };

  const eventFilterForm = useMemo(
    () => ({ ...filters, pageSize, pageNumber: currentPage }),
    [filters, pageSize, currentPage]
  );

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        const response = await EventService.getEventsFiltered(eventFilterForm);
        setEvents(response.List);
        setTotalNumberOfEvents(response.TotalSize);
        setPageSize(response.RRP);
      } catch (error) {
        console.error("Error occurred while fetching events", error);
      }
    };

    const handleScroll = () => {
      const navigation = document.querySelector(".events-page-navigation");
      if (navigation) {
        if (window.scrollY > 0) {
          navigation.classList.add("solid");
          navigation.classList.remove("transparent");
        } else {
          navigation.classList.add("transparent");
          navigation.classList.remove("solid");
        }
      }
    };
    window.addEventListener("scroll", handleScroll);

    fetchEvents();
    getLikedEvents();
    getMostLikedEvents();
    return () => {
      window.removeEventListener("scroll", handleScroll);
    };
  }, [eventFilterForm]);

  const getMostLikedEvents = async () => {
    try {
      const response = await EventService.fetchMostLikedEvents();
      setMostLikedEvents(response.slice(0, 8));
    } catch (error) {
      console.log("Error occurred while fetching most liked events", error);
    }
  };

  const getLikedEvents = async () => {
    try {
      const response = await LikeService.getUserLikes("event");
      const likes = response;

      const recentLikes = likes
        .sort((a, b) => new Date(b.DateCreated) - new Date(a.DateCreated))
        .slice(0, 8);

      const eventIds = recentLikes.map((like) => like.EventId);

      const events = [];
      for (const eventId of eventIds) {
        const event = await EventService.getEventById(eventId);
        events.push(event);
      }
      setLastLikedEvents(events);
    } catch (error) {
      console.log("Error occurred while fetching events", error);
    }
  };

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage);
  };

  const totalPages = Math.ceil(totalNumberOfEvents / pageSize);

  return (
    <div className="events-page-container">
      <div className="events-page-navigation transparent">
        <h1>Music Events</h1>
        <ul>
          <li>
            <a href="#events">Events</a>
          </li>
          <li>
            <a href="#liked-events">Events You Like</a>
          </li>
          <li>
            <a href="#most-liked-events">Most Popular Events</a>
          </li>
        </ul>
      </div>
      <div className="all-events-likes">
        <div id="events" className="events-list-container">
          <ListContainer
            list={events}
            listOf="events"
            filters={filters}
            onFilterChange={handleFilterChange}
            showFilter={showFilter}
            toggleFilter={toggleFilter}
            setSelectedEventId={setSelectedEventId}
          />
          {events && events.length !== 0 && (
            <div className="event-pagination-controls">
              <button
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                aria-label="Previous Page"
              >
                <FaChevronLeft className="arrow-left" />
              </button>
              <span>{`Page ${currentPage} of ${totalPages}`}</span>
              <button
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                aria-label="Next Page"
              >
                <FaChevronRight className="arrow-right" />
              </button>
            </div>
          )}
        </div>
        <div id="liked-events" className="liked-events-list">
          <LikedEvents
            lastLikedEvents={lastLikedEvents}
            setSelectedEventId={setSelectedEventId}
          />
        </div>
        <div id="most-liked-events" className="most-liked-events">
          <ListContainer
            list={mostLikedEvents}
            listOf="likedevents"
            filters={filters}
            onFilterChange={handleFilterChange}
            showFilter={showFilter}
            toggleFilter={toggleFilter}
            setSelectedEventId={setSelectedEventId}
          />
        </div>
      </div>
    </div>
  );
}

export default EventsPage;
