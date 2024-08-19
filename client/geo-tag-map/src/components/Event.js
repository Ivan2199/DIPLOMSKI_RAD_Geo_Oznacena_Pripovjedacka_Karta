import React, { useState, useEffect } from "react";
import { BrowserRouter, Link } from "react-router-dom";
import "../styles/Event.css";
import PerformerService from "../services/PerformerService";
import LikeService from "../services/LikeService";
import CommentService from "../services/CommentService";
import EventService from "../services/EventService";
import Comment from "./Comment";
import CommentForm from "./CommentForm";

function Event(props) {
  const [isInformation, setIsInformation] = useState(true);
  const [isComment, setIsComment] = useState(false);
  const [isAddComment, setIsAddComment] = useState(false);
  const [isVisible, setIsVisible] = useState(true);
  const [currentEventIndex, setCurrentEventIndex] = useState(0);
  const [performers, setPerformers] = useState([]);
  const [currentPerformerIndex, setCurrentPerformerIndex] = useState(0);
  const [comments, setComments] = useState([]);
  const [likes, setLikes] = useState({});
  const [numberOfLikes, setNumberOfLikes] = useState(0);
  const [isLiking, setIsLiking] = useState(false);

  const events = Array.isArray(props.slides) ? props.slides : [props.slides];
  const currentEvent =
    events[currentEventIndex]?.props?.event || events[currentEventIndex];

  useEffect(() => {
    if (!isVisible) {
      const timeout = setTimeout(() => {
        props.onClose();
      }, 500);
      return () => clearTimeout(timeout);
    }
  }, [isVisible, props.onClose, currentEvent]);

  useEffect(() => {
    if (currentEvent) {
      getEventPerformers(currentEvent);
      getInitialLikes();
      getNumberOfLikes(currentEvent.Id);
    } else {
      setPerformers([]);
    }
  }, [currentEvent]);

  useEffect(() => {
    if (currentEvent) {
      getEventComments(currentEvent.Id);
    }
  }, [currentEvent, comments]);

  useEffect(() => {
    if (performers.length > 0) {
      setCurrentPerformerIndex(0);
    }
  }, [performers]);

  const getNumberOfLikes = async (eventId) => {
    try {
      const response = await EventService.getEventById(eventId);
      setNumberOfLikes(response.NumberOfLikes || 0);
    } catch (error) {
      console.log(error);
    }
  };

  const getEventComments = async (eventId) => {
    try {
      const response = await CommentService.getCommentsByEventId(eventId);
      setComments(response || []);
    } catch (error) {
      console.error("Error fetching comments:", error);
      setComments([]);
    }
  };

  function formatDate(date) {
    const dateObject = new Date(date);

    const year = dateObject.getFullYear();
    const month = String(dateObject.getMonth() + 1).padStart(2, "0");
    const day = String(dateObject.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
  }

  const handleClose = () => {
    setIsVisible(false);
  };

  const handleToggle = (tab) => {
    if (tab === "information") {
      setIsInformation(true);
      setIsComment(false);
      setIsAddComment(false);
    } else if (tab === "comments") {
      setIsInformation(false);
      setIsComment(true);
      setIsAddComment(false);
    } else if (tab === "add-comment") {
      setIsInformation(false);
      setIsComment(false);
      setIsAddComment(true);
    }
  };

  const getEventPerformers = async (event) => {
    if (event && event.EventPerformers && event.EventPerformers.length > 0) {
      const performerIds = event.EventPerformers.map(
        (eventPerformer) => eventPerformer.PerformerId
      );

      const performerPromises = performerIds.map((performerId) =>
        PerformerService.getPerformerById(performerId)
      );
      try {
        const responses = await Promise.all(performerPromises);
        const allPerformers = responses.flat();
        setPerformers(allPerformers);
      } catch (error) {
        console.error("Error fetching performer data:", error);
      }
    } else {
      setPerformers([]);
    }
  };

  const handleNextPerformer = () => {
    setCurrentPerformerIndex(
      (prevIndex) => (prevIndex + 1) % performers.length
    );
  };

  const handlePreviousPerformer = () => {
    setCurrentPerformerIndex((prevIndex) =>
      prevIndex === 0 ? performers.length - 1 : prevIndex - 1
    );
  };

  const handleAddComment = async (newComment) => {
    try {
      const response = await CommentService.createComment(newComment);
      if (response.success) {
        setComments([...comments, response.data]);
      } else {
        console.error("Error adding comment:", response.error);
      }
    } catch (error) {
      console.error("Error adding comment:", error);
    }
  };

  const getInitialLikes = async () => {
    try {
      const likeExists = await existingCurrentUserLike(currentEvent.Id);
      if (likeExists) {
        setLikes((prevLikes) => ({
          ...prevLikes,
          [currentEvent.Id]: true,
        }));
      }
    } catch (error) {
      console.error("Error fetching likes:", error);
    }
  };

  const existingCurrentUserLike = async (eventId) => {
    try {
      const response = await LikeService.getEventLike(eventId);
      return response;
    } catch (error) {
      console.error("Error fetching like:", error);
      return false;
    }
  };
  const handleAddLike = async (eventId) => {
    if (isLiking) return;
    setIsLiking(true);

    try {
      const likeExists = await existingCurrentUserLike(eventId);
      const updatedNumberOfLikes = likeExists
        ? numberOfLikes - 1
        : numberOfLikes + 1;

      setLikes((prevLikes) => ({
        ...prevLikes,
        [eventId]: !likeExists,
      }));

      const updatedEvent = {
        NumberOfLikes: Math.max(updatedNumberOfLikes, 0),
      };
      await EventService.updateEventAsync(updatedEvent, eventId);

      if (likeExists) {
        const likeId = await deleteLike(likeExists.Id, eventId);
        console.log("Event disliked");
      } else {
        await addLikeToBase({ eventId });
      }

      setNumberOfLikes(updatedEvent.NumberOfLikes);
    } catch (error) {
      console.error("Error updating event:", error);
    } finally {
      setIsLiking(false);
    }
  };

  const addLikeToBase = async (like) => {
    try {
      like.isLike = true;
      await LikeService.addLike(like);
    } catch (error) {
      console.error("Error adding like:", error);
    }
  };

  const deleteLike = async (likeId, eventId) => {
    try {
      await LikeService.deleteLikeAsync(likeId);
    } catch (error) {
      console.error("Error deleting like:", error);
    }
  };

  const renderUserActions = () => {
    return (
      <>
        <div className="icon-elements">
          {likes[currentEvent.Id] ? (
            <img
              src="/assets/liked.png"
              alt="Liked"
              className="liked"
              title="Liked"
              onClick={() => handleAddLike(currentEvent.Id)}
            />
          ) : (
            <img
              src="/assets/like.svg"
              alt="Like"
              title="Like"
              onClick={() => handleAddLike(currentEvent.Id)}
            />
          )}
          <p className="story-number-of-likes">{numberOfLikes}</p>
        </div>
      </>
    );
  };

  return (
    <div
      className={`event-page ${
        isVisible ? "show-page-animation" : "hide-page-animation"
      } ${events.length > 1 ? "show-page-background" : "hide-page-background"}`}
    >
      <div
        className={`event-container ${
          isVisible ? "show-animation" : "hide-animation"
        }`}
        id="eventContainer"
      >
        <button className="close-button" id="closeButton" onClick={handleClose}>
          X
        </button>
        <div className="event-information">
          <div className="event-image">
            {currentEvent?.Image ? (
              <img src={currentEvent.Image} alt={currentEvent.Name} />
            ) : (
              <p>Image not available</p>
            )}
          </div>
          <div className="event-name-like-button">
            {currentEvent?.Name} {renderUserActions()}
          </div>

          <div className="event-navigation-bar">
            <ul>
              <li
                onClick={() => handleToggle("information")}
                className={isInformation ? "visible" : ""}
              >
                Informacije
              </li>
              <li
                onClick={() => handleToggle("comments")}
                className={isComment ? "visible" : ""}
              >
                Komentari
              </li>
              <li
                onClick={() => handleToggle("add-comment")}
                className={isAddComment ? "visible" : ""}
              >
                Dodaj Komentar
              </li>
            </ul>
          </div>
          <div
            className={
              isInformation ? "visible-information-card" : "hidden-card"
            }
          >
            {isInformation && currentEvent && (
              <>
                <div className="event-section">
                  <p>
                    Tip Događaja:&nbsp;&nbsp;
                    {currentEvent.Type === "Concert" ? "Koncert" : "Festival"}
                  </p>
                  <p>Status Događaja:&nbsp;&nbsp;{currentEvent.EventStatus}</p>
                  <div className="event-start-end-date">
                    <p>
                      Datum Početka:&nbsp;&nbsp;
                      {new Date(currentEvent.StartDate).toLocaleDateString(
                        "hr-HR"
                      )}
                    </p>
                    <p>
                      Datum Kraja:&nbsp;&nbsp;
                      {new Date(currentEvent.EndDate).toLocaleDateString(
                        "hr-HR"
                      )}
                    </p>
                  </div>
                  <div className="event-price">
                    {currentEvent.TicketInformation?.Price && (
                      <div>
                        <p>
                          Cijena:&nbsp;&nbsp;
                          {currentEvent.TicketInformation.Price}
                          &nbsp;&nbsp;
                          {currentEvent.TicketInformation.PriceCurrency}
                        </p>
                        <p>
                          Prodavač:&nbsp;&nbsp;
                          {currentEvent.TicketInformation.Seller}
                        </p>
                      </div>
                    )}
                  </div>
                </div>
                <div className="location-section">
                  {currentEvent.Location?.Country && (
                    <p>Zemlja:&nbsp;&nbsp;{currentEvent.Location.Country}</p>
                  )}
                  {currentEvent.Location?.City && (
                    <p>Grad:&nbsp;&nbsp;{currentEvent.Location.City}</p>
                  )}
                  {currentEvent.Location?.Address && (
                    <p>Adresa:&nbsp;&nbsp;{currentEvent.Location.Address}</p>
                  )}
                  {currentEvent.Location?.NameOfPlace && (
                    <p>
                      Ime Mjesta:&nbsp;&nbsp;
                      {currentEvent.Location.NameOfPlace}
                    </p>
                  )}
                </div>
                <div className="performer-section">
                  <div className="event-performer-information">
                    {performers.length > 0 && (
                      <div
                        className={`performer-slider ${
                          performers.length === 1 ? "single-performer" : ""
                        }`}
                      >
                        {performers.length > 1 && (
                          <button onClick={handlePreviousPerformer}>
                            &#8249;
                          </button>
                        )}
                        <div className="performer-slide">
                          {performers[currentPerformerIndex]?.Image ? (
                            <div className="performer-image">
                              <img
                                src={performers[currentPerformerIndex].Image}
                                alt={performers[currentPerformerIndex].Name}
                              />
                            </div>
                          ) : (
                            <p>Image not available</p>
                          )}
                          <div className="performer-information">
                            <p className="performer-name">
                              {performers[currentPerformerIndex].Name}
                            </p>
                            <p>
                              {
                                performers[currentPerformerIndex]
                                  .NumOfUpcomingEvents
                              }
                              &nbsp;nadolazećih događaja
                            </p>
                          </div>
                        </div>
                        {performers.length > 1 && (
                          <button onClick={handleNextPerformer}>&#8250;</button>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              </>
            )}
          </div>
          <div
            className={
              isComment ? "visible-comment-card" : "hidden-comment-card"
            }
          >
            {isComment && currentEvent && (
              <Comment
                commentOf="eventComment"
                id={currentEvent.Id}
                comments={comments}
                setComments={setComments}
                handleAddComment={handleAddComment}
              />
            )}
          </div>
          <div
            className={
              isAddComment ? "visible-comment-card" : "hidden-comment-card"
            }
          >
            {isAddComment && currentEvent && (
              <CommentForm
                id={currentEvent.Id}
                commentOf="event"
                handleAddComment={handleAddComment}
              />
            )}
          </div>
        </div>
        {events.length > 1 && (
          <div className="dot-container">
            {events.map((slide, index) => (
              <span
                key={index}
                className={`dot ${index === currentEventIndex ? "active" : ""}`}
                onClick={() => setCurrentEventIndex(index)}
              ></span>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default Event;
