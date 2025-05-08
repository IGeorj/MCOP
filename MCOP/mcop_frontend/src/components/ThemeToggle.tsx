import React from "react";
import useTheme from "../hooks/useTheme";
import { RiMoonFill } from "react-icons/ri";
import { AiOutlineSun } from "react-icons/ai";

const ThemeToggle: React.FC = () => {
  const { theme, toggleTheme } = useTheme();
  const isDark = theme === "dark";

  return (
    <button
      type="button"
      aria-label="Toggle theme"
      title="Toggle theme"
      onClick={toggleTheme}
      className={`
        flex items-center w-14 h-7 p-1 rounded-full
        bg-secondary hover:opacity-80
        relative shadow-inner cursor-pointer
      `}
    >
      <span className="text-yellow-400">
        <AiOutlineSun />
      </span>
      <span className="ml-auto text-gray-500 dark:text-yellow-300">
        <RiMoonFill />
      </span>
      <span
        className={`
          absolute top-1.1
          ${isDark ? "right-1" : "left-1"}
          w-5 h-5 rounded-full
          bg-white dark:bg-gray-900
          shadow 
          transition-all duration-300
          border border-gray-300 dark:border-gray-600
        `}
        style={{
          transitionProperty: "left, right",
        }}
      />
    </button>
  );
};

export default ThemeToggle;
