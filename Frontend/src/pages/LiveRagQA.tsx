// src/pages/LiveRagQA.tsx
import React, { useState } from "react";
import QueryInput from "../components/QueryInput";
import AnswerPanel from "../components/AnswerPanel";
import SourceViewer from "../components/SourceViewer";
import VoiceInput from "../components/VoiceInput";
import ChatHistory from "../components/ChatHistory";

const LiveRagQA: React.FC = () => {
  const [query, setQuery] = useState("");
  const [answer, setAnswer] = useState<string | null>(null);

  const handleQuerySubmit = async (input: string) => {
    setQuery(input);
    setAnswer(null); // Clear previous answer

    try {
      const res = await fetch("https://localhost:7205/api/Compliance/FetchMsgFromSearch", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(input),
      });

      const data = await res.text();
      setAnswer(data);
    } catch (error) {
      console.error("Error fetching compliance answer:", error);
      setAnswer("An error occurred while fetching the answer.");
    }
  };

  return (
    <div className="min-h-screen bg-neutral-100 p-8">
      <header className="text-3xl font-bold text-neutral-800 mb-8">
        Live Compliance Q&A
      </header>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-8">
        <div className="col-span-2 space-y-6">
          <QueryInput onSubmit={handleQuerySubmit} />
          <AnswerPanel answer={answer} />
          <SourceViewer />
        </div>

        <div className="space-y-6">
          <VoiceInput />
          <ChatHistory />
        </div>
      </div>
    </div>
  );
};

export default LiveRagQA;
