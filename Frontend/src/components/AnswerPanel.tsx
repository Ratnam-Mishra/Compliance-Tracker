// src/components/AnswerPanel.tsx
import React from "react";

type Props = {
  answer: string | null;
};

const AnswerPanel: React.FC<Props> = ({ answer }) => {
  return (
    <div className="bg-white rounded-xl shadow-md p-6">
      <h2 className="text-xl font-semibold text-neutral-800 mb-4">Answer</h2>
      <p className="text-neutral-700 whitespace-pre-line">
        {answer || "Ask a question to see the answer here."}
      </p>
    </div>
  );
};

export default AnswerPanel;
