import axios from "axios";
const BASE_URL = "https://localhost:44395/api/performer";

const getAccessToken = () => {
  return localStorage.getItem("access_token");
};

const PerformerService = {
  fetchPerformers: async () => {
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
      console.error("Error while fetching performers:", error);
      throw error;
    }
  },

  getPerformerById: async (id) => {
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
      console.log(response.data);

      return response.data;
    } catch (error) {
      console.error("Error while fetching performer:", error);
      throw error;
    }
  },

  getPerformersFiltered: async (formData) => {
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

      if (formData.bandOrMusician) {
        queryParams.append("bandOrMusician", formData.bandOrMusician);
      }

      if (formData.performanceDate) {
        queryParams.append("performanceDate", formData.performanceDate);
      }
      if (formData.searchKeyword) {
        queryParams.append("searchKeyword", formData.searchKeyword);
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
      console.error("Error while fetching performers:", error);
      throw error;
    }
  },
};

export default PerformerService;
