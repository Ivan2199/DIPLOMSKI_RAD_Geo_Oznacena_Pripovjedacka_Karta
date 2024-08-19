import axios from "axios";
const BASE_URL = "https://localhost:44395/api/event";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const EventService = {
  fetchEvents: async () => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      if (response.data) {
        console.log("Server Response:", response.data);
        return response.data;
      } else {
        console.error("Empty response from the server.");
      }
    } catch (error) {
      console.error("Error while fetching events:", error);
      throw error;
    }
  },

  getEventById: async (id) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/${id}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching event:", error);
      throw error;
    }
  },

  getEventsFiltered: async (formData) => {
    try {
      const token = getAccessToken();
      if (!token) {
        console.error("Access token not found");
        return;
      }

      const queryParams = new URLSearchParams();

      if (formData.name) {
        queryParams.append("name", formData.name);
      }

      if (formData.startDate) {
        queryParams.append("startDate", formData.startDate);
      }

      if (formData.status) {
        queryParams.append("status", formData.status);
      }

      if (formData.isAccessibleForFree) {
        queryParams.append("isAccessibleForFree", formData.isAccessibleForFree);
      }
      if (formData.type) {
        queryParams.append("type", formData.type);
      }
      if (formData.searchKeyword) {
        queryParams.append("searchKeyword", formData.searchKeyword);
      }
      if (formData.country) {
        queryParams.append("country", formData.country);
      }
      if (formData.city) {
        queryParams.append("city", formData.city);
      }
      if (formData.sortField) {
        queryParams.append("OrderBy", formData.sortField);
      }

      if (formData.sortOrder) {
        queryParams.append("SortOrder", formData.sortOrder);
      }

      if (formData.pageSize) {
        queryParams.append("pageSize", formData.pageSize);
      }

      if (formData.pageNumber) {
        queryParams.append("pageNumber", parseInt(formData.pageNumber, 10));
      }

      const urlWithParams = `${BASE_URL}?${queryParams.toString()}`;
      const response = await axios.get(urlWithParams, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      return response.data;
    } catch (error) {
      console.error("Error while fetching songs:", error);
      throw error;
    }
  },

  fetchMostLikedEvents: async () => {
    try {
      const token = getAccessToken();

      if (!token) {
        console.error("Access token not found");
        return;
      }

      const response = await axios.get(`${BASE_URL}/mostliked`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      if (response.data) {
        console.log("Server Response:", response.data);
        return response.data;
      } else {
        console.error("Empty response from the server.");
      }
    } catch (error) {
      console.error("Error while fetching most liked events:", error);
      throw error;
    }
  },

  updateEventAsync: async (formData, eventId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Token not found");
      return;
    }
    try {
      const response = await axios.put(`${BASE_URL}/${eventId}`, formData, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.data) {
        return response.data;
      } else {
        console.error("Empty response from the server.");
      }
    } catch (error) {
      console.error("Error updating event data: ", error.message);
      throw error;
    }
  },
};

export default EventService;
