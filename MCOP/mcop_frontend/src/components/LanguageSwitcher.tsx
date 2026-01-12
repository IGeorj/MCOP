import React from "react";
import { useTranslation } from "react-i18next";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "../components/ui/dropdown-menu";
import { Button } from "../components/ui/button";
import { ChevronDown } from "lucide-react";

const LANGUAGES = [
  { code: "en", label: "EN" },
  { code: "ru", label: "RU" },
];

export const LanguageSwitcher = () => {
  const { i18n } = useTranslation();
  const currentLang = LANGUAGES.find((lang) => lang.code === i18n.language) || LANGUAGES[0];

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size={"default"}
          className={`flex items-center gap-2 cursor-pointer`}
        >
          <span>{currentLang.label}</span>
          <ChevronDown className="h-4 w-4 opacity-50" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        align={"center"}
        className="min-w-16"
      >
        {LANGUAGES.map((lang) => (
          <DropdownMenuItem
            key={lang.code}
            onClick={() => i18n.changeLanguage(lang.code)}
            className="flex justify-between"
          >
            <span>{lang.label}</span>
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
};