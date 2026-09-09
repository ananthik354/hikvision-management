// import React from "react";
// import {
//   BrowserRouter,
//   Routes,
//   Route,
// } from "react-router-dom";

// import Login from "./pages/Login";
// import Register from "./pages/Register";
// import Dashboard from "./pages/Dashboard";
// // import VehicleDetection from "./pages/VehicleDetection";
// // import RegisteredVehicles from "./pages/RegisteredVehicles";
// // import RegisteredVehicle from "./pages/RegisteredVehicle";
// // import VehicleHistory from "./pages/VehicleHistory";
// import Layout from "./components/Layout";
// import CameraManagement from "./pages/CameraManagement";
// import "./App.css";

// import Camera1VehicleDetection from "./pages/Camera1VehicleDetection";
// import Camera2VehicleDetection from "./pages/Camera2VehicleDetection";

// import Camera1RegisteredVehicle from "./pages/Camera1RegisteredVehicle";
// import Camera2RegisteredVehicle from "./pages/Camera2RegisteredVehicle";

// import Camera1VehicleHistory from "./pages/Camera1VehicleHistory";
// import Camera2VehicleHistory from "./pages/Camera2VehicleHistory";

// import Cameras from "./pages/Cameras";

// import CameraLayout from "./pages/camera/CameraLayout";
// import CameraDashboard from "./pages/camera/CameraDashboard";
// import CameraVehicles from "./pages/camera/CameraVehicles";
// import CameraRegister from "./pages/camera/CameraRegister";
// import CameraHistory from "./pages/camera/CameraHistory";
// function App() {
//   return (
//     <BrowserRouter>

//       <Routes>

//         {/* =========================
//             AUTH PAGES
//             No Sidebar / Topbar
//         ========================= */}

//         <Route  path="/"  element={<Login />}/>

//         <Route  path="/register"  element={<Register />} />


//         {/* =========================
//             APPLICATION PAGES
//             Sidebar + Topbar
//         ========================= */}

//         <Route element={<Layout />}>

//           <Route  path="/dashboard"  element={<Dashboard />}  />

//           {/* <Route  path="/vehicle-detection"  element={<VehicleDetection />}  /> */}

//           {/* <Route  path="/registered-vehicles"  element={<RegisteredVehicles />}  /> */}

//           {/* <Route  path="/registered-vehicle"  element={<RegisteredVehicle />}  />

//           <Route  path="/vehicle-history"  element={<VehicleHistory />}  /> */}
//           <Route  path="/camera-management"  element={<CameraManagement />}/>
// <Route
//   path="/camera1/vehicle-detection"
//   element={<Camera1VehicleDetection />}
// />

// <Route
//   path="/camera2/vehicle-detection"
//   element={<Camera2VehicleDetection />}
// />

// <Route
//   path="/camera1/vehicle-detection"
//   element={<Camera1VehicleDetection />}
// />

// <Route
//   path="/camera2/vehicle-detection"
//   element={<Camera2VehicleDetection />}
// />

// <Route
//   path="/camera1/registered-vehicles"
//   element={<Camera1RegisteredVehicle />}
// />

// <Route
//   path="/camera2/registered-vehicles"
//   element={<Camera2RegisteredVehicle />}
// />

// <Route
//   path="/camera1/vehicle-history"
//   element={<Camera1VehicleHistory />}
// />

// <Route
//   path="/camera2/vehicle-history"
//   element={<Camera2VehicleHistory />}
// />

//         {/* Camera list */}
//         <Route path="/cameras" element={<Cameras />} />

//         {/* Camera-specific pages */}
//         <Route path="/cameras/:cameraId" element={<CameraLayout />}>

//           <Route
//             path="dashboard"
//             element={<CameraDashboard />}
//           />

//           <Route
//             path="vehicles"
//             element={<CameraVehicles />}
//           />

//           <Route
//             path="register"
//             element={<CameraRegister />}
//           />

//           <Route
//             path="history"
//             element={<CameraHistory />}
//           />
//         </Route>

//       </Routes>

//     </BrowserRouter>
//   );
// }

// export default App;

import React from "react";
import {
  BrowserRouter,
  Routes,
  Route,
} from "react-router-dom";

import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import Layout from "./components/Layout";
import CameraManagement from "./pages/CameraManagement";
import Cameras from "./pages/Cameras";

import CameraLayout from "./pages/camera/CameraLayout";
import CameraDashboard from "./pages/camera/CameraDashboard";
import CameraVehicles from "./pages/camera/CameraVehicles";
import CameraRegister from "./pages/camera/CameraRegister";
import CameraHistory from "./pages/camera/CameraHistory";

import "./App.css";

function App() {
  return (
    <BrowserRouter>

      <Routes>

        {/* =========================
            AUTH PAGES
            No Sidebar / Topbar
        ========================= */}

        <Route
          path="/"
          element={<Login />}
        />

        <Route
          path="/register"
          element={<Register />}
        />


        {/* =========================
            APPLICATION PAGES
            Sidebar + Topbar
        ========================= */}

        <Route element={<Layout />}>

          {/* Main Dashboard */}

          <Route
            path="/dashboard"
            element={<Dashboard />}
          />


          {/* Camera Management */}

          <Route
            path="/camera-management"
            element={<CameraManagement />}
          />


          {/* =========================
              CAMERA LIST
          ========================= */}

          <Route
            path="/cameras"
            element={<Cameras />}
          />


          {/* =========================
              CAMERA-SPECIFIC PAGES
              
              Example:
              /cameras/1/dashboard
              /cameras/1/vehicles
              /cameras/1/register
              /cameras/1/history

              /cameras/2/dashboard
              /cameras/2/vehicles
              /cameras/2/register
              /cameras/2/history
          ========================= */}

          <Route
            path="/cameras/:cameraId"
            element={<CameraLayout />}
          >

            <Route
              path="dashboard"
              element={<CameraDashboard />}
            />

            <Route
              path="vehicles"
              element={<CameraVehicles />}
            />

            <Route
              path="register"
              element={<CameraRegister />}
            />

            <Route
              path="history"
              element={<CameraHistory />}
            />

          </Route>

        </Route>

      </Routes>

    </BrowserRouter>
  );
}

export default App;