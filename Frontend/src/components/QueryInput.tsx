// src/components/QueryInput.tsx
import React, { useState } from "react";

type Props = {
  onSubmit: (query: string) => void;
};

const QueryInput: React.FC<Props> = ({ onSubmit }) => {
  const [query, setQuery] = useState("");

  const handleSubmit = () => {
    if (query.trim()) {
      onSubmit(query);
      setQuery(""); // Optional: clear input
    }
  };

  return (
    <div className="bg-white rounded-xl shadow-md p-6">
      <h2 className="text-xl font-semibold text-neutral-800 mb-4">
        Ask a Compliance Question
      </h2>
      <div className="flex gap-2">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="e.g., Are employees allowed to share confidential data externally?"
          className="w-full p-3 border border-neutral-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500 transition"
        />
        <button
          onClick={handleSubmit}
          className="bg-red-500 text-white px-4 rounded-lg hover:bg-red-600 transition"
        >
          Ask
        </button>
      </div>
    </div>
  );
};

export default QueryInput;
