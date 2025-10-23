import { Button } from "@/components/ui/button";
import { FiPlus, FiCheck, FiX, FiAward } from "react-icons/fi";
import { useTranslation } from "react-i18next";
import { RoleSelect } from "@/components/common/RoleSelect";
import { useEffect } from "react";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

interface AddLevelRoleCardProps {
    availableRoles: Role[];
    isAddingRole: boolean;
    newRole: { roleId: string; level: number; template: string };
    isAddingPending: boolean;
    onStartAddRole: () => void;
    onCancelAddRole: () => void;
    onSubmitAddRole: () => void;
    setNewRole: (role: { roleId: string; level: number; template: string }) => void;
}

export function AddLevelRoleCard({
    availableRoles,
    isAddingRole,
    newRole,
    isAddingPending,
    onStartAddRole,
    onCancelAddRole,
    onSubmitAddRole,
    setNewRole,
}: AddLevelRoleCardProps) {
    const { t } = useTranslation();

    const handleRoleSelect = (roleId: string) => {
        setNewRole({ ...newRole, roleId });
    };

    const handleLevelChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const level = Number(e.target.value);
        if (!isNaN(level) && level >= 1) {
            setNewRole({ ...newRole, level });
        }
    };

    const handleTemplateChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
        setNewRole({ ...newRole, template: e.target.value });
    };

    const handleKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
            onCancelAddRole();
        }
    };

    useEffect(() => {
        if (isAddingRole) {
            document.addEventListener('keydown', handleKeyDown);

            return () => {
                document.removeEventListener('keydown', handleKeyDown);
            };
        }
    }, [isAddingRole, onCancelAddRole]);

    const renderStartAdding = () => (
        <div
            className="rounded-lg border border-border p-4 hover:bg-accent dark:hover:bg-accent/50 transition-colors cursor-pointer flex flex-col items-center justify-center text-center min-h-[140px] group"
            onClick={onStartAddRole}
            title={t("common.add")}
        >
            <div className="text-muted-foreground group-hover:text-primary transition-colors mb-2">
                <FiPlus className="w-6 h-6" />
            </div>
        </div>
    );

    const renderAddingForm = () => (
        <div className="rounded-lg border border-border p-4 hover:bg-accent dark:hover:bg-accent/50 transition-colors">
            <div className="flex items-center justify-between mb-2 gap-3">
                <div className="flex-1 min-w-0">
                    <RoleSelect
                        roles={availableRoles}
                        selectedRole={newRole.roleId}
                        onRoleSelect={handleRoleSelect}
                        placeholder="leveling.selectRole"
                        searchPlaceholder="leveling.searchRoles"
                        emptyMessage="leveling.noAvailableRoles"
                        disabled={isAddingPending}
                        className="h-7 text-sm border-none shadow-none focus:ring-0 p-0 hover:bg-transparent w-full"
                    />
                </div>
                <div className="flex items-center gap-1 flex-shrink-0">
                    <Input
                        autoFocus
                        type="number"
                        min="1"
                        placeholder={t("leveling.level")}
                        className="h-6 w-12 px-2 py-1 text-center text-sm font-medium"
                        value={newRole.level.toString()}
                        onChange={handleLevelChange}
                        disabled={isAddingPending}
                    />
                </div>
            </div>

            <div className="mb-4">
                <Label className="text-xs text-muted-foreground mb-2 block">
                    {t("leveling.messageTemplate") ?? "Message Template"}
                </Label>
                <Textarea
                    autoFocus
                    className="w-full bg-background min-h-[80px] resize-none"
                    value={newRole.template}
                    onChange={handleTemplateChange}
                    placeholder={t("leveling.templatePlaceholder")}
                    disabled={isAddingPending}
                />
            </div>

            <div className="flex justify-end gap-1">
                <Button
                    variant="ghost"
                    size="sm"
                    className="cursor-pointer h-8 w-8 p-0"
                    onClick={onCancelAddRole}
                    disabled={isAddingPending}
                    title={t("common.cancel") ?? "Cancel"}
                >
                    <FiX className="w-4 h-4" />
                </Button>
                <Button
                    variant="ghost"
                    size="sm"
                    className="cursor-pointer h-8 w-8 p-0 text-green-600 hover:text-green-700 hover:bg-green-50"
                    onClick={onSubmitAddRole}
                    disabled={!newRole.roleId || newRole.level < 1 || isAddingPending}
                    title={t("common.add") ?? "Add"}
                >
                    <FiCheck className="w-4 h-4" />
                </Button>
            </div>
        </div>
    );

    return isAddingRole ? renderAddingForm() : renderStartAdding();
}