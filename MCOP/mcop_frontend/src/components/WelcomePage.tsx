import React from "react";
import { useTranslation } from "react-i18next";
import Cube from "./Cube";
import DiscordLoginButton from "./DiscordLoginButton";

interface WelcomePageProps {
  onLogin: () => void;
}

const WelcomePage: React.FC<WelcomePageProps> = ({ onLogin }) => {
  const { t } = useTranslation();
  return (
    <div className="flex flex-col items-center gap-8">
      <section className="pt-16 pb-16">
        <Cube />
      </section>
      <h1 className="font-bold text-3xl text-primary text-center">
        {t('welcome.title')}
      </h1>
      <p className="text-center text-lg">
        {t('welcome.subtitle')}
      </p>
      <DiscordLoginButton onLogin={onLogin} text={t("welcome.login")} />
    </div>
  );
};

export default WelcomePage;