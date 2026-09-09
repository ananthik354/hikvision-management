
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
const API_URL = "http://localhost:5018";



interface CameraRegister{
  id: number;
  cameraId: number;
  vehicleType: string | null;
  plateNumber: string;
  passType: string;
  status: string;
  entryDateTime: string;
  usedAt: string | null;
}

interface VehicleForm {
  cameraId: number;
  plateNumber: string;
  vehicleType: string;
  passType: string;
}



function CameraRegister(){
  const { cameraId: cameraIdParam } =
    useParams<{ cameraId: string }>();

  const cameraId = Number(cameraIdParam);

  const cameraTitle = `Camera ${cameraId}`;
  const [vehicles, setVehicles] =
    useState<CameraRegister[]>([]);

  

  const [form, setForm] =
  useState<VehicleForm>({
    cameraId: Number(cameraIdParam),
    plateNumber: "",
    vehicleType: "",
    passType: "OneTime",
  });

  const [loading, setLoading] =
    useState(false);

  const [fetching, setFetching] =
    useState(true);

  const [message, setMessage] =
    useState("");

  const [error, setError] =
    useState("");


  // ============================================================
  // GET CAMERAS
  // ============================================================

 


  // ============================================================
  // GET REGISTERED VEHICLES
  // ============================================================

  const loadVehicles = async () => {
  try {
    setFetching(true);
    setError("");

    const response = await fetch(
  `${API_URL}/api/RegisteredVehicle/camera/${cameraId}`
);

    if (!response.ok) {
      throw new Error("Failed to load registered vehicles");
    }

    const result = await response.json();

    console.log("Registered Vehicle API response:", result);

    let vehicleList: CameraRegister[] = [];

    if (Array.isArray(result)) {
      vehicleList = result;
    } else if (Array.isArray(result.data)) {
      vehicleList = result.data;
    } else if (Array.isArray(result.result)) {
      vehicleList = result.result;
    } else if (Array.isArray(result.items)) {
      vehicleList = result.items;
    } else if (Array.isArray(result.vehicles)) {
      vehicleList = result.vehicles;
    }

    setVehicles(vehicleList);

    console.log(
      "Registered vehicles loaded:",
      vehicleList
    );
  } catch (err) {
    console.error(
      "Registered vehicle loading error:",
      err
    );

    setError(
      err instanceof Error
        ? err.message
        : "Unable to load registered vehicles"
    );
  } finally {
    setFetching(false);
  }
};

  // ============================================================
  // INITIAL LOAD
  // ============================================================

  useEffect(() => {
  setForm(prev => ({
    ...prev,
    cameraId,
  }));

  loadVehicles();
}, [cameraId]);


  // ============================================================
  // FORM CHANGE
  // ============================================================

 const handleChange = (
  e: React.ChangeEvent<
    HTMLInputElement |
    HTMLSelectElement
  >
) => {
  setForm({
    ...form,
    [e.target.name]: e.target.value,
  });
};


  // ============================================================
  // REGISTER VEHICLE
  // ============================================================

  const handleSubmit = async (
    e: React.FormEvent
  ) => {
    e.preventDefault();

    setMessage("");
    setError("");


    // Validate camera
    if (form.cameraId <= 0) {
      setError("Please select a camera");
      return;
    }


    // Validate plate
    if (!form.plateNumber.trim()) {
      setError("Plate number is required");
      return;
    }


    // Validate vehicle type
    if (!form.vehicleType) {
      setError("Please select vehicle type");
      return;
    }


    try {
      setLoading(true);


      const response = await fetch(
        `${API_URL}/api/RegisteredVehicle`,
        {
          method: "POST",

          headers: {
            "Content-Type": "application/json",
          },

          body: JSON.stringify({
  cameraId: cameraId,
  plateNumber: form.plateNumber
    .trim()
    .toUpperCase(),
  vehicleType: form.vehicleType,
  passType: form.passType
}),
        }
      );


      const result =
        await response.json();


      if (!response.ok) {
        throw new Error(
          result.message ||
          "Failed to register vehicle"
        );
      }


      setMessage(
        "Vehicle registered successfully. Status: Pending"
      );


      // Clear form
      setForm({
  cameraId: cameraId,
  plateNumber: "",
  vehicleType: "",
  passType: "OneTime",
});


      // Reload table
      await loadVehicles();

    } catch (err) {
      console.error(err);

      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError(
          "Failed to register vehicle"
        );
      }

    } finally {
      setLoading(false);
    }
  };

