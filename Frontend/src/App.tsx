import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import Dashboard from "./pages/Dashboard";
import AppShell from "./components/AppShell";
import LiveRagQA from "./pages/LiveRagQA";
// import FlaggedContent from "./pages/FlaggedContent";
import Documents from "./pages/Documents";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppShell />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/live-rag" element={<LiveRagQA />} />
          {/* <Route path="/flagged-content" element={<FlaggedContent />} /> */}
          <Route path="/documents" element={<Documents />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
