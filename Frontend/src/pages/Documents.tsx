import React, { useState } from "react";
import DocumentTable from "../components/DocumentTable";
import SearchBox from "../components/SearchBox";

const Documents: React.FC = () => {
  const [query, setQuery] = useState("");

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onloadend = async () => {
      const base64 = reader.result?.toString().split(",")[1];
      if (!base64) return;

      const res = await fetch("https://localhost:7205/api/Compliance/UploadFileToSP", {
        method: "POST",
        headers: { "Accept": "*/*","Content-Type": "application/json" },
        body: JSON.stringify({ name: file.name, fileBase64: base64 }),
      });

      const json = await res.json();
      alert(json.status ? "✅ Upload successful" : "❌ Upload failed");
    };
    reader.readAsDataURL(file);
  };

  return (
    <div className="h-228 w-full p-10 bg-neutral-100">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-4xl font-bold text-neutral-900">Document Viewer</h1>
        <div className="flex gap-4 items-center">
          <SearchBox value={query} onChange={setQuery} />
          <input
            type="file"
            onChange={handleFileUpload}
            className="text-sm text-neutral-600"
          />
        </div>
      </div>

      <DocumentTable searchQuery={query} />
    </div>
  );
};

export default Documents;