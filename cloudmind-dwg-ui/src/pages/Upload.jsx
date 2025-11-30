import { useState } from "react";
import { getUploadSas } from "../api/api";

export default function Upload() {
  const [selectedFile, setSelectedFile] = useState(null);
  const [status, setStatus] = useState("");

  async function handleUpload() {
    if (!selectedFile) return;

    setStatus("Requesting SAS URL...");

    const sas = await getUploadSas(selectedFile.name);
    const url = sas.url;

    setStatus("Uploading to Azure...");

    await fetch(url, {
      method: "PUT",
      headers: {
        "x-ms-blob-type": "BlockBlob"
      },
      body: selectedFile
    });

    setStatus("Upload complete!");
  }

  return (
    <div className="p-6">
      <h2 className="text-xl font-bold mb-4">Upload Drawing</h2>

      <input
        type="file"
        onChange={(e) => setSelectedFile(e.target.files[0])}
        className="mb-4"
      />

      <button
        onClick={handleUpload}
        className="px-4 py-2 bg-blue-500 text-white rounded"
      >
        Upload File
      </button>

      <p className="mt-4">{status}</p>
    </div>
  );
}
