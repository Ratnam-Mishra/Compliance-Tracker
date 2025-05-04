// src/pages/Home.tsx
import React from "react";
import ReportedEmployees from "../components/ReportedEmployees";
import KeywordGraph from "../components/KeywordGraph";
import DateFilter from "../components/DateFilter";
import DrillDownPanel from "../components/DrillDownPanel";
import TopKeywords from "../components/TopKeywords";

const Dashboard: React.FC = () => {
  return (
    <div className="bg-neutral-100 h-screen flex flex-col">
      {/* <header className="text-3xl font-bold text-neutral-800 mb-8">
        Compliance Dashboard
      </header> */}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-8">
        <div className="col-span-2 space-y-8">
          <ReportedEmployees />
          <KeywordGraph />
        </div>

        <div className="space-y-8">
          <DateFilter />
          <TopKeywords />
          <DrillDownPanel />
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
