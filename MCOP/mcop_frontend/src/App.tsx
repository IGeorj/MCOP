import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import React from "react";
import Navbar from "./components/Navbar";
import Leaderboard from './components/Leaderboard';
import WelcomePage from './components/WelcomePage';
import DiscordGuildList from './components/DiscordGuildList';
import OAuthCallbackHandler from './components/OAuthCallbackHandler';
import { I18nextProvider } from 'react-i18next';
import i18n from './i18n';
import { useAuth } from "./hooks/useAuth";

export default function App() {
  const {
    appSession,
    user,
    handleDiscordLogin,
    handleLogout,
    handleAuthResult,
  } = useAuth();

  return (
    <I18nextProvider i18n={i18n}>
      <Router>
        <div className="min-h-screen transition-all">
          <Navbar
            isLoggedIn={!!appSession}
            username={user?.username}
            avatarUrl={user?.avatarUrl}
            onLogin={handleDiscordLogin}
            onLogout={handleLogout}
          />
          <main className="container mx-auto px-3 py-7">
            <Routes>
              <Route path="/leaderboard/:guildId" element={<Leaderboard />} />
              <Route
                path="/oauth/callback"
                element={
                  <OAuthCallbackHandler onAuth={handleAuthResult} />
                }
              />
              <Route path="/" element={
                !appSession ?
                  <WelcomePage onLogin={handleDiscordLogin} />
                  :
                  <DiscordGuildList />
              } />
            </Routes>
          </main>
        </div>
      </Router>
    </I18nextProvider>
  );
}
