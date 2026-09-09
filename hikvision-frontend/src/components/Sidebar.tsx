import React from "react";
import { NavLink, useNavigate } from "react-router-dom";

import {
  LayoutDashboard,
  Camera,
  Car,
  ShieldCheck,
  Radio,
  BarChart3,
  Settings,
  LogOut,
} from "lucide-react";

import "./Sidebar.css";

interface MenuItem {
  name: string;
  path: string;
  icon: React.ElementType;
}
const menuItems: MenuItem[] = [
  {
    name: "Dashboard",
    path: "/dashboard",
    icon: LayoutDashboard,
  },
  {
    name: "Cameras",
    path: "/cameras",
    icon: Camera,
  },
  {
    name: "Camera Management",
    path: "/camera-management",
    icon: Settings,
  },
];

// const menuItems: MenuItem[] = [
//   {
//     name: "Dashboard",
//     path: "/dashboard",
//     icon: LayoutDashboard,
//   },
//   {
//     name: "Camera Management",
//     path: "/camera-management",
//     icon: Camera,
//   },
//   {
//   name: "Camera 1 Vehicles",
//   path: "/camera1/vehicle-detection",
//   icon: Car,
// },
//  {
//   name: "Camera 1 Registered",
//   path: "/camera1/registered-vehicles",
//   icon: Car,
// },
// {
//   name: "Camera 1 History",
//   path: "/camera1/vehicle-history",
//   icon: Car,
// },
// {
//   name: "Camera 2 Vehicles",
//   path: "/camera2/vehicle-detection",
//   icon: Car,
// },
   
  
 
// {
//   name: "Camera 2 Registered",
//   path: "/camera2/registered-vehicles",
//   icon: Car,
// },

// {
//   name: "Camera 2 History",
//   path: "/camera2/vehicle-history",
//   icon: Car,
// },
  
// ];

const Sidebar: React.FC = () => {
  const handleLogout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("accessToken");
  localStorage.removeItem("user");

  window.location.href = "/";
};

  return (
    <aside className="sidebar">

      {/* Logo */}
      <div className="sidebar-logo">

        <div className="logo-icon">
          <Camera size={24} />
        </div>

        <div className="logo-text">
          <h2>ANPR System</h2>
          <span>Vehicle Management</span>
        </div>

      </div>

      {/* Navigation */}
      <nav className="sidebar-navigation">

        <p className="menu-title">
          MAIN MENU
        </p>

        {menuItems.map((item) => {

          const Icon = item.icon;

          return (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `sidebar-link ${
                  isActive ? "active" : ""
                }`
              }
            >

              <Icon
                className="menu-icon"
                size={20}
              />

              <span>
                {item.name}
              </span>

            </NavLink>
          );
        })}

      </nav>

      {/* Bottom */}
      <div className="sidebar-bottom">

        <button
  className="logout-button"
  onClick={handleLogout}
>
  <LogOut size={20} />
  <span>Logout</span>
</button>

      </div>

    </aside>
  );
};

export default Sidebar;