import React, { useEffect, useState } from "react";
import "../styles/Menu.css";
import { IconContext } from "react-icons";
import { useNavigate, useLocation } from "react-router-dom";
import {
  FaBars,
  FaGlobe,
  FaUser,
  FaPlane,
  FaMusic,
  FaSignOutAlt,
  FaUserShield,
} from "react-icons/fa";
import UserService from "../services/UserService";
import { getUserRole } from "../utils/authUtils";

const Menu = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [loggedUser, setLoggedUser] = useState({});
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    getUser();
  }, []);

  const getUser = async () => {
    try {
      const response = await UserService.getLoggedUserAsync();
      setLoggedUser(response);
    } catch (error) {
      console.log("Error while getting logged user", error);
    }
  };

  const logout = () => {
    try {
      UserService.logoutUser();
      navigate(`/`);
    } catch (error) {
      console.log("Error while logging out", error);
    }
  };

  const toggleSidebar = () => {
    setIsOpen(!isOpen);
  };

  const handleItemClick = (path) => {
    navigate(path);
  };

  const getActiveClass = (path) => {
    return location.pathname === path ? "active" : "";
  };

  return (
    <IconContext.Provider value={{ color: "#fff" }}>
      <div className={`sidebar ${isOpen ? "open" : ""}`}>
        <div className="logo-details">
          <div className="logo_name">GeoTagMap</div>
          <div id="btn" onClick={toggleSidebar}>
            <FaBars />
          </div>
        </div>
        <ul className="nav-list">
          <li
            className={`nav-item ${getActiveClass("/user-profile")}`}
            title="User Profile"
            onClick={() => handleItemClick("/user-profile")}
          >
            <a className="nav-link">
              <div className="icon">
                <FaUser />
              </div>
              <span className="links_name">User Profile</span>
            </a>
          </li>
          <li
            className={`nav-item ${getActiveClass("/home")}`}
            title="World Map"
            onClick={() => handleItemClick("/home")}
          >
            <a className="nav-link">
              <div className="icon">
                <FaGlobe />
              </div>
              <span className="links_name">World Map</span>
            </a>
          </li>
          <li
            className={`nav-item ${getActiveClass("/tourist-sites")}`}
            title="Tourist Sites"
            onClick={() => handleItemClick("/tourist-sites")}
          >
            <a className="nav-link">
              <div className="icon">
                <FaPlane />
              </div>
              <span className="links_name">Tourist Sites</span>
            </a>
          </li>
          <li
            className={`nav-item ${getActiveClass("/music-events")}`}
            title="Music Events"
            onClick={() => handleItemClick("/music-events")}
          >
            <a className="nav-link">
              <div className="icon">
                <FaMusic />
              </div>
              <span className="links_name">Music Events</span>
            </a>
          </li>
          {getUserRole() == "Admin" && (
            <li
              className={`nav-item ${getActiveClass("/admin")}`}
              title="Admin Page"
              onClick={() => handleItemClick("/admin")}
            >
              <a className="nav-link">
                <div className="icon">
                  <FaUserShield />
                </div>
                <span className="links_name">Admin Page</span>
              </a>
            </li>
          )}
          <li className="profile">
            <div className="profile-details">
              <img src={loggedUser.Image} alt="profileImg" />
              <div className="name_job">
                <div className="name">{loggedUser.Username}</div>
              </div>
            </div>
            <FaSignOutAlt id="log_out" title="Log Out" onClick={logout} />
          </li>
        </ul>
      </div>
    </IconContext.Provider>
  );
};

export default Menu;
