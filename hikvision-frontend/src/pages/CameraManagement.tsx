import { useEffect, useState } from "react";
import axios from "axios";

const API_URL = "http://localhost:5018/api";

interface Camera {
  id: number;
  name: string;
  ipAddress: string;
  location: string;
  status: string;

  ftpHost?: string;
  ftpPort?: number;
  ftpUsername?: string;
  ftpPassword?: string;
  ftpFolder?: string;

  isIsapiOnline: boolean;
  lastIsapiReceivedAt?: string | null;
  lastIsapiEventType?: string | null;
  lastIsapiPlateNumber?: string | null;
  lastIsapiError?: string | null;

  httpPort: number;
  sdkPort: number;
  hikvisionUsername?: string;
  hikvisionPassword?: string;
  isapiChannel: number;
}

interface CameraForm {
  name: string;
  ipAddress: string;
  location: string;

  httpPort: number;
  sdkPort: number;
  hikvisionUsername: string;
  hikvisionPassword: string;
  isapiChannel: number;
}

const emptyForm: CameraForm = {
  name: "",
  ipAddress: "",
  location: "",

  httpPort: 80,
  sdkPort: 8000,
  hikvisionUsername: "",
  hikvisionPassword: "",
  isapiChannel: 1,
};

export default function CameraManagement() {
  const [cameras, setCameras] = useState<Camera[]>([]);

  const [form, setForm] = useState<CameraForm>(emptyForm);

  const [editingId, setEditingId] = useState<number | null>(null);

  const [showForm, setShowForm] = useState(false);

  const [loading, setLoading] = useState(false);

  const [message, setMessage] = useState("");

  const [error, setError] = useState("");

  // ==========================================
  // LOAD CAMERAS
  // ==========================================

  const loadCameras = async () => {
    try {
      setLoading(true);
      setError("");

      const response = await axios.get(
        `${API_URL}/Camera/all`
      );

      /*
       Your ResponceDTO may return the data
       in different property names.

       We handle common possibilities.
      */

      const result = response.data;

console.log("CAMERA API RESPONSE:", result);

if (Array.isArray(result)) {
  setCameras(result);
} else if (Array.isArray(result.data)) {
  setCameras(result.data);
} else if (Array.isArray(result.result)) {
  setCameras(result.result);
} else if (Array.isArray(result.response)) {
  setCameras(result.response);
} else if (Array.isArray(result.items)) {
  setCameras(result.items);
} else if (Array.isArray(result.cameras)) {
  setCameras(result.cameras);
} else {
  console.error("Unexpected Camera API response:", result);
  setCameras([]);
}
    } catch (err) {
      console.error(err);

      setError(
        "Unable to load cameras."
      );
    } finally {
      setLoading(false);
    }
  };

  // ==========================================
  // LOAD ON PAGE OPEN
  // ==========================================

  useEffect(() => {
    loadCameras();
  }, []);

  // ==========================================
  // FORM CHANGE
  // ==========================================

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const { name, value } = e.target;

    setForm((previous) => ({
      ...previous,
      [name]:
        name === "httpPort" ||
        name === "sdkPort" ||
        name === "isapiChannel"
          ? Number(value)
          : value,
    }));
  };

  // ==========================================
  // OPEN ADD FORM
  // ==========================================

  const openAddForm = () => {
    setEditingId(null);

    setForm(emptyForm);

    setMessage("");

    setError("");

    setShowForm(true);
  };

  // ==========================================
  // OPEN EDIT FORM
  // ==========================================

  const openEditForm = (camera: Camera) => {
    setEditingId(camera.id);

    setForm({
      name: camera.name || "",
      ipAddress: camera.ipAddress || "",
      location: camera.location || "",

      httpPort: camera.httpPort || 80,
      sdkPort: camera.sdkPort || 8000,

      hikvisionUsername:
        camera.hikvisionUsername || "",

      hikvisionPassword:
        camera.hikvisionPassword || "",

      isapiChannel:
        camera.isapiChannel || 1,
    });

    setMessage("");

    setError("");

    setShowForm(true);
  };

  // ==========================================
  // SAVE CAMERA
  // ==========================================

  const handleSubmit = async (
    e: React.FormEvent
  ) => {
    e.preventDefault();

    setMessage("");

    setError("");

    if (!form.name.trim()) {
      setError("Camera name is required.");
      return;
    }

    if (!form.ipAddress.trim()) {
      setError("Camera IP address is required.");
      return;
    }

    if (!form.location.trim()) {
      setError("Camera location is required.");
      return;
    }

    try {
      setLoading(true);

      const payload = {
        name: form.name.trim(),

        ipAddress:
          form.ipAddress.trim(),

        location:
          form.location.trim(),

        httpPort:
          form.httpPort || 80,

        sdkPort:
          form.sdkPort || 8000,

        hikvisionUsername:
          form.hikvisionUsername.trim(),

        hikvisionPassword:
          form.hikvisionPassword,

        isapiChannel:
          form.isapiChannel || 1,

        status: "Offline",

        isIsapiOnline: false,
      };

      // ==========================================
      // ADD
      // ==========================================

      if (editingId === null) {
        const response =
          await axios.post(
            `${API_URL}/Camera/add`,
            payload
          );

        if (
          response.data?.success === false
        ) {
          setError(
            response.data.message ||
              "Unable to add camera."
          );

          return;
        }

        setMessage(
          "Camera added successfully."
        );
      }

      // ==========================================
      // UPDATE
      // ==========================================

      else {
        const response =
          await axios.put(
            `${API_URL}/Camera/update/${editingId}`,
            payload
          );

        if (
          response.data?.success === false
        ) {
          setError(
            response.data.message ||
              "Unable to update camera."
          );

          return;
        }

        setMessage(
          "Camera updated successfully."
        );
      }

      setShowForm(false);

      setEditingId(null);

      setForm(emptyForm);

      await loadCameras();

    } catch (err: any) {
      console.error(err);

      setError(
        err.response?.data?.message ||
          "Unable to save camera."
      );
    } finally {
      setLoading(false);
    }
  };

  // ==========================================
  // DELETE CAMERA
  // ==========================================

  const handleDelete = async (
    id: number
  ) => {
    const confirmed =
      window.confirm(
        "Are you sure you want to delete this camera?"
      );

    if (!confirmed) {
      return;
    }

    try {
      setLoading(true);

      setError("");

      await axios.delete(
        `${API_URL}/Camera/delete/${id}`
      );

      setMessage(
        "Camera deleted successfully."
      );

      await loadCameras();

    } catch (err: any) {
      console.error(err);

      setError(
        err.response?.data?.message ||
          "Unable to delete camera."
      );
    } finally {
      setLoading(false);
    }
  };

  // ==========================================
  // CLOSE FORM
  // ==========================================

  const closeForm = () => {
    setShowForm(false);

    setEditingId(null);

    setForm(emptyForm);

    setMessage("");

    setError("");
  };

  // ==========================================
  // FORMAT DATE
  // ==========================================

  const formatDate = (
    date?: string | null
  ) => {
    if (!date) {
      return "-";
    }

    return new Date(date).toLocaleString();
  };

  // ==========================================
  // UI
  // ==========================================

  return (
    <div
      style={{
        padding: "24px",
        maxWidth: "1200px",
        margin: "0 auto",
      }}
    >

      {/* ======================================
          HEADER
      ====================================== */}

      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "24px",
        }}
      >
        <div>
          <h1
            style={{
              margin: 0,
              fontSize: "28px",
            }}
          >
            Camera Management
          </h1>

          <p
            style={{
              marginTop: "6px",
              color: "#666",
            }}
          >
            Manage Hikvision cameras and ISAPI
            connection settings.
          </p>
        </div>

        <button
          onClick={openAddForm}
          style={{
            padding: "10px 18px",
            border: "none",
            borderRadius: "6px",
            cursor: "pointer",
            background: "#2563eb",
            color: "white",
            fontWeight: 600,
          }}
        >
          + Add Camera
        </button>
      </div>

      {/* ======================================
          MESSAGE
      ====================================== */}

      {message && (
        <div
          style={{
            marginBottom: "16px",
            padding: "12px",
            background: "#dcfce7",
            color: "#166534",
            borderRadius: "6px",
          }}
        >
          {message}
        </div>
      )}

      {error && (
        <div
          style={{
            marginBottom: "16px",
            padding: "12px",
            background: "#fee2e2",
            color: "#991b1b",
            borderRadius: "6px",
          }}
        >
          {error}
        </div>
      )}

      {/* ======================================
          ADD / EDIT FORM
      ====================================== */}

      {showForm && (
        <div
          style={{
            border: "1px solid #ddd",
            borderRadius: "10px",
            padding: "24px",
            marginBottom: "24px",
            background: "#fff",
          }}
        >
          <h2>
            {editingId === null
              ? "Add Camera"
              : "Edit Camera"}
          </h2>

          <form onSubmit={handleSubmit}>

            {/* Camera Name */}

            <div style={{ marginBottom: "16px" }}>
              <label>
                Camera Name
              </label>

              <input
                type="text"
                name="name"
                value={form.name}
                onChange={handleChange}
                placeholder="Example: Front Gate"
                style={inputStyle}
              />
            </div>

            {/* IP */}

            <div style={{ marginBottom: "16px" }}>
              <label>
                IP Address
              </label>

              <input
                type="text"
                name="ipAddress"
                value={form.ipAddress}
                onChange={handleChange}
                placeholder="Example: 192.168.10.64"
                style={inputStyle}
              />
            </div>

            {/* Location */}

            <div style={{ marginBottom: "16px" }}>
              <label>
                Location
              </label>

              <input
                type="text"
                name="location"
                value={form.location}
                onChange={handleChange}
                placeholder="Example: Front Gate"
                style={inputStyle}
              />
            </div>

            {/* ==================================
                ISAPI SETTINGS
            ================================== */}

            <h3>
              Hikvision / ISAPI Settings
            </h3>

            <div
              style={{
                display: "grid",
                gridTemplateColumns:
                  "repeat(3, 1fr)",
                gap: "16px",
              }}
            >

              {/* HTTP */}

              <div>
                <label>
                  HTTP Port
                </label>

                <input
                  type="number"
                  name="httpPort"
                  value={form.httpPort}
                  onChange={handleChange}
                  style={inputStyle}
                />
              </div>

              {/* SDK */}

              <div>
                <label>
                  SDK Port
                </label>

                <input
                  type="number"
                  name="sdkPort"
                  value={form.sdkPort}
                  onChange={handleChange}
                  style={inputStyle}
                />
              </div>

              {/* Channel */}

              <div>
                <label>
                  ISAPI Channel
                </label>

                <input
                  type="number"
                  name="isapiChannel"
                  value={form.isapiChannel}
                  onChange={handleChange}
                  style={inputStyle}
                />
              </div>

            </div>

            {/* Username */}

            <div style={{ marginTop: "16px" }}>
              <label>
                Hikvision Username
              </label>

              <input
                type="text"
                name="hikvisionUsername"
                value={
                  form.hikvisionUsername
                }
                onChange={handleChange}
                placeholder="admin"
                style={inputStyle}
              />
            </div>

            {/* Password */}

            <div style={{ marginTop: "16px" }}>
              <label>
                Hikvision Password
              </label>

              <input
                type="password"
                name="hikvisionPassword"
                value={
                  form.hikvisionPassword
                }
                onChange={handleChange}
                placeholder="Camera password"
                style={inputStyle}
              />
            </div>

            {/* Buttons */}

            <div
              style={{
                display: "flex",
                gap: "10px",
                marginTop: "24px",
              }}
            >
              <button
                type="submit"
                disabled={loading}
                style={{
                  padding: "10px 20px",
                  border: "none",
                  borderRadius: "6px",
                  background: "#16a34a",
                  color: "white",
                  cursor: "pointer",
                }}
              >
                {loading
                  ? "Saving..."
                  : editingId === null
                  ? "Save Camera"
                  : "Update Camera"}
              </button>

              <button
                type="button"
                onClick={closeForm}
                style={{
                  padding: "10px 20px",
                  border: "1px solid #ccc",
                  borderRadius: "6px",
                  background: "white",
                  cursor: "pointer",
                }}
              >
                Cancel
              </button>
            </div>

          </form>
        </div>
      )}

      {/* ======================================
          CAMERA LIST
      ====================================== */}

      <div
        style={{
          display: "grid",
          gridTemplateColumns:
            "repeat(auto-fit, minmax(350px, 1fr))",
          gap: "20px",
        }}
      >

        {loading && cameras.length === 0 && (
          <p>
            Loading cameras...
          </p>
        )}

        {!loading &&
          cameras.length === 0 && (
            <div
              style={{
                padding: "40px",
                textAlign: "center",
                border: "1px dashed #ccc",
                borderRadius: "10px",
              }}
            >
              <h3>
                No cameras registered
              </h3>

              <p>
                Add your first Hikvision camera.
              </p>
            </div>
          )}

        {cameras.map((camera) => (
          <div
            key={camera.id}
            style={{
              border: "1px solid #ddd",
              borderRadius: "10px",
              padding: "20px",
              background: "#fff",
            }}
          >

            {/* Camera header */}

            <div
              style={{
                display: "flex",
                justifyContent:
                  "space-between",
                alignItems: "center",
                marginBottom: "16px",
              }}
            >
              <div>
                <h2
                  style={{
                    margin: 0,
                  }}
                >
                  {camera.name}
                </h2>

                <small>
                  Camera ID: {camera.id}
                </small>
              </div>

              <span
                style={{
                  padding: "5px 10px",
                  borderRadius: "20px",
                  fontSize: "12px",
                  fontWeight: 600,
                  background:
                    camera.isIsapiOnline
                      ? "#dcfce7"
                      : "#fee2e2",
                  color:
                    camera.isIsapiOnline
                      ? "#166534"
                      : "#991b1b",
                }}
              >
                {camera.isIsapiOnline
                  ? "ISAPI Online"
                  : "ISAPI Offline"}
              </span>
            </div>

            {/* Camera details */}

            <div
              style={{
                lineHeight: 1.8,
              }}
            >
              <div>
                <strong>
                  IP Address:
                </strong>{" "}
                {camera.ipAddress}
              </div>

              <div>
                <strong>
                  Location:
                </strong>{" "}
                {camera.location}
              </div>

              <div>
                <strong>
                  HTTP Port:
                </strong>{" "}
                {camera.httpPort}
              </div>

              <div>
                <strong>
                  SDK Port:
                </strong>{" "}
                {camera.sdkPort}
              </div>

              <div>
                <strong>
                  ISAPI Channel:
                </strong>{" "}
                {camera.isapiChannel}
              </div>

              <div>
                <strong>
                  Username:
                </strong>{" "}
                {camera.hikvisionUsername ||
                  "-"}
              </div>

              <div>
                <strong>
                  Last Event:
                </strong>{" "}
                {camera.lastIsapiEventType ||
                  "-"}
              </div>

              <div>
                <strong>
                  Last Plate:
                </strong>{" "}
                {camera.lastIsapiPlateNumber ||
                  "-"}
              </div>

              <div>
                <strong>
                  Last Received:
                </strong>{" "}
                {formatDate(
                  camera.lastIsapiReceivedAt
                )}
              </div>

              {camera.lastIsapiError && (
                <div
                  style={{
                    color: "#dc2626",
                  }}
                >
                  <strong>
                    Error:
                  </strong>{" "}
                  {camera.lastIsapiError}
                </div>
              )}
            </div>

            {/* Buttons */}

            <div
              style={{
                display: "flex",
                gap: "10px",
                marginTop: "20px",
              }}
            >
              <button
                onClick={() =>
                  openEditForm(camera)
                }
                style={{
                  flex: 1,
                  padding: "9px",
                  border: "1px solid #2563eb",
                  borderRadius: "6px",
                  background: "white",
                  color: "#2563eb",
                  cursor: "pointer",
                }}
              >
                Edit
              </button>

              <button
                onClick={() =>
                  handleDelete(camera.id)
                }
                style={{
                  flex: 1,
                  padding: "9px",
                  border: "none",
                  borderRadius: "6px",
                  background: "#dc2626",
                  color: "white",
                  cursor: "pointer",
                }}
              >
                Delete
              </button>
            </div>

          </div>
        ))}

      </div>

    </div>
  );
}

// ==========================================
// INPUT STYLE
// ==========================================

const inputStyle: React.CSSProperties = {
  width: "100%",
  padding: "10px",
  marginTop: "6px",
  border: "1px solid #ccc",
  borderRadius: "6px",
  boxSizing: "border-box",
};