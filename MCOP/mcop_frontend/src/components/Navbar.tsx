import React, { useState, useRef, useEffect } from "react";
import { Link } from "react-router-dom";
import ThemeToggle from "./ThemeToggle";
import DiscordLoginButton from "./DiscordLoginButton";
import LanguageSwitcher from "./LanguageSwitcher";
import { useTranslation } from "react-i18next";

interface NavbarProps {
  isLoggedIn: boolean;
  username?: string;
  avatarUrl?: string;
  onLogin: () => void;
  onLogout: () => void;
}

const Navbar: React.FC<NavbarProps> = ({
  isLoggedIn,
  username,
  avatarUrl,
  onLogin,
  onLogout,
}) => {
  const { t } = useTranslation();
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!dropdownOpen) return;
    function handleClick(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setDropdownOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClick, true);
    return () => document.removeEventListener("mousedown", handleClick, true);
  }, [dropdownOpen]);

  return (
    <nav className="flex items-center justify-between px-8 py-4 bg-navbar shadow-lg">
      <div className="flex items-center gap-3">
        <Link to="/" className="flex items-center gap-3" style={{ textDecoration: "none" }}>
          <img src="/coplogo.ico" alt="Site logo" className="h-10" />
          <span className="font-bold text-2xl tracking-widest text-primary hover:opacity-80">
            MCOP
          </span>
        </Link>
      </div>
      <div className="flex items-center gap-6">
        <LanguageSwitcher />
        <ThemeToggle />
        {isLoggedIn ? (
          <div className="relative" ref={dropdownRef}>
            <button
              type="button"
              className="flex items-center gap-2 px-3 py-1 rounded transition cursor-pointer focus:outline-none"
              onClick={() => setDropdownOpen((open) => !open)}
              aria-haspopup="true"
              aria-expanded={dropdownOpen}
              tabIndex={0}
            >
              {avatarUrl && (
                <img src={avatarUrl} alt="User avatar" className="h-8 w-8 rounded-full border" />
              )}
              <span>{username}</span>
              <svg
                className={`w-4 h-4 ml-1 transition-transform ${dropdownOpen ? "rotate-180" : ""}`}
                viewBox="0 0 20 20"
                fill="currentColor"
                aria-hidden="true"
              >
                <path fillRule="evenodd" d="M5.23 7.21a.75.75 0 011.06.02L10 10.738l3.71-3.51a.75.75 0 011.04 1.08l-4.25 4.02a.75.75 0 01-1.04 0l-4.25-4.02a.75.75 0 01.02-1.06z" clipRule="evenodd" />
              </svg>
            </button>
            {dropdownOpen && (
              <div className="absolute right-0 mt-2 w-40 shadow-lg z-50 animate-fadein-fast">
                <button
                  onClick={() => { onLogout(); setDropdownOpen(false); }}
                  className="w-full text-left px-4 py-2 cursor-pointer rounded-b bg-navbar bg-hover"
                >
                  {t("logout")}
                </button>
              </div>
            )}
          </div>
        ) : (
          <DiscordLoginButton onLogin={onLogin} />
        )}
      </div>
    </nav>
  );
};

export default Navbar;
