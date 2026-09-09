import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const API_URL = "http://localhost:5018";

interface Vehicle {
  id: number;
  cameraId: number;
  cameraName: string | null;
  plateNumber: string | null;
  vehicleType: string | null;
  confidenceLevel: number;
  direction: string | null;
  country: string | null;
  detectedAt: string;
   fullVehicleImageUrl: string | null;
   plateImageUrl: string | null;
}

interface DashboardData {
  success: boolean;
  totalVehicles: number;
  todayVehicles: number;
  todayUniqueVehicles: number;
  last10: Vehicle[];
}
interface VehicleHistory {
  id: number;
  registeredVehicleId: number;
  vehicleDetectionId: number | null;
  plateNumber: string;
  vehicleName: string | null;
  vehicleType: string | null;
  cameraId: number;
  cameraName: string | null;
  confidenceLevel: number;
  direction: string | null;
  country: string | null;
  detectedAt: string;
  matchedAt: string;
  matchStatus: string;
  triggered: boolean;
}

function Dashboard() {
  const { cameraId } = useParams<{ cameraId: string }>();
  const [data, setData] =useState<DashboardData | null>(null);
  const [latestMatch, setLatestMatch] =useState<VehicleHistory | null>(null);
  const [loading, setLoading] = useState(true);
  const formatDateTime = (dateString: string) => {
  if (!dateString) return "Unknown";

  const date = new Date(dateString);

  if (isNaN(date.getTime())) {
    return "Invalid Date";
  }

  return date.toLocaleString("en-IN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12: true,
  });
};
  const [error, setError] =
    useState("");

  const loadDashboard = async () => {
    try {
      const response = await fetch(`${API_URL}/api/VehicleDetection/dashboard/${cameraId}`);

      if (!response.ok) {
        throw new Error("Failed to load dashboard");
      }

      const result: DashboardData =
        await response.json();

      setData(result);
    } catch (error) {
      console.error(error);
      setError("Unable to connect to backend");
    } finally {
      setLoading(false);
    }
  };
