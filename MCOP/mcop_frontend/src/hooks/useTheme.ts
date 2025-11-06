import { useEffect, useState } from "react";

type Theme = "light" | "dark";
const THEME_KEY = "mcop-site-theme";

export default function useTheme() {
    const [theme, setTheme] = useState<Theme>(() => {
        const stored = localStorage.getItem(THEME_KEY);
        if (stored === "dark" || stored === "light") return stored;
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    });

    useEffect(() => {
        document.documentElement.classList.toggle("dark", theme === "dark");
        localStorage.setItem(THEME_KEY, theme);
    }, [theme]);

    const toggleTheme = () => setTheme((t) => (t === "dark" ? "light" : "dark"));

    return { theme, toggleTheme };
}