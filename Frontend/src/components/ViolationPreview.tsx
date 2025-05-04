import React from "react";

interface Violation {
  displayName: string;
  violationExplanation: string;
}

interface Props {
  violation: Violation;
}

const ViolationPreview: React.FC<Props> = ({ violation }) => {
  const { violationExplanation } = violation;

  // Example keywords you may want to highlight
  const keywordsToHighlight = [
    "password",
    "credentials",
    "temporary",
    "shared",
  ];

  const regex = new RegExp(`(${keywordsToHighlight.join("|")})`, "gi");

  const highlighted = violationExplanation.split(regex).map((part, i) => (
    <span
      key={i}
      className={
        keywordsToHighlight.includes(part.toLowerCase())
          ? "text-red-500 font-semibold"
          : ""
      }
    >
      {part}
    </span>
  ));

  return (
    <div className="bg-gray-50 border-l-4 border-red-500 p-4 mt-2 rounded">
      <h3 className="text-md font-semibold text-gray-700 mb-2">
        Violation Preview
      </h3>
      <p className="text-gray-600">{highlighted}</p>
    </div>
  );
};

export default ViolationPreview;
