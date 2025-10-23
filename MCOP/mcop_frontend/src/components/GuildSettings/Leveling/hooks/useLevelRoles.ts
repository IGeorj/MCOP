import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useRef } from "react";
import { roleMutations, roleQueries } from "@/api/roles";

export function useLevelRoles(guildId: string) {
  const queryClient = useQueryClient();
  const [editingRole, setEditingRole] = useState<{ roleId: string; field: "level" | "template" } | null>(null);
  const [editTemplateValue, setEditTemplateValue] = useState<string>("");
  const [editLevelValue, setEditLevelValue] = useState<string>("");
  const [isAddingRole, setIsAddingRole] = useState(false);
  const [newRole, setNewRole] = useState<{ roleId: string; level: number; template: string }>({
    roleId: "",
    level: 1,
    template: "",
  });
    const ignoreBlurRef = useRef(false);

  const invalidateRolesQuery = () => {
    queryClient.invalidateQueries({
      queryKey: roleQueries.getGuildRoles(guildId).queryKey
    });
  };

  const { mutate: removeLevelRole } = useMutation({
    ...roleMutations.removeLevelRole(guildId),
    onSuccess: invalidateRolesQuery,
  });

  const { mutate: updateRoleTemplate } = useMutation({
    ...roleMutations.updateRoleTemplate(guildId),
    onSuccess: invalidateRolesQuery,
  });

  const { mutate: addOrUpdateLevelRole } = useMutation({
    ...roleMutations.addOrUpdateLevelRole(guildId),
    onSuccess: invalidateRolesQuery,
  });

  const { mutate: submitAddRoleMutation, isPending: isAddingPending } = useMutation({
    mutationFn: async ({ roleId, level, template }: { roleId: string; level: number; template: string }) => {
      await roleMutations.addOrUpdateLevelRole(guildId).mutationFn({ roleId, level });
      await roleMutations.updateRoleTemplate(guildId).mutationFn({ roleId, template });
    },
    onSuccess: () => {
      invalidateRolesQuery();
      cancelAddRole();
    },
  });

  const startEdit = (role: Role, field: "level" | "template") => {
    setEditingRole({ roleId: role.id, field });
    setEditTemplateValue(role.levelUpMessageTemplate ?? "");
    setEditLevelValue(role.levelToGetRole?.toString() ?? "");
  };

  const cancelEdit = () => {
    setEditingRole(null);
    setEditTemplateValue("");
    setEditLevelValue("");
    ignoreBlurRef.current = false;
  };

  const commitEdit = () => {
    if (!editingRole) return;

    switch (editingRole.field) {
      case "template":
        const templateValue = editTemplateValue.trim() === "" ? null : editTemplateValue;
        updateRoleTemplate({ roleId: editingRole.roleId, template: templateValue });
        break;
      case "level":
        const levelValue = parseInt(editLevelValue.toString());
        if (!isNaN(levelValue) && levelValue >= 1) { // Validate and check if value has changed
          addOrUpdateLevelRole({ roleId: editingRole.roleId, level: levelValue });
        }
        break;
    }

    cancelEdit();
  };

  const startAddRole = () => {
    setIsAddingRole(true);
    setNewRole({ roleId: "", level: 1, template: "" });
  };

  const cancelAddRole = () => {
    setIsAddingRole(false);
    setNewRole({ roleId: "", level: 1, template: "" });
  };

  const submitAddRole = () => {
    if (!newRole.roleId || newRole.level < 1) return;
    submitAddRoleMutation({ roleId: newRole.roleId, level: newRole.level, template: newRole.template });
  };

  const handleKeyDown = (e: React.KeyboardEvent, commitFunction: () => void) => {
    if (e.key === 'Enter') {
      commitFunction();
    } else if (e.key === 'Escape') {
      cancelEdit();
    }
  };

  const handleTemplateBlur = () => {
    if (ignoreBlurRef.current) {
      ignoreBlurRef.current = false;
    } else {
      commitEdit();
    }
  };

  const handleLevelBlur = () => {
    if (ignoreBlurRef.current) {
      ignoreBlurRef.current = false;
    } else {
      commitEdit();
    }
  };

  return {
    // State
    editingRole,
    editTemplateValue,
    editLevelValue,
    isAddingRole,
    newRole,
    isAddingPending,

    // Setters
    setEditTemplateValue,
    setEditLevelValue,
    setNewRole,

    // Methods
    startEdit,
    cancelEdit,
    commitEdit,
    startAddRole,
    cancelAddRole,
    submitAddRole,
    removeLevelRole,
    handleKeyDown,
    handleTemplateBlur,
    handleLevelBlur,
    ignoreBlurRef,
  };
}