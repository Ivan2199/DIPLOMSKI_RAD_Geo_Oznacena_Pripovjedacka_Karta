import React, { useEffect, useState } from "react";
import ImageSlider from "./ImageSlider";
import UserService from "../services/UserService";
import LikeService from "../services/LikeService";
import StoryService from "../services/StoryService";
import CommentService from "../services/CommentService";
import EventService from "../services/EventService";
import TouristLocationTile from "./TouristLocationTile";
import EventTile from "./EventTile";
import "../styles/UserProfile.css";
import TouristSiteService from "../services/TouristSiteService";
import LikedEvents from "./LikedEvents";
import LikedTouristSites from "./LikedTouristSites";

const UserProfile = ({ setSelectedEventId, setSelectedTouristSiteId }) => {
  const [loggedUser, setLoggedUser] = useState({});
  const [lastStories, setLastStories] = useState([]);
  const [storyCount, setStoryCount] = useState(0);
  const [likeCount, setLikeCount] = useState(0);
  const [commentCount, setCommentCount] = useState(0);
  const [userLikeCount, setUserLikeCount] = useState(0);
  const [userCommentCount, setUserCommentCount] = useState(0);
  const [lastLikedEvents, setLastLikedEvents] = useState([]);
  const [lastLikedTouristSites, setLastLikedTouristSites] = useState([]);

  const getUser = async () => {
    try {
      const response = await UserService.getLoggedUserAsync();
      setLoggedUser(response);
    } catch (error) {
      console.log("Error while getting logged user", error);
    }
  };

  const getStoryComments = async (storyId) => {
    try {
      const response = await CommentService.getCommentsByStoryId(storyId);
      return response.length;
    } catch (error) {
      console.log("Error occurred while fetching story comments", error);
      return;
    }
  };
  const getAllUserStoryComments = async () => {
    try {
      const response = await CommentService.getCommentsByUserId();
      setUserCommentCount(response.length);
      return response.length;
    } catch (error) {
      console.log("Error occurred while fetching story comments", error);
      return;
    }
  };
  const getAllUserStoryLikes = async () => {
    try {
      const response = await LikeService.getUserLikes("story");
      setUserLikeCount(response.length);
    } catch (error) {
      console.log("Error occurred while fetching user likes", error);
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

  const getStories = async () => {
    try {
      const response = await StoryService.fetchStoriesByUserId();
      setStoryCount(response.length);

      const totalLikes = response.reduce((total, story) => {
        if (
          typeof story.NumberOfLikes === "number" &&
          !isNaN(story.NumberOfLikes)
        ) {
          return total + story.NumberOfLikes;
        }
        return total;
      }, 0);
      setLikeCount(totalLikes || 0);

      const commentCounts = await Promise.all(
        response.map((story) => getStoryComments(story.Id))
      );

      const totalComments = commentCounts.reduce(
        (total, count) => total + count,
        0
      );
      setCommentCount(totalComments);

      const sortedStories = response.sort(
        (a, b) => new Date(b.DateTime) - new Date(a.DateTime)
      );
      const lastThreeStories = sortedStories.slice(0, 2);

      const storiesWithComments = await Promise.all(
        lastThreeStories.map(async (story) => {
          const comments = await getStoryComments(story.Id);
          return { ...story, NumberOfComments: comments };
        })
      );
      setLastStories(storiesWithComments);
    } catch (error) {
      console.log("Error occurred while fetching stories", error);
    }
  };

  useEffect(() => {
    getUser();
    getStories();
    getAllUserStoryLikes();
    getAllUserStoryComments();
    getLikedEvents();
    getLikedTouristSites();
  }, []);

  return (
    <div className="container">
      <div className="profile-container">
        <div className="profile-card">
          <div className="profile-header">
            <div className="main-profile">
              <img
                src={loggedUser.Image}
                alt="Profile Image"
                className="profile-image"
              />
              <div className="profile-names">
                <h1 className="username">{loggedUser.Username}</h1>
                <small className="page-title">
                  {loggedUser.FirstName} {loggedUser.LastName}
                </small>
              </div>
            </div>
          </div>

          <div className="profile-body">
            <div className="profile-actions">
              <button className="follow">Edit Profile</button>
              <section className="bio">
                <div className="bio-header">
                  <i className="fa fa-info-circle"></i>
                  {loggedUser.Username} email
                </div>
                <p className="bio-text">{loggedUser.Email}</p>
              </section>
            </div>

            <div className="account-info">
              <div className="data">
                <div className="important-data">
                  <section className="data-item">
                    <h3 className="value">{storyCount}</h3>
                    <small className="title">Stories</small>
                  </section>
                  <section className="data-item">
                    <h3 className="value">{String(likeCount)}</h3>
                    <small className="title">Story Likes</small>
                  </section>
                  <section className="data-item">
                    <h3 className="value">{commentCount}</h3>
                    <small className="title">Story Comments</small>
                  </section>
                </div>
                <div className="other-data">
                  <section className="data-item">
                    <h3 className="value">{userLikeCount}</h3>
                    <small className="title">Liked Stories</small>
                  </section>
                  <section className="data-item">
                    <h3 className="value">{userCommentCount}</h3>
                    <small className="title">Comments On Stories</small>
                  </section>
                </div>
              </div>

              <div className="social-media">
                <p className="title-last-stories">Last Stories</p>
                <div className="last-published-stories">
                  {lastStories.length > 0 ? (
                    lastStories.map((story) => (
                      <div key={story.Id} className="story-user-profile-card">
                        <div className="story-card-information">
                          <p className="story-card-text">{story.Text}</p>
                          <p className="story-card-likes">
                            <span>Likes</span> {story.NumberOfLikes || 0}
                            {"  "}
                            <span>Comments</span> {story.NumberOfComments}
                          </p>
                        </div>
                        <div className="story-card-images">
                          {story.Photos && story.Photos.length > 0 && (
                            <ImageSlider images={story.Photos} />
                          )}
                        </div>
                      </div>
                    ))
                  ) : (
                    <div>"You haven't published any story"</div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="tourist-event-cards">
          <LikedEvents
            lastLikedEvents={lastLikedEvents}
            setSelectedEventId={setSelectedEventId}
          />
          <LikedTouristSites
            lastLikedTouristSites={lastLikedTouristSites}
            setSelectedTouristSiteId={setSelectedTouristSiteId}
          />
        </div>
      </div>
    </div>
  );
};

export default UserProfile;
