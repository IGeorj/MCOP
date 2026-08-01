import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import React from "react";
import { Navbar } from "./components/Navbar";
import Leaderboard from './components/Leaderboard';
import WelcomePage from './components/WelcomePage';
import DiscordGuildList from './components/DiscordGuildList';
import OAuthCallbackHandler from './components/OAuthCallbackHandler';
import { I18nextProvider } from 'react-i18next';
import i18n from './i18n';
import { useAuth } from "./hooks/useAuth";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SlideShow } from "./components/Slideshow/Slideshow";
import { GuildSettings } from "./components/GuildSettings/GuildSettings";
import { GuildListProvider } from "./contexts/GuildListContext";
import useTheme from "@/hooks/useTheme";
import { GuildLayout } from "./components/GuildSettings/GuildSettingsLayout";
import VideoBrowser from './components/Video/VideoBrowser';
import { ApiReferenceReact } from '@scalar/api-reference-react';
import { config } from "./config";

const queryClient = new QueryClient();

const FullScreenLayout = ({ children }: { children: React.ReactNode }) => (
    <div className="fixed inset-0 overflow-hidden bg-black">
        {children}
    </div>
);

function AuthAppContent() {
    const navigate = useNavigate();

    const handleLogoutNavigation = () => {
        navigate("/", { replace: true });
    };

    useTheme();

    const {
        isAuthenticated,
        user,
        handleDiscordLogin,
        handleLogout,
        handleAuthResult,
    } = useAuth(handleLogoutNavigation);

    return (
        <I18nextProvider i18n={i18n}>
            <GuildListProvider>
                <div className="h-screen flex flex-col transition-all">
                    <Routes>
                        <Route path="/" element={
                            <div id="app-content" className="h-screen flex flex-col transition-all">
                                <Navbar
                                    isLoggedIn={isAuthenticated}
                                    username={user?.username}
                                    avatarUrl={user?.avatarUrl}
                                    onLogin={handleDiscordLogin}
                                    onLogout={handleLogout}
                                />
                                {!isAuthenticated ? (
                                    <WelcomePage onLogin={handleDiscordLogin} />
                                ) : (
                                    <main className="container mx-auto px-3 py-7">
                                        <DiscordGuildList />
                                    </main>
                                )}
                            </div>
                        } />
                        <Route path="/leaderboard/:guildId" element={
                            <div id="app-content" className="h-screen flex flex-col transition-all">
                                <Navbar
                                    isLoggedIn={isAuthenticated}
                                    username={user?.username}
                                    avatarUrl={user?.avatarUrl}
                                    onLogin={handleDiscordLogin}
                                    onLogout={handleLogout}
                                />
                                <main className="flex-1 flex flex-col container mx-auto px-3 py-7">
                                    <Leaderboard />
                                </main>
                            </div>
                        } />
                        <Route path="/guilds/:guildId" element={
                            <div id="app-content" className="h-screen flex flex-col transition-all">
                                <GuildLayout />
                            </div>
                        }>
                            <Route index element={
                                <div id="app-content" className="h-screen flex flex-col transition-all">
                                    <Navigate to="leveling" replace />
                                </div>
                            } />
                            <Route path=":category" element={
                                <div id="app-content" className="h-screen flex flex-col transition-all">
                                    <GuildSettings />
                                </div>
                            } />
                        </Route>
                        <Route path="/slideshow" element={
                            <div id="app-content" className="h-screen flex flex-col transition-all">
                                <FullScreenLayout>
                                    <SlideShow />
                                </FullScreenLayout>
                            </div>
                        } />
                        <Route path="/oauth/callback" element={
                            <div id="app-content" className="h-screen flex flex-col transition-all">
                                <OAuthCallbackHandler onAuth={handleAuthResult} />
                            </div>
                        }
                        />
                        <Route path="/videos" element={
                            <div id="app-content" className="h-screen flex flex-col transition-all">
                                <FullScreenLayout>
                                    <VideoBrowser />
                                </FullScreenLayout>
                            </div>
                        } />
                        <Route path="/api-v1" element={
                            <ApiReferenceReact
                                configuration={{
                                    url: `${config.API_URL}/openapi/v1.json`
                                }}
                            />
                        } />
                        <Route path="*" element={<Navigate to="/" replace />} />
                    </Routes>
                </div>
            </GuildListProvider>
        </I18nextProvider>
    );

}

function AuthApp() {
    return (
        <QueryClientProvider client={queryClient}>
            <Router>
                <AuthAppContent />
            </Router>
        </QueryClientProvider>
    );
}

export default function App() {
    return (
        <AuthApp />
    );
}