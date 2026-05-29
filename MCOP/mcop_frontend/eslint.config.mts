import js from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import pluginReact from "eslint-plugin-react";
import pluginReactHooks from "eslint-plugin-react-hooks";
import { defineConfig } from "eslint/config";

export default defineConfig([
    // Global ignores (flat config requires explicit ignores)
    { ignores: ["dist/", "node_modules/"] },

    // Base JS/TS/JSX/TSX files — browser globals + JS recommended
    {
        files: ["**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
        plugins: { js },
        extends: ["js/recommended"],
        languageOptions: { globals: globals.browser },
    },

    // Vite/Tailwind config — needs Node globals (process.env, etc.)
    {
        files: ["vite.config.mjs", "tailwind.config.js"],
        languageOptions: { globals: { ...globals.browser, ...globals.node } },
    },

    // TypeScript recommended rules
    tseslint.configs.recommended,

    // React recommended rules
    pluginReact.configs.flat.recommended,

    // React version detection + cleanup of JSX transform era rules
    {
        settings: {
            react: { version: "detect" },
        },
        rules: {
            "react/react-in-jsx-scope": "off",
            "react/jsx-uses-react": "off",
        },
    },

    // React Hooks rules (catches missing deps, conditional hooks, etc.)
    pluginReactHooks.configs.flat.recommended,
]);
