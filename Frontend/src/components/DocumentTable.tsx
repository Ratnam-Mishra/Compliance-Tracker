import React, { useEffect, useState } from "react";

interface Props {
  searchQuery: string;
}

type Document = {
  fileName: string;
  fileUrl: string;
  embedded?: boolean;
};

const DocumentTable: React.FC<Props> = ({ searchQuery }) => {
  const [docs, setDocs] = useState<Document[]>([]);

  useEffect(() => {
    const fetchDocuments = async () => {
      try {
        const response = await fetch("https://localhost:7205/api/Compliance/GetDocuments");
        if (!response.ok) {
          throw new Error("Failed to fetch documents");
        }
        const data: Document[] = await response.json();
        setDocs(data);
      } catch (err: any) {
        console.error(err);
      }
    } 
    fetchDocuments();

  },[]);
  
  const filteredDocs = docs.filter((doc) =>
    doc.fileName.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="bg-white rounded-2xl shadow-md p-6 hover:shadow-lg transition-shadow">
      <h2 className="text-2xl font-semibold text-neutral-900 mb-4">
        Documents
      </h2>

      <table className="w-full text-left text-neutral-700">
        <thead>
          <tr className="text-neutral-500 text-sm border-b border-neutral-200">
            <th className="pb-3">Document</th>
            <th className="pb-3">Actions</th>
          </tr>
        </thead>
        <tbody>
          {filteredDocs.map((doc, index) => (
            <tr key={index} className="hover:bg-neutral-50 text-sm">
              <td className="py-3 font-medium">{doc.fileName}</td>
              <td className="py-3">
                <a
                  href={doc.fileUrl}
                  className="text-blue-500 hover:underline text-sm"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Open in SharePoint
                </a>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default DocumentTable;