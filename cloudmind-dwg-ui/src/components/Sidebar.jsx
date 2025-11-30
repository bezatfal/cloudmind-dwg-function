export default function Sidebar() {
  return (
    <aside className="w-64 min-h-screen bg-gray-900 text-white p-6 space-y-6">
      <h2 className="text-xl font-bold">Cloudmind DWG</h2>

      <nav className="space-y-4">
        <a href="/upload" className="block hover:text-blue-400">Upload</a>
        <a href="/processed" className="block hover:text-blue-400">Processed</a>
        <a href="/settings" className="block hover:text-blue-400">Settings</a>
      </nav>
    </aside>
  );
}
