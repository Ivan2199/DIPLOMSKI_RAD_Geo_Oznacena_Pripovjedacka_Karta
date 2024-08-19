import "./App.css";
import React, { useState } from "react";
import {
  BrowserRouter as Router,
  Route,
  Routes,
  useLocation,
} from "react-router-dom";
import ToggleMaps from "./components/ToggleMaps";
import LoginForm from "./components/LoginForm";
import RegistrationForm from "./components/RegistrationForm";
import LandingPage from "./components/LandingPage";
import Gallery from "./components/Gallery";
import Background3DMap from "./components/Background3DMap";
import Menu from "./components/Menu";
import UserProfile from "./components/UserProfile";
import EventsPage from "./components/EventsPage";
import TouristSitesPage from "./components/TouristSitesPage";
import AdminPage from "./components/AdminPage";
import { v4 as uuidv4 } from "uuid";

function App() {
  const [user, setUser] = useState({
    username: "",
    firstName: "",
    lastName: "",
    email: "",
    password: "",
  });

  const [isEvent, setIsEvent] = useState(true);
  const [isTouristSite, setIsTouristSite] = useState(false);
  const [isStory, setIsStory] = useState(false);
  const [selectedEventId, setSelectedEventId] = useState(uuidv4());
  const [selectedTouristSiteId, setSelectedTouristSiteId] = useState(uuidv4());

  return (
    <div className="App">
      <header className="App-header">
        <Router>
          <AppContent
            user={user}
            setUser={setUser}
            isEvent={isEvent}
            setIsEvent={setIsEvent}
            isTouristSite={isTouristSite}
            setIsTouristSite={setIsTouristSite}
            isStory={isStory}
            setIsStory={setIsStory}
            setSelectedEventId={setSelectedEventId}
            selectedEventId={selectedEventId}
            setSelectedTouristSiteId={setSelectedTouristSiteId}
            selectedTouristSiteId={selectedTouristSiteId}
          />
        </Router>
      </header>
    </div>
  );
}

function AppContent({
  user,
  setUser,
  isEvent,
  setIsEvent,
  isTouristSite,
  setIsTouristSite,
  isStory,
  setIsStory,
  setSelectedEventId,
  selectedEventId,
  setSelectedTouristSiteId,
  selectedTouristSiteId,
}) {
  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const mapType = queryParams.get("mapType") || "";

  return (
    <>
      {location.pathname !== "/" && <Menu />}
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route
          path="/registration"
          element={<RegistrationForm user={user} setUser={setUser} />}
        />
        <Route path="/login" element={<LoginForm />} />
        <Route
          path="/home"
          element={
            <Background3DMap
              setIsEvent={setIsEvent}
              isEvent={isEvent}
              setIsTouristSite={setIsTouristSite}
              isTouristSite={isTouristSite}
              setIsStory={setIsStory}
              isStory={isStory}
              selectedEventId={selectedEventId}
              setSelectedEventId={setSelectedEventId}
              setSelectedTouristSiteId={setSelectedTouristSiteId}
              selectedTouristSiteId={selectedTouristSiteId}
              mapType={mapType}
            />
          }
        />
        <Route path="/gallery/:id" element={<Gallery />} />
        <Route
          path="/user-profile"
          element={
            <UserProfile
              setSelectedEventId={setSelectedEventId}
              setSelectedTouristSiteId={setSelectedTouristSiteId}
            />
          }
        />
        <Route
          path="/music-events"
          element={<EventsPage setSelectedEventId={setSelectedEventId} />}
        />
        <Route
          path="/tourist-sites"
          element={
            <TouristSitesPage
              setSelectedTouristSiteId={setSelectedTouristSiteId}
            />
          }
        />
        <Route path="/admin" element={<AdminPage />} />
      </Routes>
    </>
  );
}

export default App;
