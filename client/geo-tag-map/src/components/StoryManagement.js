import React, { useEffect, useState } from "react";
import StoryService from "../services/StoryService";
import "../styles/StoryManagement.css";

function StoryManagement() {
  const [stories, setStories] = useState([]);
  const [pageSize, setPageSize] = useState(25);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalNumberOfStories, setTotalNumberOfStories] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [orderBy, setOrderBy] = useState("Date");
  const [sortOrder, setSortOrder] = useState("ASC");
  const [isReported, setIsReported] = useState(false);

  const getStories = async () => {
    setLoading(true);
    setError(null);
    try {
      const options = {
        orderBy,
        sortOrder,
        pageSize,
        pageNumber,
        isReported,
      };
      const response = await StoryService.fetchStories(options);
      console.log(response.List);
      setStories(response.List);
      setTotalNumberOfStories(response.TotalSize);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    getStories();
  }, [pageSize, pageNumber, orderBy, sortOrder, isReported]);

  const handleDeleteStory = async (storyId) => {
    try {
      await StoryService.deleteStoryAsync(storyId);
      getStories();
    } catch (error) {
      console.error("Error has occurred while deleting a story", error);
    }
  };

  return (
    <div className="story-management">
      <h1>Story Management</h1>
      <div className="controls">
        <label className="checkbox-container">
          Show Reported:
          <input
            type="checkbox"
            checked={isReported}
            onChange={(e) => setIsReported(e.target.checked)}
          />
          <span className="checkmark"></span>
        </label>
      </div>
      {loading ? (
        <p>Loading...</p>
      ) : (
        <>
          <table className="story-table">
            <thead>
              <tr>
                <th>Author</th>
                <th>Story Text</th>
                <th>Location</th>
                <th>Date</th>
                <th>Reported Story</th>
                <th>Number Of Likes</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {stories.map((story) => (
                <tr key={story.Id}>
                  <td>{story.User.Username}</td>
                  <td>{story.Text}</td>
                  <td>
                    <div className="story-location-admin-information">
                      <p>{story.Location.Country}</p>
                      <p>{story.Location.City}</p>
                    </div>
                  </td>
                  <td>{new Date(story.DateTime).toLocaleDateString()}</td>
                  <td className={story.IsReported ? "reported" : ""}>
                    {story.IsReported ? "true" : "false"}
                  </td>
                  <td>{story.NumberOfLikes || 0}</td>
                  <td>
                    <div className="story-action-buttons">
                      <div
                        className="delete-story-action-button"
                        onClick={() => handleDeleteStory(story.Id)}
                      >
                        Delete
                      </div>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="pagination">
            <button
              onClick={() => setPageNumber((prev) => Math.max(prev - 1, 1))}
              disabled={pageNumber === 1}
            >
              Previous
            </button>
            <span>
              Page {pageNumber} of {Math.ceil(totalNumberOfStories / pageSize)}
            </span>
            <button
              onClick={() => setPageNumber((prev) => prev + 1)}
              disabled={
                pageNumber >= Math.ceil(totalNumberOfStories / pageSize)
              }
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default StoryManagement;
