import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

const API_URL = "http://localhost:5018";

interface Camera {
  id: number;
  name: string;
  location: string;
  ipAddress: string;
  status: string;
  httpPort?: number;
  sdkPort?: number;
  isIsapiOnline?: boolean;
}

interface ApiResponse {
  success?: boolean;
  message?: string;
  data?: Camera[];
}

const Cameras = () => {
  const navigate = useNavigate();

  const [cameras, setCameras] = useState<Camera[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // ============================================================
  // LOAD CAMERAS FROM DATABASE
  // ============================================================

  const loadCameras = async () => {
    try {
      setLoading(true);
      setError("");

      const response = await fetch(
        `${API_URL}/api/Camera/all`
      );

      if (!response.ok) {
        throw new Error(
          `Failed to load cameras (${response.status})`
        );
      }

      const result: ApiResponse = await response.json();

      console.log("Camera API response:", result);

      if (Array.isArray(result.data)) {
        setCameras(result.data);
      } else {
        setCameras([]);
        throw new Error("Invalid camera data received");
      }
    } catch (err) {
      console.error("Camera loading error:", err);

      setError(
        err instanceof Error
          ? err.message
          : "Unable to load cameras"
      );
    } finally {
      setLoading(false);
    }
  };

  // ============================================================
  // LOAD WHEN PAGE OPENS
  // ============================================================

  useEffect(() => {
    loadCameras();
  }, []);

  // ============================================================
  // OPEN CAMERA
  // ============================================================

  const openCamera = (cameraId: number) => {
    navigate(`/cameras/${cameraId}/dashboard`);
  };

  // ============================================================
  // LOADING
  // ============================================================

  if (loading) {
    return (
      <div className="cameras-page">
        <h1>Cameras</h1>
        <p>Loading cameras...</p>
      </div>
    );
  }

  // ============================================================
  // ERROR
  // ============================================================

  if (error) {
    return (
      <div className="cameras-page">
        <h1>Cameras</h1>

        <div>
          <p>Failed to load cameras.</p>
          <p>{error}</p>

          <button onClick={loadCameras}>
            Retry
          </button>
        </div>
      </div>
    );
  }

  // ============================================================
  // DISPLAY CAMERAS
  // ============================================================

  return (
    <div className="cameras-page">

      <h1>Cameras</h1>

      {cameras.length === 0 ? (
        <p>No cameras found.</p>
      ) : (
        <div className="camera-grid">

          {cameras.map((camera) => (

            <div
              className="camera-card"
              key={camera.id}
            >

              <h2>{camera.name}</h2>

              <p>
                <strong>Location:</strong>{" "}
                {camera.location}
              </p>

              <p>
                <strong>IP Address:</strong>{" "}
                {camera.ipAddress}
              </p>

              <p>
                <strong>Status:</strong>{" "}
                {camera.status}
              </p>

              <button
                onClick={() =>
                  openCamera(camera.id)
                }
              >
                View Camera
              </button>

            </div>

          ))}

        </div>
      )}

    </div>
  );
};

export default Cameras;