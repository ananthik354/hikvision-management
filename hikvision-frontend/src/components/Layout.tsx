import React from "react";
import { Outlet } from "react-router-dom";

import Sidebar from "./Sidebar";
import Topbar from "./Topbar";

const Layout: React.FC = () => {
  return (
    <>
      <Sidebar />
      <Topbar />

      <main className="main-content">
        <Outlet />
      </main>
    </>
  );
};

export default Layout;