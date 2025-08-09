import { config } from "@/config";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { FiTrash2 } from "react-icons/fi";
import { useTranslation } from "react-i18next";

export function CurrentLevelRoles({
  guildId,
  roles,
  isLoadingRoles,
  searchTerm
}: {
  guildId: string;
  roles: Role[] | undefined;
  isLoadingRoles: boolean;
  searchTerm: string;
}) {
  const { t } = useTranslation();

  const queryClient = useQueryClient();

  const { mutate: removeLevelRole } = useMutation({
    mutationFn: async (roleId: string) => {
      const resp = await fetch(`${config.API_URL}/guilds/${guildId}/level-roles/${roleId}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${localStorage.getItem("app_session")}`,
          "Content-Type": "application/json",
        },
      });
      if (!resp.ok) throw new Error("Failed to remove level role");
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guildRoles", guildId] });
    },
  });

  const filteredRoles = roles
    ?.filter(role =>
      role.levelToGetRole !== null &&
      role.name.toLowerCase().includes(searchTerm.toLowerCase())
    )
    ?.sort((a, b) => (a.levelToGetRole || 0) - (b.levelToGetRole || 0)) || [];


  return (
    <div className="bg-navbar p-4 rounded-lg border border-border">
      <h4 className="font-medium mb-4">{t("leveling.currentLevelRoles")}</h4>

      {isLoadingRoles ? (
        <div className="space-y-2">
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} className="h-10 w-full" />
          ))}
        </div>
      ) : filteredRoles.length === 0 ? (
        <p className="text-muted-foreground">{t("leveling.noLevelRoles")}</p>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("leveling.role")}</TableHead>
              <TableHead>{t("leveling.level")}</TableHead>
              <TableHead>{t("leveling.actions")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredRoles.map((role) => (
              <TableRow key={role.id} className="border-0 hover:bg-accent dark:hover:bg-accent/50">
                <TableCell className="flex items-center gap-1 text-base">
                  <span className="mr-2 h-4 w-4 rounded-full" style={{ backgroundColor: role.color || 'transparent' }} />
                  {role.name}
                  </TableCell>
                <TableCell>{role.levelToGetRole}</TableCell>
                <TableCell>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="cursor-pointer"
                    onClick={() => removeLevelRole(role.id)}
                  >
                    <FiTrash2 className="w-4 h-4" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}