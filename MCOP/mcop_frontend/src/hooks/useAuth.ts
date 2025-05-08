import { useState, useCallback, useEffect } from "react";
import { config } from "../config";
import { getDiscordAuthUrl } from "../utils/discordApi";

export interface IUser {
  id: string;
  username: string;
  avatarUrl: string;
}

export function useAuth() {
  const [appSession, setAppSession] = useState<string | null>(() => {
    return window.localStorage.getItem('app_session') ?? null;
  });

  const [user, setUser] = useState<IUser | null>(null);

  const handleDiscordLogin = useCallback(() => {
    window.location.href = getDiscordAuthUrl();
  }, []);

  const handleLogout = useCallback(() => {
    setAppSession(null);
    setUser(null);
    window.localStorage.removeItem("app_session");
  }, []);

  const handleAuthResult = useCallback((data: any) => {
    if (!data || !data.session) {
      setAppSession(null);
      setUser(null);
      window.localStorage.removeItem("app_session");
      return;
    }
    setAppSession(data.session);
    setUser({ id: data.id, username: data.username, avatarUrl: data.avatarUrl });
  }, []);

  useEffect(() => {
    if (!appSession) {
      setUser(null);
      return;
    }
    if (!user && appSession) {
      fetch(config.API_URL + "/auth/me", {
        headers: {
          "Authorization": `Bearer ${appSession}`
        }
      })
      .then(async (res) => {
        if (!res.ok) throw new Error("Session invalid");
        const data = await res.json();
        setUser({
          id: data.id,
          username: data.username,
          avatarUrl: data.avatarUrl,
        });
      })
      .catch(e => {
        console.error("Failed to validate session:", e);
        handleLogout();
      });
    }
  }, [appSession, user, handleLogout]);

  return {
    appSession,
    user,
    handleDiscordLogin,
    handleLogout,
    handleAuthResult
  };
}