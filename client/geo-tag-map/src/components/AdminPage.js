import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getUserRole } from "../utils/authUtils";
import UserService from "../services/UserService";
import UserManagement from "./UserManagement";
import StoryManagement from "./StoryManagement";
import CommentManagement from "./CommentManagement";
import "../styles/AdminPage.css";

function AdminPage() {
  const navigate = useNavigate();
  const [loggedAdminUser, setLoggedAdminUser] = useState({});
  const [activePage, setActivePage] = useState("");

  useEffect(() => {
    const userRole = getUserRole();
    if (userRole !== "Admin") {
      navigate("/home");
    } else {
      getLoggedAdminUser();
    }
  }, [navigate]);

  const getLoggedAdminUser = async () => {
    try {
      const response = await UserService.getLoggedUserAsync();
      setLoggedAdminUser(response);
    } catch (error) {
      console.log("Error occurred while fetching logged user", error);
    }
  };

  const handlePageChange = (page) => {
    setActivePage(page);
  };

  const { FirstName, LastName } = loggedAdminUser;

  return (
    <div className="admin-page-container">
      <h1 className="admin-page-title">
        Welcome Back {FirstName} {LastName}
      </h1>
      <div className="admin-user-actions">
        <div
          className={`action-button ${
            activePage === "commentM" ? "active" : ""
          }`}
          onClick={() => handlePageChange("commentM")}
        >
          Kontrola Komentara
        </div>
        <div
          className={`action-button ${activePage === "storyM" ? "active" : ""}`}
          onClick={() => handlePageChange("storyM")}
        >
          Kontrola Priča
        </div>
        <div
          className={`action-button ${activePage === "userM" ? "active" : ""}`}
          onClick={() => handlePageChange("userM")}
        >
          Kontrola Korisnika
        </div>
      </div>
      <div className="admin-content">
        {activePage === "commentM" && <CommentManagement />}
        {activePage === "storyM" && <StoryManagement />}
        {activePage === "userM" && <UserManagement />}
      </div>
    </div>
  );
}

const AddTouristSite = () => <div>Add Tourist Site Content</div>;
const AddEvent = () => <div>Add Event Content</div>;

export default AdminPage;
