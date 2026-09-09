import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const API_URL = "http://localhost:5018";

interface VehicleDetection {
  id: number;
  cameraId: number;
  cameraName: string | null;
  plateNumber: string | null;
  vehicleType: string | null;
  imageFileName: string | null;
  detectedAt: string;
  confidenceLevel: number;
  xmlFileName: string | null;
  uuid: string | null;
  direction: string | null;
  country: string | null;
}



function CameraVehicles() {
  const { cameraId: cameraIdParam } =
    useParams<{ cameraId: string }>();

  const cameraId = Number(cameraIdParam);
  
  const cameraTitle = `Camera ${cameraId}`;
  const [vehicles, setVehicles] = useState<VehicleDetection[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [startDate, setStartDate] = useState("");
const [startTime, setStartTime] = useState("");

const [endDate, setEndDate] = useState("");
const [endTime, setEndTime] = useState("");
  const fetchVehicles = async () => {
    try {
      setError("");

      const response = await fetch(
        `${API_URL}/api/VehicleDetection/camera/${cameraId}`
      );

      if (!response.ok) {
        throw new Error("Unable to get vehicle detections");
      }

      const result = await response.json();

      /*
       * Backend response:
       *
       * {
       *   success: true,
       *   cameraId: 1,
       *   count: 10,
       *   data: [...]
       * }
       */

      const data: VehicleDetection[] = Array.isArray(result)
        ? result
        : result.data ?? [];

      setVehicles(data);
    } catch (err) {
      console.error("Vehicle API error:", err);

      setError(
        err instanceof Error
          ? err.message
          : "Unable to connect to backend"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    setLoading(true);
    fetchVehicles();

    const interval = setInterval(() => {
      fetchVehicles();
    }, 10000);

    return () => clearInterval(interval);
  }, [cameraId]);

  const filteredVehicles = vehicles.filter((vehicle) => {
  // Plate number search
  const matchesPlate = (vehicle.plateNumber ?? "")
    .toLowerCase()
    .includes(search.toLowerCase());

  if (!matchesPlate) {
    return false;
  }

  const detectedTime = new Date(vehicle.detectedAt).getTime();

  // Start date + time
  if (startDate) {
    const startDateTime = new Date(
      `${startDate}T${startTime || "00:00"}`
    ).getTime();

    if (detectedTime < startDateTime) {
      return false;
    }
  }

  // End date + time
  if (endDate) {
    const endDateTime = new Date(
      `${endDate}T${endTime || "23:59:59"}`
    ).getTime();

    if (detectedTime > endDateTime) {
      return false;
    }
  }

  return true;
});
if (!cameraId || cameraId <= 0) {
  return (
    <div style={styles.center}>
      <h3>Invalid Camera</h3>
      <p>Invalid camera ID in URL.</p>
    </div>
  );
}
  if (loading) {
    return (
      <div style={styles.center}>
        Loading {cameraTitle} vehicle detections...
      </div>
    );
  }

  if (error) {
    return (
      <div style={styles.center}>
        <h3>Error</h3>
        <p>{error}</p>

        <button onClick={fetchVehicles}>
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div style={styles.page}>

      {/* HEADER */}

      <div style={styles.header}>
        <div>
          <h1 style={styles.title}>
            {cameraTitle} - Vehicle Detection
          </h1>

          <p style={styles.subtitle}>
            Hikvision ANPR Monitoring
          </p>
        </div>

        <button
          onClick={fetchVehicles}
          style={styles.refreshButton}
        >
          ↻ Refresh
        </button>
      </div>

      {/* CAMERA INFORMATION */}

      <div style={styles.cameraInfo}>
        <strong>Camera ID:</strong> {cameraId}
      </div>

      {/* SEARCH */}

      {/* SEARCH / DATE-TIME FILTER */}

<div style={styles.filterBox}>

  {/* Plate Search */}
  <div style={styles.filterItem}>
    <label style={styles.label}>Plate Number</label>

    <input
      type="text"
      placeholder="Search plate number..."
      value={search}
      onChange={(e) => setSearch(e.target.value)}
      style={styles.searchInput}
    />
  </div>

  {/* Start Date */}
  <div style={styles.filterItem}>
    <label style={styles.label}>Start Date</label>

    <input
      type="date"
      value={startDate}
      onChange={(e) => setStartDate(e.target.value)}
      style={styles.dateInput}
    />
  </div>

  {/* Start Time */}
  <div style={styles.filterItem}>
    <label style={styles.label}>Start Time</label>

    <input
      type="time"
      value={startTime}
      onChange={(e) => setStartTime(e.target.value)}
      style={styles.dateInput}
    />
  </div>

  {/* End Date */}
  <div style={styles.filterItem}>
    <label style={styles.label}>End Date</label>

    <input
      type="date"
      value={endDate}
      onChange={(e) => setEndDate(e.target.value)}
      style={styles.dateInput}
    />
  </div>

  {/* End Time */}
  <div style={styles.filterItem}>
    <label style={styles.label}>End Time</label>

    <input
      type="time"
      value={endTime}
      onChange={(e) => setEndTime(e.target.value)}
      style={styles.dateInput}
    />
  </div>

  {/* Clear */}
  <button
    onClick={() => {
      setSearch("");
      setStartDate("");
      setStartTime("");
      setEndDate("");
      setEndTime("");
    }}
    style={styles.clearButton}
  >
    Clear
  </button>

</div>

      {/* COUNT */}

      <div style={styles.count}>
        {cameraTitle} vehicles detected:{" "}
        <strong>{filteredVehicles.length}</strong>
      </div>

      {/* VEHICLES */}

      {filteredVehicles.length === 0 ? (
        <div style={styles.empty}>
          No vehicle detections found for {cameraTitle}.
        </div>
      ) : (
        <div style={styles.grid}>

          {filteredVehicles.map((vehicle) => (
            <div
              key={vehicle.id}
              style={styles.card}
            >

              {/* IMAGE */}

              <div style={styles.imageContainer}>

                {/* FULL VEHICLE IMAGE */}

                <img
                  src={`${API_URL}/api/VehicleDetection/${vehicle.id}/image`}
                  alt={vehicle.plateNumber ?? "Vehicle"}
                  style={styles.image}
                />

                {/* PLATE IMAGE */}

                <div style={styles.plateContainer}>

                  <img
                    src={`${API_URL}/api/VehicleDetection/${vehicle.id}/plate-image`}
                    alt="Number plate"
                    style={styles.plateImage}
                    onError={(e) => {
                      e.currentTarget.style.display = "none";
                    }}
                  />

                  <strong style={styles.plateText}>
                    {vehicle.plateNumber ?? "NO PLATE"}
                  </strong>

                </div>
              </div>

              {/* DETAILS */}

              <div style={styles.details}>

                <div style={styles.row}>
                  <span>Vehicle Type</span>

                  <strong>
                    {vehicle.vehicleType ?? "Unknown"}
                  </strong>
                </div>

                <div style={styles.row}>
                  <span>Confidence</span>

                  <strong>
                    {vehicle.confidenceLevel}%
                  </strong>
                </div>

                <div style={styles.row}>
                  <span>Camera</span>

                  <strong>
                    {vehicle.cameraName ?? "Unknown"}
                  </strong>
                </div>

                <div style={styles.row}>
                  <span>Camera ID</span>

                  <strong>
                    {vehicle.cameraId}
                  </strong>
                </div>

                <div style={styles.row}>
                  <span>Country</span>

                  <strong>
                    {vehicle.country ?? "Unknown"}
                  </strong>
                </div>

                <div style={styles.row}>
                  <span>Direction</span>

                  <strong>
                    {vehicle.direction ?? "Unknown"}
                  </strong>
                </div>

                <div style={styles.time}>
                  {new Date(
                    vehicle.detectedAt
                  ).toLocaleString()}
                </div>

              </div>
            </div>
          ))}

        </div>
      )}
    </div>
  );
}

const styles: {
  [key: string]: React.CSSProperties;
} = {

  page: {
    minHeight: "100vh",
    padding: "30px",
    backgroundColor: "#f5f6f8",
    fontFamily: "Arial, sans-serif",
  },

  center: {
    minHeight: "100vh",
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    alignItems: "center",
  },

  header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: "20px",
  },

  title: {
    margin: 0,
    fontSize: "28px",
  },

  subtitle: {
    marginTop: "6px",
    color: "#666",
  },

  refreshButton: {
    padding: "10px 18px",
    border: "none",
    borderRadius: "8px",
    cursor: "pointer",
    backgroundColor: "#1976d2",
    color: "white",
  },

  cameraInfo: {
    display: "inline-block",
    marginBottom: "15px",
    padding: "8px 14px",
    borderRadius: "6px",
    backgroundColor: "#e8f1fb",
    color: "#1976d2",
  },

  searchBox: {
    marginBottom: "15px",
  },

  searchInput: {
    width: "400px",
    maxWidth: "100%",
    padding: "12px",
    border: "1px solid #ddd",
    borderRadius: "8px",
    fontSize: "15px",
  },

  count: {
    marginBottom: "20px",
    color: "#555",
  },

  grid: {
    display: "grid",
    gridTemplateColumns:
      "repeat(auto-fill, minmax(320px, 1fr))",
    gap: "20px",
  },

  card: {
    backgroundColor: "white",
    borderRadius: "12px",
    overflow: "hidden",
    boxShadow:
      "0 2px 10px rgba(0,0,0,0.08)",
  },

  imageContainer: {
    width: "100%",
    overflow: "hidden",
    background: "#f5f5f5",
  },

  image: {
    width: "100%",
    height: "175px",
    objectFit: "cover",
    display: "block",
  },

  plateContainer: {
    display: "flex",
    alignItems: "center",
    gap: "12px",
    padding: "10px 12px",
    background: "#f8f8f8",
    borderTop: "1px solid #ddd",
  },

  plateImage: {
    width: "90px",
    height: "40px",
    objectFit: "contain",
    border: "1px solid #ccc",
    borderRadius: "4px",
    background: "white",
  },

  plateText: {
    fontSize: "18px",
    letterSpacing: "1px",
  },

  details: {
    padding: "18px",
  },

  row: {
    display: "flex",
    justifyContent: "space-between",
    padding: "8px 0",
    borderBottom: "1px solid #eee",
  },

  time: {
    marginTop: "15px",
    color: "#777",
    fontSize: "13px",
  },

  empty: {
    textAlign: "center",
    padding: "50px",
    color: "#777",
  },
  filterBox: {
  display: "flex",
  flexWrap: "wrap",
  alignItems: "flex-end",
  gap: "12px",
  marginBottom: "20px",
  padding: "18px",
  backgroundColor: "white",
  borderRadius: "10px",
  boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
},

filterItem: {
  display: "flex",
  flexDirection: "column",
  gap: "6px",
},

label: {
  fontSize: "13px",
  fontWeight: "600",
  color: "#555",
},

dateInput: {
  padding: "11px",
  border: "1px solid #ddd",
  borderRadius: "8px",
  fontSize: "14px",
  minWidth: "150px",
},

clearButton: {
  padding: "11px 18px",
  border: "none",
  borderRadius: "8px",
  cursor: "pointer",
  backgroundColor: "#777",
  color: "white",
  fontSize: "14px",
},
};





export default CameraVehicles;