const loadLatestMatch = async () => {
  try {
    const response = await fetch(`${API_URL}/api/VehicleHistory?cameraId=${cameraId}`);

    if (!response.ok) {
      throw new Error("Failed to load vehicle history");
    }

    const result = await response.json();

    if (
      result.success &&
      result.data &&
      result.data.length > 0
    ) {
      // API should already return newest first.
      // We can still sort here to be safe.
      const sorted = [...result.data].sort(
        (a: VehicleHistory, b: VehicleHistory) =>
          new Date(b.matchedAt).getTime() -
          new Date(a.matchedAt).getTime()
      );

      setLatestMatch(sorted[0]);
    } else {
      setLatestMatch(null);
    }
  } catch (error) {
    console.error(
      "Vehicle history error:",
      error
    );
  }
};
  useEffect(() => {
  if (!cameraId) {
    return;
  }

  loadDashboard();
  loadLatestMatch();

  const interval = setInterval(() => {
    loadDashboard();
    loadLatestMatch();
  }, 10000);

  return () => clearInterval(interval);
}, [cameraId]);

  if (loading) {
    return <h2>Loading dashboard...</h2>;
  }

  if (error) {
    return <h2>{error}</h2>;
  }

  if (!data) {
    return <h2>No dashboard data</h2>;
  }

  return (
    <div
      style={{
        padding: "30px",
        background: "#f5f6f8",
        minHeight: "100vh",
      }}
    >
      <h1>Camera {cameraId} - Vehicle Dashboard</h1>

      {/* CARDS */}

      <div
        style={{
          display: "grid",
          gridTemplateColumns:
            "repeat(auto-fit, minmax(220px, 1fr))",
          gap: "20px",
          marginTop: "30px",
        }}
      >

        {/* TOTAL */}

        <div style={cardStyle}>
          <div style={iconStyle}>
            🚗
          </div>

          <div>
            <p style={labelStyle}>
              Total Vehicles
            </p>

            <h2 style={numberStyle}>
              {data.totalVehicles}
            </h2>
          </div>
        </div>

        {/* TODAY */}

        <div style={cardStyle}>
          <div style={iconStyle}>
            📅
          </div>

          <div>
            <p style={labelStyle}>
              Today's Vehicles
            </p>

            <h2 style={numberStyle}>
              {data.todayVehicles}
            </h2>
          </div>
        </div>

        {/* UNIQUE */}

        <div style={cardStyle}>
          <div style={iconStyle}>
            🔢
          </div>

          <div>
            <p style={labelStyle}>
              Unique Plates Today
            </p>

            <h2 style={numberStyle}>
              {data.todayUniqueVehicles}
            </h2>
          </div>
        </div>

        {/* LAST 10 */}

        <div style={cardStyle}>
          <div style={iconStyle}>
            🕐
          </div>

          <div>
            <p style={labelStyle}>
              Recent Detections
            </p>

            <h2 style={numberStyle}>
              {data.last10.length}
            </h2>
          </div>
        </div>

      </div>

      {/* TRIGGERED VEHICLES */}
      {latestMatch &&
 latestMatch.matchStatus === "Matched" &&
 latestMatch.triggered && (

  <div
    style={{
      marginTop: "30px",
      padding: "25px",
      borderRadius: "12px",
      background: "#fee2e2",
      border: "2px solid #dc2626",
      boxShadow:
        "0 4px 15px rgba(220,38,38,0.2)",
    }}
  >

    <div
      style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        flexWrap: "wrap",
        gap: "15px",
      }}
    >

      <div>

        <h2
          style={{
            margin: 0,
            color: "#b91c1c",
          }}
        >
          🚨 VEHICLE MATCH DETECTED
        </h2>

        <p
          style={{
            marginTop: "8px",
            color: "#7f1d1d",
          }}
        >
          Registered vehicle detected by
          Hikvision camera
        </p>

      </div>

      <div
        style={{
          background: "#dc2626",
          color: "white",
          padding: "12px 25px",
          borderRadius: "30px",
          fontWeight: "bold",
          fontSize: "18px",
        }}
      >
        🔴 TRIGGER ON
      </div>

    </div>

    <div
      style={{
        marginTop: "20px",
        display: "grid",
        gridTemplateColumns:
          "repeat(auto-fit, minmax(180px, 1fr))",
        gap: "15px",
      }}
    >

      <div>
        <strong>Plate Number</strong>
        <div style={{ fontSize: "22px" }}>
          {latestMatch.plateNumber}
        </div>
      </div>

      <div>
        <strong>Vehicle</strong>
        <div>
          {latestMatch.vehicleName || "Unknown"}
        </div>
      </div>

      <div>
        <strong>Vehicle Type</strong>
        <div>
          {latestMatch.vehicleType || "Unknown"}
        </div>
      </div>

      <div>
        <strong>Camera</strong>
        <div>
          {latestMatch.cameraName || "Unknown"}
        </div>
      </div>

      <div>
        <strong>Confidence</strong>
        <div>
          {latestMatch.confidenceLevel}%
        </div>
      </div>

      <div>
        <strong>Matched At</strong>
        <div>
          {new Date(
            latestMatch.matchedAt
          ).toLocaleString()}
        </div>
      </div>

    </div>

  </div>
)}

      {/* LAST 10 VEHICLES */}

<div
  style={{
    background: "white",
    marginTop: "30px",
    padding: "25px",
    borderRadius: "12px",
    boxShadow: "0 2px 10px rgba(0,0,0,0.06)",
  }}
