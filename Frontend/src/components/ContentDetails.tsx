import React from "react";

const ContentDetails: React.FC = () => {
  const content = "Here is the full flagged content showing a breach...";

  return (
    <div className="bg-white rounded-2xl shadow-md p-6 hover:shadow-lg transition-shadow flex-1">
      <h2 className="text-2xl font-semibold text-neutral-900 mb-4">
        Content Details
      </h2>
      <p className="text-neutral-700 leading-relaxed">{content}</p>
    </div>
  );
};

export default ContentDetails;
