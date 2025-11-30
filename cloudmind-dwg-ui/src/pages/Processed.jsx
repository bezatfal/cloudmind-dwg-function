import { useEffect, useState } from "react";
import { listProcessed, getPdfSas } from "../api/api";

export default function Processed() {
  const [files, setFiles] = useState([]);

  useEffect(() => {
    async function loadFiles() {
      const result = await listProcessed();
      setFiles(result.files || []);
    }
    
    loadFiles();
  }, []);

  async function download(name) {
    const sas = await getPdfSas(name);
    window.open(sas.url, "_blank");
  }

  return (
    <div className="p-6">
      <h2 className="text-xl font-bold mb-4">Processed PDFs</h2>

      {files.map((f) => (
        <div key={f} className="flex justify-between p-2 border-b">
          <span>{f}</span>
          <button
            onClick={() => download(f)}
            className="px-3 py-1 bg-green-600 text-white rounded"
          >
            Download
          </button>
        </div>
      ))}
    </div>
  );
}
