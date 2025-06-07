import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import {
  ChevronDown,
  LogOut,
  User
} from "lucide-react";
import { LanguageSwitcher } from "./LanguageSwitcher";
import ThemeToggle from "./ThemeToggle";
import DiscordLoginButton from "./DiscordLoginButton";

interface NavbarProps {
  isLoggedIn: boolean;
  username?: string;
  avatarUrl?: string;
  onLogin: () => void;
  onLogout: () => void;
}

export const Navbar = ({
  isLoggedIn,
  username,
  avatarUrl,
  onLogin,
  onLogout,
}: NavbarProps) => {
  const { t } = useTranslation();

  return (
    <nav className="sticky top-0 z-40 flex items-center justify-between px-4 py-3 bg-navbar shadow-lg md:px-8">
      {/* Логотип */}
      <Link to="/" className="flex items-center gap-3">
        <img src="/coplogo.ico" alt="Site logo" className="h-10" />
        <span className="font-bold text-2xl tracking-widest text-primary hover:opacity-80">
          MCOP
        </span>
      </Link>

      {/* Десктопные элементы */}
      <div className="hidden md:flex items-center gap-4">
        <LanguageSwitcher />
        <ThemeToggle />

        {isLoggedIn ? (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="flex items-center gap-2 px-3 cursor-pointer">
                {avatarUrl ? (
                  <img
                    src={avatarUrl}
                    alt="User avatar"
                    className="h-8 w-8 rounded-full border"
                  />
                ) : (
                  <User className="h-5 w-5" />
                )}
                <span>{username}</span>
                <ChevronDown className="h-4 w-4 opacity-80" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-48" align="end">
              <DropdownMenuItem
                onClick={onLogout}
                className="flex items-center gap-2 cursor-pointer"
              >
                <LogOut className="h-4 w-4" />
                {t("logout")}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        ) : (
          <DiscordLoginButton onLogin={onLogin} />
        )}
      </div>

      {/* Мобильные элементы */}
      <div className="flex items-center gap-2 md:hidden">
        {isLoggedIn ? (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                className="flex items-center gap-2 px-2"
              >
                {avatarUrl ? (
                  <img
                    src={avatarUrl}
                    alt="User avatar"
                    className="h-9 w-9 rounded-full border"
                  />
                ) : (
                  <User className="h-5 w-5" />
                )}
                <ChevronDown className="h-4 w-4 opacity-80" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end">
              <div className="px-4 py-1.5 flex items-center gap-2">
                <p className="text-lg">{username}</p>
              </div>

              <DropdownMenuSeparator />

              <div className="px-2 pr-4 py-2 flex items-center justify-between">
                <LanguageSwitcher />
                <ThemeToggle />
              </div>

              <DropdownMenuSeparator />

              <DropdownMenuItem
                onClick={onLogout}
                className="px-4 py-2 flex items-center gap-2 cursor-pointer"
              >
                <LogOut className="h-4 w-4" />
                {t("logout")}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        ) : (
          <DiscordLoginButton onLogin={onLogin} />
        )}
      </div>
    </nav>
  );
};