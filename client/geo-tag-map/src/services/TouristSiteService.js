import axios from "axios";
const BASE_URL = "https://localhost:44395/api/touristsite";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const TouristSiteService = {
  fetchTouristSites: async () => {
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
      console.error("Error while fetching tourist sites:", error);
      throw error;
    }
  },
  fetchMostLikedTouristSites: async () => {
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
      console.error("Error while fetching most liked tourist sites:", error);
      throw error;
    }
  },
  getTouristSiteById: async (id) => {
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
      console.error("Error while fetching tourist site:", error);
      throw error;
    }
  },

  getTouristSitesFiltered: async (formData) => {
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

      if (formData.popularity) {
        queryParams.append("popularity", formData.popularity);
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
        queryParams.append("pageSize", parseInt(formData.pageSize, 10));
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
  updateTouristSiteAsync: async (formData, touristSiteId) => {
    const token = getAccessToken();
    if (!token) {
      console.error("Token not found");
      return;
    }
    try {
      const response = await axios.put(
        `${BASE_URL}/${touristSiteId}`,
        formData,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (response.data) {
        return response.data;
      } else {
        console.error("Empty response from the server.");
      }
    } catch (error) {
      console.error("Error updating tourist site data: ", error.message);
      throw error;
    }
  },
};

export default TouristSiteService;
