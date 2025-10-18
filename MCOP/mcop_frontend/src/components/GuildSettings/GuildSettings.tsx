import { useEffect, useState } from "react";
import { FiMenu, FiAward, FiEyeOff, FiImage } from "react-icons/fi";
import { cn } from "@/lib/utils";
import { LevelingSettings } from "./Leveling/LevelingSettings";
import { useParams } from "react-router-dom";
import { Guild } from "@/types/Guild";
import { ScrollArea } from "@/components/ui/scroll-area";
import { SettingsCategory } from "@/types/SettingsCategory";
import { Sidebar } from "./Sidebar";
import { useTranslation } from "react-i18next";
import { useGuildList } from "../../contexts/GuildListContext";
import { config } from "@/config";
import { useQuery } from "@tanstack/react-query";
import { Skeleton } from "@/components/ui/skeleton";
import { useNavigate } from "react-router-dom";
import { GuildSelect } from "./GuildSelect";
import { NsfwSettings } from "./Nsfw/NsfwSettings";
import { ImageVerificationSettings } from "./ImageVerification/ImageVerificationSettings";

const LoadingSkeleton = () => (
  <section className="flex flex-col flex-1 min-h-0 w-full">
    <div className="md:hidden flex items-center p-4 bg-navbar gap-4">
      <Skeleton className="h-8 w-8 rounded-full" />
      <Skeleton className="h-6 w-32" />
    </div>
    <div className="flex flex-1 min-h-0 w-full">
      <aside className="bg-navbar flex-shrink-0 min-h-0 md:w-64 md:block overflow-y-auto transition-all z-20 hidden">
        <div className="p-4 hidden items-center md:flex">
          <Skeleton className="w-8 h-8 rounded-full mr-4" />
          <Skeleton className="h-6 w-24" />
        </div>
        <div className="space-y-4 p-4">
          <Skeleton className="h-6 w-32" />
          <Skeleton className="h-6 w-24" />
        </div>
      </aside>
      <div className="flex-1 min-h-0 flex flex-col p-8 space-y-4">
        <Skeleton className="h-8 w-full" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Skeleton className="h-40 w-full" />
          <Skeleton className="h-40 w-full" />
        </div>
      </div>
    </div>
  </section>
);

const MobileSidebar = ({ 
  currentGuild, 
  guilds, 
  onGuildSelect, 
  onMenuToggle 
}: {
  currentGuild: Guild;
  guilds: Guild[];
  onGuildSelect: (value: string) => void;
  onMenuToggle: () => void;
}) => (
  <div className="md:hidden flex items-center justify-between p-4 bg-navbar">
    <GuildSelect 
      guilds={guilds} 
      currentGuild={currentGuild} 
      onSelect={onGuildSelect} 
    />
    <button
      onClick={onMenuToggle}
      className="p-2 rounded-md hover:bg-accent cursor-pointer"
    >
      {<FiMenu className="w-5 h-5" />}
    </button>
  </div>
);

export function GuildSettings({ activeCategory: initialActiveCategory = "leveling" }: { activeCategory?: string }) {
  const { t } = useTranslation();
  const { guildId } = useParams();
  const navigate = useNavigate();
  const { guilds, setGuilds } = useGuildList();
  const [activeCategory, setActiveCategory] = useState(initialActiveCategory);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const categories: SettingsCategory[] = [
    {
      id: "leveling",
      name: t("leveling.title"),
      icon: <FiAward className="w-5 h-5" />,
      component: (guildId: string) => <LevelingSettings guildId={guildId} />,
      link: `/guilds/${guildId}/leveling`,
    },    {
      id: "nsfw",
      name: t("nsfw.title"),
      icon: <FiEyeOff className="w-5 h-5" />,
      component: (guildId: string) => <NsfwSettings guildId={guildId} />,
      link: `/guilds/${guildId}/nsfw`,
    },
    {
      id: "image",
      name: "Image",
      icon: <FiImage className="w-5 h-5" />,
      component: (guildId: string) => <ImageVerificationSettings guildId={guildId} />,
      link: `/guilds/${guildId}/image`,
    }
  ];

  const guild = guilds.find(g => g.id === guildId);
  const { data: fetchedGuild, isLoading, error } = useQuery({
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
    enabled: !guild && !!guildId,
    retry: 1,
  });

  useEffect(() => {
    if (fetchedGuild && !guilds.some(g => g.id === fetchedGuild.id)) {
      setGuilds(prev => [...prev, fetchedGuild]);
    }
  }, [fetchedGuild, guilds, setGuilds]);

  const currentGuild = guild || fetchedGuild;

  const handleGuildSelect = (value: string) => {
    navigate(`/guilds/${value}`);
  };

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(v => !v);
  };

  if (isLoading || !currentGuild) {
    return <LoadingSkeleton />;
  }

  return (
    <section className="flex flex-col flex-1 min-h-0 w-full">
      <MobileSidebar
        currentGuild={currentGuild}
        guilds={guilds}
        onGuildSelect={handleGuildSelect}
        onMenuToggle={toggleMobileMenu}
      />

      <div className="flex flex-1 min-h-0 w-full">
        <aside
          className={cn(
            "bg-navbar text-sidebar-foreground flex-shrink-0 min-h-0 md:w-64 md:block overflow-y-auto transition-all z-20",
            isMobileMenuOpen ? "w-full h-full block shadow-lg" : "hidden md:block"
          )}
        >
          <div className="gap-1 px-2 pt-1 pb-0 hidden items-center md:flex">
            <GuildSelect 
              guilds={guilds} 
              currentGuild={currentGuild} 
              onSelect={handleGuildSelect} 
            />
          </div>
          <Sidebar
            categories={categories}
            activeCategory={activeCategory}
            setActiveCategory={setActiveCategory}
            setMobileMenuOpen={setIsMobileMenuOpen}
          />
        </aside>
        <ScrollArea className={`flex-1 min-h-0 flex flex-col inset-shadow-md ${isMobileMenuOpen ? "hidden": ""}`}>
          {categories.find((cat) => cat.id === activeCategory)?.component(currentGuild.id) || (
            <div>{t("settings.error")}</div>
          )}
        </ScrollArea>
      </div>
    </section>
  );
}