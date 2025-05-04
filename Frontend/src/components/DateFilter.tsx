import React, { useState } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { CalendarDays } from "lucide-react";

const DateFilter: React.FC = () => {
  const [startDate, setStartDate] = useState<Date | null>(new Date());
  const [endDate, setEndDate] = useState<Date | null>(new Date());

  return (
    <div className="bg-white rounded-2xl shadow-md px-4 py-3 flex items-center gap-5 hover:shadow-lg transition-shadow">
      <CalendarDays className="text-red-500" size={20} />
      <div className="flex items-center gap-2">
        <DatePicker
          selected={startDate}
          onChange={(date) => setStartDate(date)}
          selectsStart
          startDate={startDate}
          endDate={endDate}
          placeholderText="Start Date"
          className="border border-neutral-300 rounded-lg px-2 py-1 text-sm text-neutral-700 focus:ring-2 focus:ring-red-400 focus:outline-none"
          dateFormat="yyyy-MM-dd"
        />
        <span className="text-neutral-500 text-sm">to</span>
        <DatePicker
          selected={endDate}
          onChange={(date) => setEndDate(date)}
          selectsEnd
          startDate={startDate}
          endDate={endDate}
          minDate={startDate || new Date()}
          placeholderText="End Date"
          className="border border-neutral-300 rounded-lg px-2 py-1 text-sm text-neutral-700 focus:ring-2 focus:ring-red-400 focus:outline-none"
          dateFormat="yyyy-MM-dd"
        />
      </div>
    </div>
  );
};

export default DateFilter;

// import React from "react";
// import { CalendarDays } from "lucide-react"; // Optional icon

// const DateFilter: React.FC = () => {
//   return (
//     <div className="bg-white rounded-2xl shadow-md px-4 py-3 flex items-center gap-3 hover:shadow-lg transition-shadow">
//       <CalendarDays className="text-red-500" size={20} />
//       <div className="flex items-center gap-2">
//         <input
//           type="date"
//           className="border border-neutral-300 rounded-lg px-2 py-1 text-sm text-neutral-700 focus:ring-2 focus:ring-red-400 focus:outline-none"
//         />
//         <span className="text-neutral-500 text-sm">to</span>
//         <input
//           type="date"
//           className="border border-neutral-300 rounded-lg px-2 py-1 text-sm text-neutral-700 focus:ring-2 focus:ring-red-400 focus:outline-none"
//         />
//       </div>
//     </div>
//   );
// };

// export default DateFilter;
