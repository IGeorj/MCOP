import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useRef } from "react";
import { roleMutations, roleQueries } from "@/api/roles";

export function useLevelRoles(guildId: string) {
  const queryClient = useQueryClient();
  const [editingRoleId, setEditingRoleId] = useState<string | null>(null);
  const [editingField, setEditingField] = useState<"level" | "template" | null>(null);
  const [editTemplateValue, setEditTemplateValue] = useState<string>("");
  const [editLevelValue, setEditLevelValue] = useState<string>("");
  const [isAddingRole, setIsAddingRole] = useState(false);
  const [newRoleId, setNewRoleId] = useState<string>("");
  const [newRoleLevel, setNewRoleLevel] = useState<number>(1);
  const [newRoleTemplate, setNewRoleTemplate] = useState<string>("");
  const ignoreBlurRef = useRef(false);

  // Мутации
  const invalidateRolesQuery = () => {
    queryClient.invalidateQueries({
      queryKey: roleQueries.getGuildRoles(guildId).queryKey
    });
  };

  // Мутации
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

  const { mutate: submitAddRoleMutation, isPending: isAdding } = useMutation({
    mutationFn: async ({ roleId, level, template }: { roleId: string; level: number, template: string }) => {
      await roleMutations.addOrUpdateLevelRole(guildId).mutationFn({ roleId, level });
      await roleMutations.updateRoleTemplate(guildId).mutationFn({ roleId, template });
    },
    onSuccess: () => {
      invalidateRolesQuery();
      cancelAddRole();
    },
  });

  // Методы для редактирования
  const startEditTemplate = (role: Role) => {
    setEditingRoleId(role.id);
    setEditingField("template");
    setEditTemplateValue(role.levelUpMessageTemplate ?? "");
  };

  const startEditLevel = (role: Role) => {
    setEditingRoleId(role.id);
    setEditingField("level");
    setEditLevelValue(role.levelToGetRole?.toString() ?? "");
  };

  const cancelEdit = () => {
    setEditingRoleId(null);
    setEditingField(null);
    setEditTemplateValue("");
    setEditLevelValue("");
    ignoreBlurRef.current = false;
  };

  const commitEditTemplate = () => {
    if (!editingRoleId || editingField !== "template") return;
    const value = editTemplateValue.trim() === "" ? null : editTemplateValue;
    updateRoleTemplate({ roleId: editingRoleId, template: value });
    cancelEdit();
  };

  const commitEditLevel = () => {
    if (!editingRoleId || editingField !== "level") return;
    
    const level = parseInt(editLevelValue);
    if (isNaN(level) || level < 1) {
      cancelEdit();
      return;
    }

    addOrUpdateLevelRole({ roleId: editingRoleId, level });
    cancelEdit();
  };

  // Методы для добавления
  const startAddRole = () => {
    setIsAddingRole(true);
    setNewRoleId("");
    setNewRoleLevel(1);
    setNewRoleTemplate("");
  };

  const cancelAddRole = () => {
    setIsAddingRole(false);
    setNewRoleId("");
    setNewRoleLevel(1);
    setNewRoleTemplate("");
  };

  const submitAddRole = () => {
    if (!newRoleId || isNaN(newRoleLevel) || newRoleLevel < 1) return;
        submitAddRoleMutation({roleId: newRoleId, level: newRoleLevel, template: newRoleTemplate});
  };

  // Вспомогательные методы
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
      return;
    }
    commitEditTemplate();
  };

  const handleLevelBlur = () => {
    if (ignoreBlurRef.current) {
      ignoreBlurRef.current = false;
      return;
    }
    commitEditLevel();
  };

  return {
    // Состояние
    editingRoleId,
    editingField,
    editTemplateValue,
    editLevelValue,
    isAddingRole,
    newRoleId,
    newRoleLevel,
    newRoleTemplate,
    isAdding,
    
    // Сеттеры
    setEditTemplateValue,
    setEditLevelValue,
    setNewRoleId,
    setNewRoleLevel,
    setNewRoleTemplate,
    
    // Методы
    startEditTemplate,
    startEditLevel,
    cancelEdit,
    commitEditTemplate,
    commitEditLevel,
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