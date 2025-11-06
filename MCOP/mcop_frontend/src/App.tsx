import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
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

const queryClient = new QueryClient();

const FullScreenLayout = ({ children }: { children: React.ReactNode }) => (
    <div className="fixed inset-0 overflow-hidden bg-black">
        {children}
    </div>
);

export default function App() {
    return (
        <QueryClientProvider client={queryClient}>
            <AuthApp />
        </QueryClientProvider>
    );
}

export function AuthApp() {
    useTheme();

    const {
        isAuthenticated,
        user,
        handleDiscordLogin,
        handleLogout,
        handleAuthResult,
    } = useAuth();

    return (
        <QueryClientProvider client={queryClient}>
            <I18nextProvider i18n={i18n}>
                <Router>
                    <GuildListProvider>
                        <div className="h-screen flex flex-col transition-all">
                            <Routes>
                                <Route path="/" element={
                                    <>
                                        <Navbar
                                            isLoggedIn={isAuthenticated}
                                            username={user?.username}
                                            avatarUrl={user?.avatarUrl}
                                            onLogin={handleDiscordLogin}
                                            onLogout={handleLogout}
                                        />
                                        <main className="container mx-auto px-3 py-7">
                                            {!isAuthenticated ? (
                                                <WelcomePage onLogin={handleDiscordLogin} />
                                            ) : (
                                                <DiscordGuildList />
                                            )}
                                        </main>
                                    </>
                                } />
                                <Route path="/leaderboard/:guildId" element={
                                    <>
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
                                    </>
                                } />
                                <Route path="/guilds/:guildId" element={<GuildLayout />}>
                                    <Route index element={<GuildSettings />} />
                                    <Route path="leveling" element={<GuildSettings activeCategory="leveling" />} />
                                    <Route path="nsfw" element={<GuildSettings activeCategory="nsfw" />} />
                                    <Route path="image" element={<GuildSettings activeCategory="image" />} />
                                </Route>
                                <Route path="/slideshow" element={
                                    <FullScreenLayout>
                                        <SlideShow />
                                    </FullScreenLayout>
                                } />
                                <Route
                                    path="/oauth/callback"
                                    element={
                                        <OAuthCallbackHandler onAuth={handleAuthResult} />
                                    }
                                />
                            </Routes>
                        </div>
                    </GuildListProvider>
                </Router>
            </I18nextProvider>
        </QueryClientProvider>
    );

}