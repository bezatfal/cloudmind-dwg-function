import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import Sidebar from "./components/Sidebar";
import Header from "./components/Header";

import Upload from "./pages/Upload";
import Processed from "./pages/Processed";
import Settings from "./pages/Settings";

export default function App() {
  return (
    <BrowserRouter>
      <div className="flex">
        <Sidebar />

        <div className="ml-64 flex-1 p-4">
          <Header />
          <main className="mt-4">
            <Routes>
              <Route path="/" element={<Navigate to="/upload" />} />
              <Route path="/upload" element={<Upload />} />
              <Route path="/processed" element={<Processed />} />
              <Route path="/settings" element={<Settings />} />
            </Routes>
          </main>
        </div>
      </div>
    </BrowserRouter>
  );
}
