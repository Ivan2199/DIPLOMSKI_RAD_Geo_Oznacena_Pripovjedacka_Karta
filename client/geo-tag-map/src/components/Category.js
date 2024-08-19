import React, { useState, useEffect } from "react";
import CategoryService from "../services/CategoryService";

function Category({ categoryId }) {
  const [category, setCategory] = useState(null);

  useEffect(() => {
    const fetchCategory = async () => {
      try {
        const response = await CategoryService.getCategoryById(categoryId);
        const lastUnderscoreIndex = response.Icon.lastIndexOf("_");
        const modifiedIcon =
          response.Icon.slice(0, lastUnderscoreIndex) +
          "_44" +
          response.Icon.slice(lastUnderscoreIndex + 1);
        response.Icon = modifiedIcon;
        setCategory(response);
      } catch (error) {
        console.error("Error fetching Category:", error);
      }
    };

    fetchCategory();
  }, [categoryId]);

  if (!category) {
    return null;
  }

  return (
    <div className="tourist-site-map-tile-category">
      <img src={category.Icon} alt={category.Type} title={category.Type} />
      <p>{category.Type}</p>
    </div>
  );
}

export default Category;
