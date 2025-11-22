import React, { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { config } from "../config";
import { useTranslation } from "react-i18next";
import { AuthResponse } from "@/types/AuthResponse";

interface Props {
  onAuth: (data: AuthResponse | null) => void;
}

export default function OAuthCallbackHandler({ onAuth }: Props) {
  const { t } = useTranslation();

  const [searchParams] = useSearchParams();
  const [, setLoading] = useState(true);
  const [status, setStatus] = useState(t("auth.processing"));
  const [isError, setIsError] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const code = searchParams.get("code");
    if (!code) {
      setStatus(t("auth.noCodeFound"));
      setIsError(true);
      setLoading(false);
      return;
    }

    if (window.localStorage.getItem("oauth_code_handled") === code) {
      setStatus(t("auth.loginAlreadyProcessed"));
      setLoading(false);
      navigate("/");
      return;
    }

    (async () => {
      try {
        setStatus(t("auth.connectingToDiscord"));
        const res = await fetch(config.API_URL + "/auth/discord/callback", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ code }),
        });

        if (!res.ok) throw new Error("OAuth failed");

        setStatus(t("auth.verifyingYourAccount"));
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
          setStatus(t("auth.loginSuccessful"));
          window.history.replaceState({}, document.title, "/");
          setTimeout(() => navigate("/"), 1000);
        } else {
          throw new Error("Missing session in backend response");
        }
      } catch (e) {
        console.error(e);
        setStatus(t("auth.loginFailed"));
        setIsError(true);
        onAuth(null);
        setTimeout(() => navigate("/"), 2000);
      } finally {
        setLoading(false);
      }
    })();
  }, [searchParams, onAuth, navigate]);

  return (
    <div className="flex items-center justify-center min-h-screen bg-navbar">
      <div className="w-full max-w-md p-8 space-y-6 text-center card">
        {!isError ? (
          <div className="flex justify-center">
            <div
              className="w-16 h-16 rounded-full animate-spin"
              style={{
                border: '4px solid',
                borderColor: 'var(--color-primary)',
                borderTopColor: 'transparent'
              }}
            ></div>
          </div>
        ) : (
          <div className="flex justify-center">
            <div className="w-16 h-16 flex items-center justify-center">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                className="w-12 h-12 text-destructive"
              >
                <circle cx="12" cy="12" r="10"></circle>
                <line x1="12" y1="8" x2="12" y2="12"></line>
                <line x1="12" y1="16" x2="12.01" y2="16"></line>
              </svg>
            </div>
          </div>
        )}

        <h2 className="text-2xl font-semibold tracking-tight">
          {isError ? t("auth.authenticationFailed") : t("auth.authenticating")}
        </h2>

        <p className={isError ? "text-destructive" : "text-muted-foreground"}>
          {status}
        </p>

        {isError && (
          <button
            onClick={() => navigate("/")}
            className="flex justify-self-center items-center gap-2 p-3 rounded-md text-sm font-medium transition-colors cursor-pointer hover:opacity-75"
          >
            {t("auth.returnToHome")}
          </button>
        )}
      </div>
    </div>
  );
}