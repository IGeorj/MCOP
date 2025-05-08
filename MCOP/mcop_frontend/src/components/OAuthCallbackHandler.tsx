import React, { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { config } from "../config";

interface Props {
  onAuth: (data: any) => void;
}

export default function OAuthCallbackHandler({ onAuth }: Props) {
  const [searchParams] = useSearchParams();
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const code = searchParams.get("code");
    if (!code) {
      setLoading(false);
      return;
    }

    if (window.localStorage.getItem("oauth_code_handled") === code) {
      setLoading(false);
      navigate("/");
      return;
    }

    (async () => {
      try {
        const res = await fetch(config.API_URL + "/auth/discord/callback", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ code }),
        });
        if (!res.ok) throw new Error("OAuth failed");
        const data = await res.json();
        if (data.session) {
          window.localStorage.setItem("app_session", data.session);
          window.localStorage.setItem("oauth_code_handled", code);
          onAuth({
            session: data.session,
            id: data.id || "",
            username: data.username || "",
            avatarUrl: data.avatarUrl || "",
          });
          window.history.replaceState({}, document.title, "/");
          navigate("/");
        } else {
          throw new Error("Missing session in backend response");
        }
      } catch (e) {
        alert("OAuth login failed. Please try again.");
        onAuth(null);
        navigate("/");
      } finally {
        setLoading(false);
      }
    })();
  }, [searchParams, onAuth, navigate]);

  return <div>{loading ? "Processing login..." : "Finalizing login..."}</div>;
}