>
  <h2>Last 10 Vehicle Detections</h2>

  <div
    style={{
      overflowX: "auto",
      marginTop: "20px",
    }}
  >
    <table
      style={{
        width: "100%",
        borderCollapse: "collapse",
      }}
    >
      <thead>
        <tr>
          <th style={thStyle}>Vehicle Image</th>
          <th style={thStyle}>Number Plate</th>
          <th style={thStyle}>Plate Number</th>
          <th style={thStyle}>Vehicle</th>
          <th style={thStyle}>Confidence</th>
          <th style={thStyle}>Time</th>
        </tr>
      </thead>

      <tbody>
        {data.last10.map((vehicle, index) => {
          const isLatest = index === 0;

          return (
            <tr
              key={vehicle.id}
              style={{
                background: isLatest
                  ? "#f5f9ff"
                  : "white",

                borderLeft: isLatest
                  ? "4px solid #1976d2"
                  : "4px solid transparent",
              }}
            >
              {/* VEHICLE IMAGE */}

              <td
                style={{
                  ...tdStyle,
                  padding: isLatest
                    ? "18px 12px"
                    : "10px 12px",
                }}
              >
                {vehicle.fullVehicleImageUrl ? (
                  <img
                    src={`${API_URL}${vehicle.fullVehicleImageUrl}`}
                    alt="Vehicle"
                    style={{
                      width: isLatest
                        ? "180px"
                        : "120px",

                      height: isLatest
                        ? "110px"
                        : "75px",

                      objectFit: "cover",
                      borderRadius: "8px",
                      border: "1px solid #ddd",
                      display: "block",
                    }}
                  />
                ) : (
                  <div
                    style={{
                      width: isLatest
                        ? "180px"
                        : "120px",

                      height: isLatest
                        ? "110px"
                        : "75px",

                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      background: "#f1f1f1",
                      borderRadius: "8px",
                      color: "#888",
                      fontSize: "12px",
                    }}
                  >
                    No Image
                  </div>
                )}

                {isLatest && (
                  <span
                    style={{
                      display: "inline-block",
                      marginTop: "6px",
                      fontSize: "10px",
                      padding: "3px 7px",
                      borderRadius: "8px",
                      background: "#1976d2",
                      color: "white",
                    }}
                  >
                    LATEST
                  </span>
                )}
              </td>

              {/* NUMBER PLATE IMAGE */}

              <td
                style={{
                  ...tdStyle,
                  padding: isLatest
                    ? "18px 12px"
                    : "10px 12px",
                }}
              >
                {vehicle.plateImageUrl ? (
                  <img
                    src={`${API_URL}${vehicle.plateImageUrl}`}
                    alt="Number plate"
                    style={{
                      width: isLatest
                        ? "150px"
                        : "100px",

                      height: isLatest
                        ? "75px"
                        : "55px",

                      objectFit: "contain",
                      borderRadius: "8px",
                      border: "1px solid #ddd",
                      background: "#f5f5f5",
                      display: "block",
                    }}
                  />
                ) : (
                  <div
                    style={{
                      width: isLatest
                        ? "150px"
                        : "100px",

                      height: isLatest
                        ? "75px"
                        : "55px",

                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      background: "#f1f1f1",
                      borderRadius: "8px",
                      color: "#888",
                      fontSize: "12px",
                    }}
                  >
                    No Plate
                  </div>
                )}
              </td>

              {/* PLATE NUMBER */}

              <td
                style={{
                  ...tdStyle,

                  padding: isLatest
                    ? "18px 12px"
                    : "10px 12px",

                  fontSize: isLatest
                    ? "18px"
                    : "14px",
                }}
              >
                <strong>
                  {vehicle.plateNumber || "No Plate"}
                </strong>

                {isLatest && (
                  <div
                    style={{
                      marginTop: "5px",
                      fontSize: "11px",
                      color: "#1976d2",
                    }}
                  >
                    Latest Detection
                  </div>
                )}
              </td>

              {/* VEHICLE TYPE */}

              <td
                style={{
                  ...tdStyle,

                  padding: isLatest
                    ? "18px 12px"
                    : "10px 12px",

                  fontSize: isLatest
                    ? "16px"
                    : "14px",
                }}
              >
                {vehicle.vehicleType || "Unknown"}
              </td>

              {/* CONFIDENCE */}

              <td
                style={{
                  ...tdStyle,

                  padding: isLatest
                    ? "18px 12px"
                    : "10px 12px",

                  fontSize: isLatest
                    ? "16px"
                    : "14px",
                }}
              >
                {vehicle.confidenceLevel}%
              </td>

              {/* TIME */}

              <td
                style={{
                  ...tdStyle,

                  padding: isLatest
                    ? "18px 12px"
                    : "10px 12px",

                  fontSize: isLatest
                    ? "15px"
                    : "13px",
                }}
              >
                {formatDateTime(vehicle.detectedAt)}
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  </div>
</div>

    </div>
  );
}

const cardStyle: React.CSSProperties = {
  background: "white",
  padding: "22px",
  borderRadius: "12px",
  display: "flex",
  alignItems: "center",
  gap: "18px",
  boxShadow:
    "0 2px 10px rgba(0,0,0,0.08)",
};

const iconStyle: React.CSSProperties = {
  fontSize: "32px",
};

const labelStyle: React.CSSProperties = {
  margin: 0,
  color: "#777",
  fontSize: "14px",
};

const numberStyle: React.CSSProperties = {
  margin: "5px 0 0",
  fontSize: "28px",
};

const thStyle: React.CSSProperties = {
  textAlign: "left",
  padding: "12px",
  borderBottom: "1px solid #ddd",
};

const tdStyle: React.CSSProperties = {
  padding: "12px",
  borderBottom: "1px solid #eee",
};

export default Dashboard;