import { useState } from 'react';

export default function App() {
  const [category, setCategory] = useState("Adventure");
  const [ideas, setIdeas] = useState("");
  const [image, setImage] = useState(null);
  const [story, setStory] = useState("");
  const [illustration, setIllustration] = useState("");

  const submitStory = async () => {
    const formData = new FormData();
    formData.append("category", category);
    formData.append("ideas", ideas);
    formData.append("image", image);

    const res = await fetch("https://<your-function-app>.azurewebsites.net/api/GenerateStoryFunction", {
      method: "POST",
      body: formData
    });

    const data = await res.json();
    setStory(data.story);
    setIllustration(data.illustration);
  };

  return (
    <div className="p-6 max-w-lg mx-auto">
      <h1 className="text-2xl font-bold">Create Your Story!</h1>
      <select value={category} onChange={(e) => setCategory(e.target.value)}>
        {["Adventure", "Fairy Tale", "Animal", "Space", "Mystery"].map((c) => (
          <option key={c}>{c}</option>
        ))}
      </select>
      <textarea
        placeholder="Describe your characters and ideas!"
        value={ideas}
        onChange={(e) => setIdeas(e.target.value)}
        className="w-full mt-2 border rounded p-2"
      />
      <input type="file" onChange={(e) => setImage(e.target.files[0])} />
      <button onClick={submitStory} className="mt-4 bg-blue-500 text-white p-2 rounded">Generate Story</button>
      {story && <div className="mt-4 p-4 bg-gray-100 border rounded">{story}</div>}
      {illustration && <img src={illustration} alt="Story Illustration" className="mt-4 rounded shadow" />}
    </div>
  );
}