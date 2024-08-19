import React, { useEffect, useState } from "react";
import "../styles/LandingPage.css";
import EventService from "../services/EventService";
import EventTile from "./EventTile";
import TouristSiteService from "../services/TouristSiteService";
import TouristLocationTile from "./TouristLocationTile";
import LoginForm from "./LoginForm";

function LandingPage() {
  const [loginSelected, setLoginSelected] = useState(false);
  const [events, setEvents] = useState([]);
  const [touristSites, setTouristSites] = useState([]);
  const [eventFilterForm, setEventFilterForm] = useState({
    pageSize: 3,
    pageNumber: 1,
    orderBy: "Name",
    sortOrder: "ASC",
  });
  const [touristSiteFilterForm, setTouristSiteFilterForm] = useState({
    pageSize: 4,
    pageNumber: 4,
    sortOrder: "DESC",
  });

  useEffect(() => {
    fetchAndFilterEvents();
    fetchAndFilterTouristSites();

    const handleScroll = () => {
      const navigation = document.querySelector(".landing-page-navigation");
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

    const handleLoginForm = () => {
      const loginForm = document.querySelector(".login-overlay");
      if (loginForm) {
        if (loginSelected) {
          loginForm.style.position = "fixed";
          loginForm.style.top = "50%";
          loginForm.style.left = "50%";
          loginForm.style.transform = "translate(-50%, -50%)";
          document.body.style.overflow = "hidden";
        } else {
          loginForm.style.position = "static";
          document.body.style.overflow = "visible";
        }
      }
    };

    window.addEventListener("scroll", handleScroll);

    handleLoginForm();
    return () => {
      window.removeEventListener("scroll", handleScroll);
    };
  }, [loginSelected]);

  const fetchAndFilterEvents = async () => {
    try {
      const response = await EventService.getEventsFiltered(eventFilterForm);
      console.log("API response:", response);
      if (Array.isArray(response.List)) {
        console.log("List of elements:", response.List);
        setEvents(response.List);
      } else {
        setEvents([]);
      }
    } catch (error) {
      console.error("Error fetching events: ", error);
      setEvents([]);
    }
  };

  const fetchAndFilterTouristSites = async () => {
    try {
      const response = await TouristSiteService.getTouristSitesFiltered(
        touristSiteFilterForm
      );
      console.log("API response:", response);
      if (Array.isArray(response.List)) {
        console.log("List of elements:", response.List);
        setTouristSites(response.List);
      } else {
        setTouristSites([]);
      }
    } catch (error) {
      console.error("Error fetching events: ", error);
      setTouristSites([]);
    }
  };

  return (
    <div className="landing-page">
      <div
        className="login-overlay"
        style={{ display: loginSelected ? "flex" : "none" }}
      >
        <LoginForm />
      </div>
      <div className={`landing-page-content ${loginSelected ? "blurred" : ""}`}>
        <div className="landing-page-navigation transparent">
          <h1>GeoTagMap</h1>
          <ul>
            <li>
              <a href="#about">About Us</a>
            </li>
            <li>
              <a href="#events">Events</a>
            </li>
            <li>
              <a href="#locations">Tourist Locations</a>
            </li>
          </ul>
          <div className="login-registration-button">
            <div
              className="login-button"
              onClick={() => setLoginSelected(true)}
            >
              Login
            </div>
            <div className="registration-button">Registration</div>
          </div>
        </div>

        <div className="landing-page-header-image">
          <div className="landing-page-header-image-overlay">
            <div id="hero">
              <div className="hero-content">
                <h2>Explore the World</h2>
                <p>
                  Explore events, tourist attractions, and user experiences from
                  around the world.
                </p>
                <a href="#about" className="btn-primary">
                  Learn More
                </a>
              </div>
            </div>
          </div>
          <img src="/assets/earthGlobe.jpg" alt="Sphere Earth Map" />
        </div>

        <div id="about" className="content-section">
          <div className="about-section">
            <div className="about-section-information">
              <h2>About Us</h2>
              <p>
                Naša platforma omogućava vam da istražujete događaje i
                turističke lokacije širom svijeta, te dijelite vlastite priče i
                slike.
              </p>
            </div>
            <img src="/assets/information-icon.jpg" />
          </div>
        </div>

        <div id="events" className="content-section">
          <div className="landing-page-events">
            <h2>Events</h2>
            <div id="globe-container">
              {events.map((event) => (
                <EventTile
                  key={event.Id}
                  name={event.Name}
                  url={event.Url}
                  image={event.Image}
                  startDate={event.StartDate}
                  endDate={event.EndDate}
                  type={event.Type}
                  country={event.Location?.Country}
                  city={event.Location?.City}
                  address={event.Location?.Address}
                />
              ))}
            </div>
          </div>
        </div>

        <div id="locations" className="content-section">
          <div className="landing-page-tourist-sites">
            <h2>Tourist Locations</h2>
            <div id="map-container">
              {touristSites.map((touristSite) => (
                <TouristLocationTile
                  key={touristSite.Id}
                  name={touristSite.Name}
                  images={touristSite.Photos}
                  popularity={touristSite.Popularity}
                  description={touristSite.Description}
                  country={touristSite.Location?.Country}
                  city={touristSite.Location?.City}
                  address={touristSite.Location?.Address}
                  hours={touristSite.HoursOpen}
                />
              ))}
            </div>
          </div>
        </div>

        <div className="landing-page-footer">
          <p>© 2024 GeoTagMap. All rights reserved.</p>
        </div>
      </div>
    </div>
  );
}

export default LandingPage;
