import React from "react";
import {
  Menu,
  Bell,
  User,
  ChevronDown,
} from "lucide-react";

import "./Topbar.css";

interface TopbarProps {
  onMenuClick?: () => void;
}

const Topbar: React.FC<TopbarProps> = ({
  onMenuClick,
}) => {
  return (
    <header className="topbar">

      {/* Left */}
      <div className="topbar-left">

        <button
          className="menu-button"
          onClick={onMenuClick}
          aria-label="Toggle menu"
        >
          <Menu size={22} />
        </button>

        <div className="page-title">
          Dashboard
        </div>

      </div>

      {/* Right */}
      <div className="topbar-right">

        {/* Notification */}
        <button
          className="notification-button"
          aria-label="Notifications"
        >
          <Bell size={21} />

          <span className="notification-badge">
            3
          </span>
        </button>

        {/* User */}
        <div className="user-profile">

          <div className="user-avatar">
            <User size={19} />
          </div>

          <div className="user-info">
            <span className="user-name">
              Admin
            </span>

            <span className="user-role">
              Administrator
            </span>
          </div>

          <ChevronDown
            size={17}
            className="user-arrow"
          />

        </div>

      </div>

    </header>
  );
};

export default Topbar;