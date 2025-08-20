// Project: Kids Story Creator - Web App with Azure AI Integration

// This document includes:
// - React frontend (App.jsx + full frontend folder structure)
// - package.json and Tailwind config
// - Azure Function backend (C#)
// - Prompt template
// - Azure deployment config (YAML)
// - Azure CLI script to provision resources
// - DALL·E Illustration generation
// - README.md for setup instructions

/* --- FRONTEND (React - App.jsx) --- */

import { useState } from 'react';
import girafishImage from './assets/girafish-inspiration.png';

function formDataToJson(formData) {
  const obj = {};
  formData.forEach((value, key) => {
    if (value instanceof File) {
      // Convert file to base64
      const reader = new FileReader();
      return new Promise((resolve, reject) => {
        reader.onload = () => {
          obj[key] = reader.result.split(',')[1]; // base64 string
          resolve();
        };
        reader.onerror = reject;
        reader.readAsDataURL(value);
      });
    } else {
      obj[key] = value;
    }
  });
  return Promise.resolve(obj);
}


export default function App() {
  const [category, setCategory] = useState("Adventure");
  const [ideas, setIdeas] = useState("");
  const [image, setImage] = useState(null);
  const [story, setStory] = useState("");
  const [illustration, setIllustration] = useState("");
  // New state variable for loading
  const [isLoading, setIsLoading] = useState(false);
  
  const submitStory = async () => {
    const formData = new FormData();
    formData.append("category", category);
    formData.append("ideas", ideas);
    formData.append("image", image);

    // Convert FormData to JSON for logging
    const jsonData = await formDataToJson(formData);
    console.log("Submitting data:", jsonData);

    // Set loading state to true
    setIsLoading(true);

    // Send the FormData to the Azure Function
    try {
      const res = await fetch("http://localhost:7071/api/GenerateStoryFunction", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(jsonData) // Correctly stringify the JSON data
      });

      if (!res.ok) {
        // Handle non-successful HTTP responses (e.g., 400, 500)
        const errorData = await res.text(); // Or res.json() if the error response is JSON
        console.error("Fetch failed with status:", res.status, errorData);
        // You might want to show an error message to the user
        return; // Stop execution if the response was not OK
      }

      const data = await res.json();
      setStory(data.story);
      setIllustration(data.illustration);

    } catch (error) {
      // This catch block will handle the "Failed to fetch" TypeError and other network errors
      console.error("Failed to fetch:", error);
      // You might want to show a user-friendly error message indicating a network problem
    }finally {
    // Set loading state to false, regardless of success or failure
    setIsLoading(false);
  }
  };

  return (
    <div className="p-6 max-w-lg mx-auto">
      <div className="bg-blue-50 p-4 rounded mb-6 shadow-md">
        <h1 className="text-3xl font-bold text-center text-purple-700 mb-2">Unleash Your Imagination!</h1>
        <p className="text-gray-700 text-center">Just like Luka imagined the incredible Girafish, you can create your own characters and stories. Upload your drawing, add your ideas, and let AI bring your imagination to life!</p>
        <img src={girafishImage} alt="Girafish inspiration" className="mx-auto mt-4 rounded-lg shadow-lg w-2/3" />
      </div>

      <h2 className="text-2xl font-semibold mb-2">Create Your Story!</h2>
      <select value={category} onChange={(e) => setCategory(e.target.value)} className="w-full mb-2 border p-2 rounded">
        {["Adventure", "Fairy Tale", "Animal", "Space", "Mystery"].map((c) => (
          <option key={c}>{c}</option>
        ))}
      </select>
      <textarea
        placeholder="Describe your characters and ideas!"
        value={ideas}
        onChange={(e) => setIdeas(e.target.value)}
        className="w-full mb-2 border rounded p-2"
      />
      <input type="file" onChange={(e) => setImage(e.target.files[0])} className="mb-2" />
      <button onClick={submitStory} className="w-full bg-blue-500 text-white p-2 rounded disabled:bg-gray-400" disabled={isLoading}>
        {isLoading ? "Generating..." : "Generate Story"}
      </button>

      {/* Loading animation */}
      {isLoading && (
        <div className="flex justify-center mt-4">
          <div className="w-8 h-8 border-4 border-t-4 border-blue-200 border-solid rounded-full animate-spin"></div>
        </div>
      )}

      {illustration && <img src={illustration} alt="Story Illustration" className="mt-4 rounded shadow" />}

      {story && <div className="mt-4 p-4 bg-gray-100 border rounded whitespace-pre-wrap">{story}</div>}
   
    </div>
  );
}
