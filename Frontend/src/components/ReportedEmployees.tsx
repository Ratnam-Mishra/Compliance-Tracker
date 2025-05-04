import React, { useEffect, useMemo, useState } from "react";
import {
  useReactTable,
  getCoreRowModel,
  getFilteredRowModel,
  getSortedRowModel,
  flexRender,
} from "@tanstack/react-table";

type Employee = {
  email: string;
  displayName: string;
  department: string;
  count: number;
};

const ReportedEmployees: React.FC = () => {
  const [globalFilter, setGlobalFilter] = useState("");
  const [employeeData, setEmployeeData] = useState<Employee[]>();

  useEffect(() => {
    const fetchEmployeeDetails = async () => {
      try {
        const response = await fetch(
          "https://localhost:7205/api/Compliance/GetEmployeeDetails"
        );
        if (!response.ok) {
          throw new Error("Failed to fetch employees data");
        }
        const data: Employee[] = await response.json();
        setEmployeeData(data);
      } catch (err: any) {
        console.error(err);
      }
    };
    fetchEmployeeDetails();
  }, []);

  const columns = useMemo(
    () => [
      {
        accessorKey: "displayName",
        header: "Employee",
      },
      {
        accessorKey: "count",
        header: "Violations",
      },
      {
        accessorKey: "department",
        header: "Department",
      },
    ],
    []
  );

  const table = useReactTable({
    data: employeeData ?? [],
    columns,
    state: {
      globalFilter,
    },
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getSortedRowModel: getSortedRowModel(),
  });

  return (
    <div className="bg-white rounded-2xl shadow-md p-6 hover:shadow-lg transition-shadow h-100 flex flex-col">
      <h2 className="text-2xl font-semibold text-neutral-900 mb-4">
        Flagged Employees
      </h2>
      <input
        type="text"
        placeholder="Search employees..."
        value={globalFilter ?? ""}
        onChange={(e) => setGlobalFilter(e.target.value)}
        className="border border-neutral-300 rounded-lg px-3 py-2 text-sm text-neutral-700 focus:ring-2 focus:ring-red-400 focus:outline-none mb-4 w-1/2"
      />
      <div className="overflow-x-auto">
        <table className="w-full text-left text-neutral-700 border-collapse">
          <thead className="bg-neutral-100 sticky top-0 z-10">
            {table.getHeaderGroups().map((headerGroup) => (
              <tr
                key={headerGroup.id}
                className="text-neutral-500 text-sm border-b border-neutral-200"
              >
                {headerGroup.headers.map((header) => (
                  <th
                    key={header.id}
                    onClick={header.column.getToggleSortingHandler()}
                    className="py-3 px-4 cursor-pointer select-none"
                  >
                    {flexRender(
                      header.column.columnDef.header,
                      header.getContext()
                    )}
                    {header.column.getIsSorted() === "asc" ? " ▲" : ""}
                    {header.column.getIsSorted() === "desc" ? " ▼" : ""}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
        </table>
      </div>

      <div className="overflow-y-auto max-h-96">
        <table className="w-full text-left text-neutral-700">
          <tbody>
            {table.getRowModel().rows.map((row) => (
              <tr
                key={row.id}
                className="hover:bg-neutral-50 transition-colors text-sm"
              >
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id} className="py-3 px-4 font-medium">
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default ReportedEmployees;
