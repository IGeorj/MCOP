import React from "react";
import { Outlet } from 'react-router-dom';
import { Navbar } from "../Navbar";
import { useAuth } from "@/hooks/useAuth";

export function GuildLayout() {
      const {
        isAuthenticated,
        user,
        handleDiscordLogin,
        handleLogout,
      } = useAuth();
      
  return (
    <>
      <Navbar
        isLoggedIn={isAuthenticated}
        username={user?.username}
        avatarUrl={user?.avatarUrl}
        onLogin={handleDiscordLogin}
        onLogout={handleLogout}
      />
      <div className="flex-1 min-h-0 flex flex-col">
        <Outlet />
      </div>
    </>
  );
}