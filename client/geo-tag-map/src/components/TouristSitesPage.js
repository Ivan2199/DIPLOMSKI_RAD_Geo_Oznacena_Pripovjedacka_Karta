import React, { useEffect, useState, useMemo } from "react";
import TouristSiteService from "../services/TouristSiteService";
import ListContainer from "./ListContainer";
import LikedTouristSites from "./LikedTouristSites";
import LikeService from "../services/LikeService";
import "../styles/TouristSitesPage.css";
import { FaChevronLeft, FaChevronRight } from "react-icons/fa";

function TouristSitesPage({ setSelectedTouristSiteId }) {
  const [touristSites, setTouristSites] = useState([]);
  const [totalNumberOfTouristSites, setTotalNumberOfTouristSites] = useState(0);
  const [pageSize, setPageSize] = useState(12);
  const [currentPage, setCurrentPage] = useState(1);
  const [lastLikedTouristSites, setLastLikedTouristSites] = useState([]);
  const [mostLikedTouristSites, setMostLikedTouristSites] = useState([]);
  const [showFilter, setShowFilter] = useState(false);
  const [filters, setFilters] = useState({
    orderBy: "Popularity",
    sortOrder: "DESC",
    name: "",
    popularity: null,
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

  const touristSitesFilterForm = useMemo(
    () => ({ ...filters, pageSize, pageNumber: currentPage }),
    [filters, pageSize, currentPage]
  );

  useEffect(() => {
    const fetchTouristSites = async () => {
      try {
        const response = await TouristSiteService.getTouristSitesFiltered(
          touristSitesFilterForm
        );
        setTouristSites(response.List);
        setTotalNumberOfTouristSites(response.TotalSize);
        setPageSize(response.RRP);
      } catch (error) {
        console.error("Error occurred while fetching tourist sites", error);
      }
    };

    const handleScroll = () => {
      const navigation = document.querySelector(".tourist-sites-navigation ");
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

    fetchTouristSites();
    getLikedTouristSites();
    getMostLikedTouristSites();
    return () => {
      window.removeEventListener("scroll", handleScroll);
    };
  }, [touristSitesFilterForm]);

  const getMostLikedTouristSites = async () => {
    try {
      const response = await TouristSiteService.fetchMostLikedTouristSites();
      setMostLikedTouristSites(response.slice(0, 8));
    } catch (error) {
      console.log("Error occurred while fetching most liked events", error);
    }
  };

  const getLikedTouristSites = async () => {
    try {
      const response = await LikeService.getUserLikes("site");
      const likes = response;
      console.log(likes);

      const recentLikes = likes
        .sort((a, b) => new Date(b.DateCreated) - new Date(a.DateCreated))
        .slice(0, 8);

      const touristSiteIds = recentLikes.map((like) => like.TouristSiteId);

      const touristSites = [];
      for (const touristSiteId of touristSiteIds) {
        const touristSite = await TouristSiteService.getTouristSiteById(
          touristSiteId
        );
        touristSites.push(touristSite);
      }
      setLastLikedTouristSites(touristSites);
    } catch (error) {
      console.log("Error occurred while fetching tourist sites", error);
    }
  };

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage);
  };

  const totalPages = Math.ceil(totalNumberOfTouristSites / pageSize);

  return (
    <div className="tourist-sites-page-container">
      <div className="tourist-sites-navigation transparent">
        <h1>Tourist Sites</h1>
        <ul>
          <li>
            <a href="#tourist-sites">Tourist Sites</a>
          </li>
          <li>
            <a href="#liked-tourist-sites">Tourist Sites You Like</a>
          </li>
          <li>
            <a href="#most-liked-tourist-sites">Most Popular Tourist Sites</a>
          </li>
        </ul>
      </div>
      <div className="all-tourist-sites-likes">
        <div id="tourist-sites" className="tourist-sites-list-container">
          <ListContainer
            list={touristSites}
            listOf="sites"
            filters={filters}
            onFilterChange={handleFilterChange}
            showFilter={showFilter}
            toggleFilter={toggleFilter}
            setSelectedTouristSiteId={setSelectedTouristSiteId}
          />
          {touristSites && touristSites.length !== 0 && (
            <div className="tourist-sites-pagination-controls">
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
        <div id="liked-tourist-sites" className="liked-tourist-sites-list">
          <LikedTouristSites
            lastLikedTouristSites={lastLikedTouristSites}
            setSelectedTouristSiteId={setSelectedTouristSiteId}
          />
        </div>
        <div id="most-liked-tourist-sites" className="most-liked-tourist-sites">
          <ListContainer
            list={mostLikedTouristSites}
            listOf="likedsites"
            filters={filters}
            onFilterChange={handleFilterChange}
            showFilter={showFilter}
            toggleFilter={toggleFilter}
            setSelectedTouristSiteId={setSelectedTouristSiteId}
          />
        </div>
      </div>
    </div>
  );
}

export default TouristSitesPage;
