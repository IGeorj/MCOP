export const config = {
    API_URL: import.meta.env.MODE === 'development'
        ? import.meta.env.VITE_API_URL_DEV
        : import.meta.env.VITE_API_URL_PROD,
    DISCORD_REDIRECT_URI: import.meta.env.MODE === 'development'
        ? import.meta.env.VITE_DISCORD_REDIRECT_URI_DEV
        : import.meta.env.VITE_DISCORD_REDIRECT_URI_PROD,
    CLIENT_ID: import.meta.env.MODE === 'development'
        ? import.meta.env.VITE_CLIENT_ID_DEV
        : import.meta.env.VITE_CLIENT_ID_PROD
};
