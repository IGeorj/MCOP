import { useState } from "react";
import { FiMenu, FiX, FiAward, FiLoader } from "react-icons/fi";
import { cn } from "@/lib/utils";
import { LevelingSettings } from "./Leveling/LevelingSettings";
import { Navigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Guild } from "@/types/Guild";
import { config } from "@/config";
import { ScrollArea } from "@/components/ui/scroll-area";
import { SettingsCategory } from "@/types/SettingsCategory";
import { Sidebar } from "./Sidebar";
import { useTranslation } from "react-i18next";



export function GuildSettings() {
  const { t } = useTranslation();

  const categories: SettingsCategory[] = [
    {
      id: "leveling",
      name: t("leveling.title"),
      icon: <FiAward className="w-4 h-4" />,
      component: (guildId: string) => <LevelingSettings guildId={guildId} />,
    }
  ];

  const { guildId } = useParams();
  const [activeCategory, setActiveCategory] = useState("leveling");
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const { data: guild, isLoading, error } = useQuery<Guild>({
    queryKey: ["guild", guildId],
    queryFn: async () => {
      const res = await fetch(`${config.API_URL}/guilds/${guildId}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
        },
      });
      if (!res.ok) throw new Error("Failed to fetch guild");
      return res.json();
    },
    retry: 1,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-64px)]">
        <FiLoader className="animate-spin w-8 h-8" />
      </div>
    );
  }

  if (error || !guild) {
    return <Navigate to="/guilds" replace />;
  }

  return (
    <section className="flex flex-col flex-1 min-h-0 w-full">
      {/* Mobile Navbar */}
      <div className="md:hidden flex items-center justify-between p-4 bg-navbar">
        <h2 className="text-xl font-semibold">{t("settings.title")}</h2>
        <button
          onClick={() => setMobileMenuOpen((v) => !v)}
          className="p-2 rounded-md hover:bg-accent focus:outline-none focus:ring"
        >
          {mobileMenuOpen ? <FiX className="w-5 h-5" /> : <FiMenu className="w-5 h-5" />}
        </button>
      </div>
      <div className="flex flex-1 min-h-0 w-full">
        {/* Desktop Sidebar */}
        <aside
          className={cn(
            "bg-navbar text-sidebar-foreground flex-shrink-0 min-h-0 md:w-64 md:block overflow-y-auto transition-all z-20",
            mobileMenuOpen ? " w-64 h-full block shadow-lg" : "hidden md:block"
          )}
        >
          <div className="p-4 hidden items-center md:flex">
            {guild.icon ? (
              <img
                src={`https://cdn.discordapp.com/icons/${guild.id}/${guild.icon}.webp`}
                alt={guild.name}
                className="w-8 h-8 rounded-full mr-4 object-cover bg-hover"
              />
            ) : (
              <div className="w-8 h-8 rounded-full mr-4 flex items-center justify-center font-bold text-xl bg-gray-500">
                {guild.name[0].toUpperCase()}
              </div>
            )}
            <h2 className="font-medium">{guild.name}</h2>
          </div>
          <Sidebar
            categories={categories}
            activeCategory={activeCategory}
            setActiveCategory={setActiveCategory}
            setMobileMenuOpen={setMobileMenuOpen}
          />
        </aside>
        {/* Main content */}
        <ScrollArea className="flex-1 min-h-0 flex flex-col inset-shadow-md">
          {categories.find((cat) => cat.id === activeCategory)?.component(guild.id) || (
            <div>{t("settings.error")}</div>
          )}
        </ScrollArea>
      </div>
    </section>
  );
}
