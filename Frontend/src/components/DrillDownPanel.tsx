import React, { useState, useEffect } from "react";
import ViolationPreview from "./ViolationPreview";

type Violation = {
  email: string;
  displayName: string;
  violationExplanation: string;
};

const DrillDownPanel: React.FC = () => {
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const [violations, setViolations] = useState<Violation[]>([]);

  useEffect(() => {
    const fetchViolations = async () => {
      try {
        const response = await fetch("https://localhost:7205/api/Compliance/GetViolationDetails");
        if (!response.ok) {
          throw new Error("Failed to fetch violation details");
        }

        const data = await response.json();

        // Transform keys to match the Violation type
        const mappedViolations: Violation[] = data.map((v: any) => ({
          email: v.email,
          displayName: v.displayName,
          violationExplanation: v.violationExplanation,
        }));
        

        setViolations(mappedViolations);
      } catch (error) {
        console.error("Error fetching violation data:", error);
      }
    };

    fetchViolations();
  }, []);

  return (
    <div className="bg-white rounded-2xl shadow-md p-6 h-100 hover:shadow-lg transition-shadow">
      <h2 className="text-2xl font-semibold text-neutral-900 mb-6">
        Violation Details
      </h2>

      <ul className="space-y-3">
        {violations.map((violation, index) => (
          <li
            key={index}
            className={`p-4 border rounded-xl cursor-pointer transition-colors ${
              selectedIndex === index
                ? "bg-neutral-100 border-red-400"
                : "border-neutral-200 hover:bg-neutral-50"
            }`}
            onClick={() => setSelectedIndex(index)}
          >
            <p className="text-neutral-700 truncate font-medium">
              {violation.violationExplanation}
              <span className="text-red-500 ml-2">[{violation.displayName}]</span>
            </p>
          </li>
        ))}
      </ul>

      {selectedIndex !== null && (
        <div className="mt-6">
          <ViolationPreview violation={violations[selectedIndex]} />
        </div>
      )}
    </div>
  );
};

export default DrillDownPanel;
