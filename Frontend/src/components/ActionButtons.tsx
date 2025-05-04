import React from "react";

const ActionButtons: React.FC = () => {
  return (
    <div className="flex gap-3">
      <button className="bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-lg shadow transition">
        Approve
      </button>
      <button className="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg shadow transition">
        Dismiss
      </button>
      <button className="bg-neutral-500 hover:bg-neutral-600 text-white px-4 py-2 rounded-lg shadow transition">
        Export
      </button>
    </div>
  );
};

export default ActionButtons;
