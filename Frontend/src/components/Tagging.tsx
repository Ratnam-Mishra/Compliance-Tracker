import React from "react";

const Tagging: React.FC = () => {
  return (
    <div className="bg-white rounded-2xl shadow-md p-4 hover:shadow-lg transition-shadow flex items-center gap-3">
      <label className="text-neutral-700 font-medium">Tag:</label>
      <select className="border border-neutral-300 rounded-lg px-2 py-1 text-sm text-neutral-700 focus:ring-2 focus:ring-red-400 focus:outline-none">
        <option>Phishing</option>
        <option>Harassment</option>
        <option>Data Leak</option>
      </select>
    </div>
  );
};

export default Tagging;
