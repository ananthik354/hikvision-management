import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

const API_URL = "http://localhost:5018";

interface VehicleHistoryItem {
  id: number;

  plateNumber: string | null;
  vehicleType: string | null;

  detectedAt: string;

  matchStatus: string | null;

  registeredVehicleId: number | null;

  registeredVehicleName: string | null;
  ownerName: string | null;
  ownerPhone: string | null;

  vehicleDetectionId: number | null;
}



function CameraHistory(){
     const { cameraId: cameraIdParam } =
    useParams<{ cameraId: string }>();

  const cameraId = Number(cameraIdParam);

  const cameraTitle = `Camera ${cameraId}`;
     const [history, setHistory] = useState<VehicleHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadHistory = async () => {
    try {
      setLoading(true);
      setError("");

      const response = await fetch(
  `${API_URL}/api/VehicleHistory/camera/${cameraId}`
);

      if (!response.ok) {
        throw new Error("Failed to load vehicle history");
      }

      const result = await response.json();

      /*
       * Handles both:
       *
       * [
       *   {...}
       * ]
       *
       * and
       *
       * {
       *   success: true,
       *   data: [...]
       * }
       */

      if (Array.isArray(result)) {
        setHistory(result);
      } else if (Array.isArray(result.data)) {
        setHistory(result.data);
      } else {
        setHistory([]);
      }

    } catch (err) {
      console.error("Vehicle history error:", err);
      setError("Unable to load vehicle history");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
  loadHistory();

  const interval = setInterval(() => {
    loadHistory();
  }, 10000);

  return () => clearInterval(interval);
}, [cameraId]);

  if (loading) {
    return (
      <div style={styles.container}>
        <h2>Loading vehicle history...</h2>
      </div>
    );
  }

  if (error) {
    return (
      <div style={styles.container}>
        <h2>{error}</h2>

        <button
          onClick={loadHistory}
          style={styles.retryButton}
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div style={styles.container}>

      {/* HEADER */}

      <div style={styles.header}>
        <div>
          <h1 style={styles.title}>
  {cameraTitle} - Vehicle History
</h1>

<p style={styles.subtitle}>
  Vehicles matched against registered vehicles
</p>
        </div>

        <button
          onClick={loadHistory}
          style={styles.refreshButton}
        >
          🔄 Refresh
        </button>
      </div>


      {/* COUNT */}

      <div style={styles.countCard}>
        <div>
          <p style={styles.countLabel}>
            Total Matched Vehicles
          </p>

          <h2 style={styles.count}>
            {history.length}
          </h2>
        </div>

        <div style={styles.countIcon}>
          🚨
        </div>
      </div>


      {/* HISTORY */}

      <div style={styles.tableContainer}>

        <h2 style={styles.sectionTitle}>
          Detection History
        </h2>

        {history.length === 0 ? (

          <div style={styles.empty}>
            No matched vehicles found.
          </div>

        ) : (

          <div style={{ overflowX: "auto" }}>

            <table style={styles.table}>

              <thead>

                <tr>

                  <th style={styles.th}>
                    #
                  </th>

                  <th style={styles.th}>
                    Plate Number
                  </th>

                  <th style={styles.th}>
                    Vehicle
                  </th>

                  <th style={styles.th}>
                    Registered Vehicle
                  </th>

                  <th style={styles.th}>
                    Owner
                  </th>

                  <th style={styles.th}>
                    Phone
                  </th>

                  <th style={styles.th}>
                    Status
                  </th>

                  <th style={styles.th}>
                    Detected At
                  </th>

                  <th style={styles.th}>
                    Image
                  </th>

                </tr>

              </thead>


              <tbody>

                {history.map((item, index) => (

                  <tr
                    key={item.id}
                    style={styles.tr}
                  >

                    {/* NUMBER */}

                    <td style={styles.td}>
                      {index + 1}
                    </td>


                    {/* PLATE */}

                    <td style={styles.td}>

                      <strong
                        style={styles.plate}
                      >
                        {item.plateNumber ||
                          "NO PLATE"}
                      </strong>

                    </td>


                    {/* VEHICLE */}

                    <td style={styles.td}>
                      {item.vehicleType ||
                        "Unknown"}
                    </td>


                    {/* REGISTERED VEHICLE */}

                    <td style={styles.td}>

                      {item.registeredVehicleName ||
                        "Unknown"}

                    </td>


                    {/* OWNER */}

                    <td style={styles.td}>

                      {item.ownerName ||
                        "Unknown"}

                    </td>


                    {/* PHONE */}

                    <td style={styles.td}>

                      {item.ownerPhone ||
                        "N/A"}

                    </td>


                    {/* STATUS */}

                    <td style={styles.td}>

                      <span
                        style={
                          item.matchStatus
                            ?.toLowerCase() ===
                          "matched"
                            ? styles.matched
                            : styles.normal
                        }
                      >
                        {item.matchStatus ||
                          "Matched"}
                      </span>

                    </td>


                    {/* DATE */}

                    <td style={styles.td}>

                      {new Date(
                        item.detectedAt
                      ).toLocaleString()}

                    </td>


                    {/* IMAGE */}

                    <td style={styles.td}>

                      {item.vehicleDetectionId ? (

                        <img
                          src={`${API_URL}/api/VehicleDetection/${item.vehicleDetectionId}/image`}
                          alt="Vehicle"
                          style={styles.image}
                        />

                      ) : (

                        <span>
                          No Image
                        </span>

                      )}

                    </td>

                  </tr>

                ))}

              </tbody>

            </table>

          </div>

        )}

      </div>

    </div>
  );
}


/* =========================
   STYLES
========================= */

const styles: {
  [key: string]: React.CSSProperties;
} = {

  container: {
    padding: "30px",
    background: "#f5f6f8",
    minHeight: "100vh",
  },

  header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: "25px",
  },

  title: {
    margin: 0,
    fontSize: "30px",
  },

  subtitle: {
    marginTop: "6px",
    color: "#777",
  },

  refreshButton: {
    padding: "10px 18px",
    border: "none",
    borderRadius: "8px",
    cursor: "pointer",
    background: "#222",
    color: "white",
    fontSize: "14px",
  },

  retryButton: {
    padding: "10px 20px",
    border: "none",
    borderRadius: "8px",
    cursor: "pointer",
  },

  countCard: {
    background: "white",
    padding: "22px",
    borderRadius: "12px",
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    maxWidth: "300px",
    boxShadow: "0 2px 10px rgba(0,0,0,0.08)",
    marginBottom: "30px",
  },

  countLabel: {
    margin: 0,
    color: "#777",
    fontSize: "14px",
  },

  count: {
    margin: "5px 0 0",
    fontSize: "30px",
  },

  countIcon: {
    fontSize: "32px",
  },

  tableContainer: {
    background: "white",
    padding: "25px",
    borderRadius: "12px",
    boxShadow: "0 2px 10px rgba(0,0,0,0.06)",
  },

  sectionTitle: {
    marginTop: 0,
    marginBottom: "20px",
  },

  table: {
    width: "100%",
    borderCollapse: "collapse",
  },

  th: {
    textAlign: "left",
    padding: "14px",
    borderBottom: "2px solid #ddd",
    whiteSpace: "nowrap",
    fontSize: "14px",
  },

  td: {
    padding: "14px",
    borderBottom: "1px solid #eee",
    whiteSpace: "nowrap",
  },

  tr: {
    height: "70px",
  },

  plate: {
    fontSize: "16px",
    letterSpacing: "1px",
  },

  matched: {
    display: "inline-block",
    padding: "6px 12px",
    borderRadius: "20px",
    background: "#ffe1e1",
    color: "#d00000",
    fontWeight: "bold",
    fontSize: "12px",
  },

  normal: {
    display: "inline-block",
    padding: "6px 12px",
    borderRadius: "20px",
    background: "#e7f7e7",
    color: "#168316",
    fontWeight: "bold",
    fontSize: "12px",
  },

  image: {
    width: "100px",
    height: "60px",
    objectFit: "cover",
    borderRadius: "6px",
  },

  empty: {
    padding: "50px",
    textAlign: "center",
    color: "#777",
  },
};



export default CameraHistory;