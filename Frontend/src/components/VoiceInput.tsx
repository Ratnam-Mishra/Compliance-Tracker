// src/components/VoiceInput.tsx
import React from "react";

const VoiceInput: React.FC = () => {
  const handleVoiceInput = () => {
    // Placeholder: Integrate Web Speech API or Whisper API
    alert("Voice input started...");
  };

  return (
    <div className="bg-white rounded-xl shadow-md p-6 text-center">
      <h2 className="text-xl font-semibold text-neutral-800 mb-4">
        Voice Search
      </h2>
      <button
        onClick={handleVoiceInput}
        className="inline-flex items-center justify-center px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition"
      >
        🎤 Start Voice Input
      </button>
    </div>
  );
};

export default VoiceInput;
