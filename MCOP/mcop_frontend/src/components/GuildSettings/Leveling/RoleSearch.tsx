import { useTranslation } from "react-i18next";
import { FiSearch } from "react-icons/fi";
import { Input } from "@/components/ui/input";

export function RoleSearch({ searchTerm, setSearchTerm }: {
  searchTerm: string;
  setSearchTerm: (term: string) => void;
}) {
  const { t } = useTranslation();

  return (
    <div className="relative">
      <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
      <Input
        placeholder={t("leveling.searchRoles")}
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="pl-10 bg-navbar"
      />
    </div>
  );
}