const downloadTablePdf = () => {
  const doc = new jsPDF("landscape");

  const generatedAt = new Date().toLocaleString();

  doc.setFontSize(20);
  doc.setFont("helvetica", "bold");

  doc.text(
    `${cameraTitle} - Registered Vehicles`,
    doc.internal.pageSize.getWidth() / 2,
    20,
    { align: "center" }
  );

  doc.setFontSize(10);
  doc.setFont("helvetica", "normal");

  doc.text(
    `Generated: ${generatedAt}`,
    14,
    30
  );

  autoTable(doc, {
    startY: 38,

    head: [[
      "Camera",
      "Plate Number",
      "Vehicle Type",
      "Pass Type",
      "Status",
      "Entry Date / Time",
      "Used At"
    ]],

    body: vehicles.map((vehicle) => [
      cameraTitle,
      vehicle.plateNumber,
      vehicle.vehicleType || "-",

      vehicle.passType === "OneTime"
        ? "One Time"
        : "All Time",

      vehicle.status,

      vehicle.entryDateTime
        ? new Date(
            vehicle.entryDateTime
          ).toLocaleString()
        : "-",

      vehicle.usedAt
        ? new Date(
            vehicle.usedAt
          ).toLocaleString()
        : "-"
    ]),

    styles: {
      fontSize: 9,
      cellPadding: 4,
      overflow: "linebreak"
    },

    headStyles: {
      fontStyle: "bold"
    },

    margin: {
      top: 38,
      left: 10,
      right: 10
    },

    didDrawPage: (data) => {
      const pageNumber =
        doc.getNumberOfPages();

      doc.setFontSize(8);

      doc.text(
        `Page ${pageNumber}`,
        doc.internal.pageSize.getWidth() - 25,
        doc.internal.pageSize.getHeight() - 10
      );
    }
  });

  doc.save(
    `${cameraTitle}_Registered_Vehicles.pdf`
  );
};


  // ============================================================
  // UI
  // ============================================================

  return (
    <div style={styles.page}>

      <h1 style={styles.title}>
  {cameraTitle} - Registered Vehicles
</h1>

<p style={styles.subtitle}>
  Register vehicles for {cameraTitle}.
</p>


      {/* ======================================================
          REGISTER FORM
      ====================================================== */}

      <div style={styles.formCard}>

        <h2 style={styles.sectionTitle}>
          Register Vehicle
        </h2>


        <form onSubmit={handleSubmit}>

          <div style={styles.formGrid}>

            {/* CAMERA */}

            <div style={styles.field}>

  <label style={styles.label}>
    Camera / Gate
  </label>

  <input
    value={cameraTitle}
    readOnly
    style={styles.input}
  />

</div>


            {/* PLATE NUMBER */}

            <div style={styles.field}>

              <label style={styles.label}>
                Plate Number *
              </label>

              <input
                name="plateNumber"
                value={form.plateNumber}
                onChange={handleChange}
                placeholder="Example: TN38AB1234"
                style={styles.input}
              />

            </div>


            {/* VEHICLE TYPE */}

            <div style={styles.field}>

              <label style={styles.label}>
                Vehicle Type *
              </label>

              <select
                name="vehicleType"
                value={form.vehicleType}
                onChange={handleChange}
                style={styles.input}
              >

                <option value="">
                  Select vehicle type
                </option>

                <option value="Car">
                  Car
                </option>

                <option value="SUV">
                  SUV
                </option>

                <option value="MPV">
                  MPV
                </option>

                <option value="Truck">
                  Truck
                </option>

                <option value="Bus">
                  Bus
                </option>

                <option value="Motorcycle">
                  Motorcycle
                </option>

              </select>

            </div>


            {/* PASS TYPE */}

            <div style={styles.field}>

              <label style={styles.label}>
                Pass Type *
              </label>

              <select
                name="passType"
                value={form.passType}
                onChange={handleChange}
                style={styles.input}
              >

                <option value="OneTime">
                  One Time
                </option>

                <option value="AllTime">
                  All Time
                </option>

              </select>

            </div>

          </div>


          {/* MESSAGES */}

          {message && (
            <div style={styles.success}>
              {message}
            </div>
          )}

          {error && (
            <div style={styles.error}>
              {error}
            </div>
          )}


          <button
            type="submit"
            disabled={loading}
            style={styles.button}
          >
            {loading
              ? "Registering..."
              : "Register Vehicle"}
          </button>

        </form>

      </div>


      {/* ======================================================
          TABLE
      ====================================================== */}

      <div style={styles.tableCard}>

        <div style={styles.tableHeader}>

  <h2 style={styles.sectionTitle}>
    Registered Vehicles
  </h2>

  <div style={{ display: "flex", gap: "10px" }}>

    <button
      onClick={loadVehicles}
      style={styles.refreshButton}
    >
      ↻ Refresh
    </button>

    <button
      onClick={downloadTablePdf}
      disabled={vehicles.length === 0}
      style={styles.pdfButton}
    >
      Download PDF
    </button>

  </div>

</div>


        {fetching ? (

          <p>
            Loading vehicles...
          </p>

        ) : vehicles.length === 0 ? (

          <div style={styles.empty}>
            No registered vehicles found.
          </div>

        ) : (

          <div
            style={{
              overflowX: "auto",
            }}
          >

            <table style={styles.table}>

              <thead>

                <tr>

                  <th style={styles.th}>
                    Camera
                  </th>

                  <th style={styles.th}>
                    Plate Number
                  </th>

                  <th style={styles.th}>
                    Vehicle Type
                  </th>

                  <th style={styles.th}>
                    Pass Type
                  </th>

                  <th style={styles.th}>
                    Status
                  </th>

                  <th style={styles.th}>
                    Entry Date / Time
                  </th>

                  <th style={styles.th}>
                    Used At
                  </th>

                </tr>

              </thead>


              <tbody>

                {vehicles.map(vehicle => (

                  <tr key={vehicle.id}>

                    <td style={styles.td}>
  {cameraTitle}
</td>


                    <td style={styles.td}>

                      <strong>
                        {vehicle.plateNumber}
                      </strong>

                    </td>


                    <td style={styles.td}>
                      {vehicle.vehicleType || "-"}
                    </td>


                    <td style={styles.td}>
                      {vehicle.passType ===
                      "OneTime"
                        ? "One Time"
                        : "All Time"}
                    </td>


                    <td style={styles.td}>

                      <span
                        style={{
                          ...styles.status,

                          background:
                            vehicle.status ===
                            "Active"
                              ? "#dcfce7"
                              : vehicle.status ===
                                "Pending"
                              ? "#fef3c7"
                              : "#fee2e2",

                          color:
                            vehicle.status ===
                            "Active"
                              ? "#166534"
                              : vehicle.status ===
                                "Pending"
                              ? "#92400e"
                              : "#991b1b",
                        }}
                      >
                        {vehicle.status}
                      </span>

                    </td>


                    <td style={styles.td}>

                      {new Date(
                        vehicle.entryDateTime
                      ).toLocaleString()}

                    </td>


                    <td style={styles.td}>

                      {vehicle.usedAt
                        ? new Date(
                            vehicle.usedAt
                          ).toLocaleString()
                        : "-"}

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


const styles: {
  [key: string]: React.CSSProperties;
} = {

  page: {
    minHeight: "100vh",
    background: "#f5f6f8",
    padding: "30px",
  },

  title: {
    margin: 0,
    fontSize: "32px",
  },

  subtitle: {
    color: "#666",
    marginTop: "8px",
  },

  formCard: {
    background: "white",
    padding: "25px",
    borderRadius: "12px",
    marginTop: "25px",
    boxShadow:
      "0 2px 10px rgba(0,0,0,0.08)",
  },

  tableCard: {
    background: "white",
    padding: "25px",
    borderRadius: "12px",
    marginTop: "30px",
    boxShadow:
      "0 2px 10px rgba(0,0,0,0.08)",
  },

  sectionTitle: {
    marginTop: 0,
    marginBottom: "20px",
  },

  formGrid: {
    display: "grid",
    gridTemplateColumns:
      "repeat(auto-fit, minmax(250px, 1fr))",
    gap: "18px",
  },

  field: {
    display: "flex",
    flexDirection: "column",
    gap: "7px",
  },

  label: {
    fontWeight: 600,
    fontSize: "14px",
  },

  input: {
    padding: "11px",
    border: "1px solid #ccc",
    borderRadius: "7px",
    fontSize: "14px",
    boxSizing: "border-box",
    width: "100%",
  },

  button: {
    marginTop: "20px",
    padding: "12px 25px",
    border: "none",
    borderRadius: "7px",
    background: "#2563eb",
    color: "white",
    fontSize: "15px",
    fontWeight: 600,
    cursor: "pointer",
  },

  refreshButton: {
    padding: "9px 16px",
    border: "1px solid #ccc",
    borderRadius: "7px",
    background: "white",
    cursor: "pointer",
  },

  tableHeader: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
  },

  table: {
    width: "100%",
    borderCollapse: "collapse",
  },

  th: {
    textAlign: "left",
    padding: "12px",
    borderBottom: "2px solid #ddd",
    whiteSpace: "nowrap",
  },

  td: {
    padding: "12px",
    borderBottom: "1px solid #eee",
    whiteSpace: "nowrap",
  },

  status: {
    padding: "5px 10px",
    borderRadius: "20px",
    fontSize: "12px",
    fontWeight: 600,
  },

  success: {
    marginTop: "15px",
    padding: "10px",
    background: "#dcfce7",
    color: "#166534",
    borderRadius: "6px",
  },

  error: {
    marginTop: "15px",
    padding: "10px",
    background: "#fee2e2",
    color: "#991b1b",
    borderRadius: "6px",
  },

  empty: {
    padding: "30px",
    textAlign: "center",
    color: "#777",
  },
  pdfButton: {
  padding: "9px 16px",
  border: "none",
  borderRadius: "7px",
  background: "#dc2626",
  color: "white",
  cursor: "pointer",
  fontWeight: 600,
},
};



export default CameraRegister;