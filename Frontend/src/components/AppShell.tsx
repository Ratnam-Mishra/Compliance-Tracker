// src/components/AppShell.tsx
import React from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import { Home, Search, FileText } from "lucide-react"; // Modern icons

const AppShell: React.FC = () => {
  const location = useLocation();

  const navItems = [
    { name: "Dashboard", path: "/", icon: <Home size={18} /> },
    { name: "Live RAG Q&A", path: "/live-rag", icon: <Search size={18} /> },
    // {
    //   name: "Flagged Content",
    //   path: "/flagged-content",
    //   icon: <Flag size={18} />,
    // },
    { name: "Documents", path: "/documents", icon: <FileText size={18} /> },
  ];

  return (
    <div className="flex">
      {/* Sidebar */}
      <aside className="w-64 bg-neutral-900 text-white flex flex-col justify-between">
        <div>
          <h1 className="text-2xl font-bold text-red-500 px-6 py-6">
            Compliance RAG
          </h1>
          <nav className="space-y-2 mt-4 px-4">
            {navItems.map((item) => (
              <Link
                key={item.path}
                to={item.path}
                className={`flex items-center gap-3 px-4 py-2 rounded-lg text-sm font-medium hover:bg-neutral-800 transition ${
                  location.pathname === item.path
                    ? "bg-neutral-800 text-red-400"
                    : "text-neutral-300"
                }`}
              >
                {item.icon} {item.name}
              </Link>
            ))}
          </nav>
        </div>
        <div className="px-6 py-4 text-xs text-neutral-500">
          &copy; 2025 Compliance AI
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 bg-neutral-100 p-7 overflow-y-auto">
        <Outlet />
      </main>
    </div>
  );
};

export default AppShell;
