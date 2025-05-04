// src/components/ChatHistory.tsx
import React from "react";

const ChatHistory: React.FC = () => {
  const history = [
    {
      question: "Can employees share data externally?",
      answer: "No, it’s against policy.",
    },
    {
      question: "What’s the retention period for logs?",
      answer: "Minimum 7 years.",
    },
  ];

  return (
    <div className="bg-white rounded-xl shadow-md p-6">
      <h2 className="text-xl font-semibold text-neutral-800 mb-4">
        Chat History
      </h2>
      <ul className="space-y-3">
        {history.map((item, idx) => (
          <li key={idx} className="border-b border-neutral-200 pb-2">
            <p className="text-neutral-600 text-sm">Q: {item.question}</p>
            <p className="text-neutral-800 text-sm font-medium">
              A: {item.answer}
            </p>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default ChatHistory;
