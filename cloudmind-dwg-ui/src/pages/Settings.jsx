export default function Settings() {
  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold mb-4">Settings</h1>

      <div className="space-y-4">
        <div>
          <label className="block mb-1 font-medium">API Base URL</label>
          <input
            type="text"
            className="border p-2 rounded w-full"
            placeholder="http://localhost:7071"
          />
        </div>
      </div>
    </div>
  );
}
