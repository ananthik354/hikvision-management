import { Outlet, NavLink, useNavigate, useParams } from "react-router-dom";
import {
  LayoutDashboard,
  Car,
  UserPlus,
  History,
  ArrowLeft,
} from "lucide-react";

import "./CameraLayout.css";

const CameraLayout = () => {
  const { cameraId } = useParams<{ cameraId: string }>();
  const navigate = useNavigate();

  const menuItems = [
    {
      name: "Dashboard",
      path: `/cameras/${cameraId}/dashboard`,
      icon: LayoutDashboard,
    },
    {
      name: "Vehicle Detection",
      path: `/cameras/${cameraId}/vehicles`,
      icon: Car,
    },
    {
      name: "Registered Vehicles",
      path: `/cameras/${cameraId}/register`,
      icon: UserPlus,
    },
    {
      name: "History",
      path: `/cameras/${cameraId}/history`,
      icon: History,
    },
  ];

  return (
    <div className="camera-layout">

      <div className="camera-header">

        <div>
          <h1>Camera {cameraId}</h1>
          <span className="camera-status">
            ● Online
          </span>
        </div>

        <button
          className="back-camera-button"
          onClick={() => navigate("/cameras")}
        >
          <ArrowLeft size={18} />
          Back to Cameras
        </button>

      </div>

      <div className="camera-navigation">

        {menuItems.map((item) => {
          const Icon = item.icon;

          return (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `camera-nav-link ${isActive ? "active" : ""}`
              }
            >
              <Icon size={18} />
              <span>{item.name}</span>
            </NavLink>
          );
        })}

      </div>

      <div className="camera-content">
        <Outlet />
      </div>

    </div>
  );
};

export default CameraLayout;