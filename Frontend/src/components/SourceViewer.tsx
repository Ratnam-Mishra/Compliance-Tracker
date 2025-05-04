// src/components/SourceViewer.tsx
import React from "react";

const SourceViewer: React.FC = () => {
  const sources = [
    {
      type: "Email",
      title: "Subject: Data Policy Reminder",
      snippet: "...confidential data must not...",
    },
    {
      type: "Chat",
      title: "Team Compliance Discussion",
      snippet: "...shared externally is against policy...",
    },
    {
      type: "Document",
      title: "Compliance Policy 2025.pdf",
      snippet: "...section 4.1 prohibits...",
    },
  ];

  return (
    <div className="bg-white rounded-xl shadow-md p-6">
      <h2 className="text-xl font-semibold text-neutral-800 mb-4">
        Source Evidence
      </h2>
      <ul className="space-y-3">
        {sources.map((src, idx) => (
          <li
            key={idx}
            className="p-3 bg-neutral-50 border border-neutral-200 rounded-lg"
          >
            <p className="text-sm font-medium text-neutral-600">
              {src.type}: {src.title}
            </p>
            <p className="text-neutral-700 text-sm">{src.snippet}</p>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default SourceViewer;
