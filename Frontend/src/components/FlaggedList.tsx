import React from "react";

const FlaggedList: React.FC = () => {
  const flaggedItems = [
    { id: "1", type: "Email", summary: "Confidential data sent externally." },
    { id: "2", type: "Chat", summary: "Harassment language detected." },
    { id: "3", type: "Document", summary: "Phishing attempt logged." },
  ];

  return (
    <div className="bg-white rounded-2xl shadow-md p-6 hover:shadow-lg transition-shadow flex-1 overflow-y-auto">
      <h2 className="text-2xl font-semibold text-neutral-900 mb-4">
        Flagged Items
      </h2>
      <ul className="space-y-3">
        {flaggedItems.map((item) => (
          <li
            key={item.id}
            className="p-3 border border-neutral-200 rounded-lg hover:bg-neutral-50 cursor-pointer transition"
          >
            <p className="text-neutral-700 font-medium">{item.type}</p>
            <p className="text-sm text-neutral-500">{item.summary}</p>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default FlaggedList;
