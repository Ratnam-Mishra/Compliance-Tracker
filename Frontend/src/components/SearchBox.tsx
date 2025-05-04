import React from "react";

interface Props {
  value: string;
  onChange: (value: string) => void;
}

const SearchBox: React.FC<Props> = ({ value, onChange }) => {
  return (
    <input
      type="text"
      placeholder="Search documents..."
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="border border-neutral-300 rounded-lg px-3 py-2 text-sm text-neutral-700 focus:ring-2 focus:ring-red-400 focus:outline-none w-64"
    />
  );
};

export default SearchBox;
