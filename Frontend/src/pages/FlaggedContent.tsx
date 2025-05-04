import React from "react";
import FlaggedList from "../components/FlaggedList";
import ContentDetails from "../components/ContentDetails";
import ActionButtons from "../components/ActionButtons";
import Tagging from "../components/Tagging";

const FlaggedContent: React.FC = () => {
  return (
    <div className="h-full w-full p-10 bg-neutral-100">
      <h1 className="text-4xl font-bold text-neutral-900 mb-10">
        Flagged Content
      </h1>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-8 h-[calc(100vh-10rem)]">
        {/* Left: List */}
        <div className="col-span-1 flex flex-col gap-4">
          <FlaggedList />
        </div>

        {/* Right: Details + Actions */}
        <div className="col-span-2 flex flex-col gap-6">
          <ContentDetails />
          <div className="flex gap-4">
            <ActionButtons />
            <Tagging />
          </div>
        </div>
      </div>
    </div>
  );
};

export default FlaggedContent;
