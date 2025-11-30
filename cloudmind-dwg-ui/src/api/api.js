const API_BASE = "http://localhost:7071/api";

// GET SAS URL for upload
export async function getUploadSas(filename) {
  const res = await fetch(`${API_BASE}/get-upload-sas?file=${filename}`);
  return await res.json();
}

// GET SAS URL for output PDF
export async function getPdfSas(filename) {
  const res = await fetch(`${API_BASE}/get-pdf-sas?file=${filename}`);
  return await res.json();
}

// LIST processed PDFs
export async function listProcessed() {
  const res = await fetch(`${API_BASE}/list-pdfs`);
  return await res.json();
}
