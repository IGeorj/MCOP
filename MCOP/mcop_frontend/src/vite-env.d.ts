/// <reference types="vite/client" />

interface ViteTypeOptions {
    strictImportMetaEnv: unknown
}

interface ImportMetaEnv {
    readonly VITE_CLIENT_ID_DEV: string
    readonly VITE_CLIENT_ID_PROD: string
    readonly VITE_API_URL_DEV: string
    readonly VITE_API_URL_PROD: string
    readonly VITE_DISCORD_REDIRECT_URI_DEV: string
    readonly VITE_DISCORD_REDIRECT_URI_PROD: string
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}