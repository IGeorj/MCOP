import React from "react";
import { FaDiscord } from "react-icons/fa";
import { useTranslation } from "react-i18next";

const DISCORD_BLURPLE = "rgb(88, 101, 242)";
const DISCORD_BLURPLE_DARK = "rgb(71, 82, 196)";

interface DiscordLoginButtonProps {
  onLogin: () => void;
  text?: string;
}

const DiscordLoginButton: React.FC<DiscordLoginButtonProps> = ({ onLogin, text }) => {
  const { t } = useTranslation();

  return (
    <a
      onClick={onLogin}
      className="flex items-center cursor-pointer text-white px-4 py-2 gap-2 rounded transition duration-200"
      style={{
        backgroundColor: DISCORD_BLURPLE
      }}
      onMouseEnter={e => (e.currentTarget.style.backgroundColor = DISCORD_BLURPLE_DARK)}
      onMouseLeave={e => (e.currentTarget.style.backgroundColor = DISCORD_BLURPLE)}
      aria-label="Login with Discord"
      title="Login with Discord"
    >
      {text ?? t("login")}
      <FaDiscord size={20} />
  </a>
  );
};

export default DiscordLoginButton;
