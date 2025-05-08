import React, { useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";

const LANGUAGES: { code: string; label: string }[] = [
  { code: "en", label: "EN" },
  { code: "ru", label: "RU" },
];

const LanguageSwitcher: React.FC = () => {
  const { i18n } = useTranslation();
  const [open, setOpen] = useState(false);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    function handleClick(e: MouseEvent) {
      if (
        menuRef.current && !menuRef.current.contains(e.target as Node) &&
        buttonRef.current && !buttonRef.current.contains(e.target as Node)
      ) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, [open]);

  const currentLang = LANGUAGES.find(l => l.code === i18n.language) || LANGUAGES[0];

  const handleSelect = (code: string) => {
    i18n.changeLanguage(code);
    setOpen(false);
  };

  return (
    <div className="relative flex justify-end p-2 z-50">
      <button
        ref={buttonRef}
        className={`
          flex items-center gap-1 px-2 py-1 rounded
          bg-transparent hover:opacity-60
          text-primary border border-primary/50 font-medium shadow-sm
          transition-all cursor-pointer
        `}
        style={{
          backdropFilter: 'blur(6px)',
          WebkitBackdropFilter: 'blur(6px)'
        }}
        onClick={() => setOpen((v) => !v)}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label="Select language"
        type="button"
      >
        <span style={{fontSize: "small"}}>{currentLang.label}</span>
        <svg
          className={`ml-1 transition-transform ${open ? "rotate-180" : "rotate-0"}`}
          width="16" height="16"
          viewBox="0 0 20 20"
          fill="none"
        >
          <path d="M5.25 8.25L10 13.25L14.75 8.25" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round"/>
        </svg>
      </button>
      {open && (
        <div
          ref={menuRef}
          className={`
            absolute right-0 mt-10 min-w-full bg-navbar dark:bg-navbar/90 
            border border-primary/20 rounded shadow-lg z-50
            backdrop-blur-2xl border-none
          `}
        >
          {LANGUAGES.map((lang) => (
            <button
              key={lang.code}
              type="button"
              className={`
                block w-full text-left px-4 py-2
                hover:opacity-60 transition
                text-primary font-normal cursor-pointer
                ${i18n.language === lang.code ? "font-bold" : ""}
              `}
              style={{
                background: "none",
                border: "none",
                outline: "none",
                width: "100%"
              }}
              onClick={() => handleSelect(lang.code)}
            >
              {lang.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default LanguageSwitcher;
