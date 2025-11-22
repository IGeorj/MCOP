import React from "react";
import { FiChevronDown } from "react-icons/fi";
import { Button } from "@/components/ui/button";
import { Command, CommandEmpty, CommandInput, CommandItem } from "@/components/ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { Role } from "@/types/Role";

interface RoleSelectProps {
    roles: Role[];
    selectedRole: string | null;
    onRoleSelect: (roleId: string) => void;
    disabled?: boolean;
    className?: string;
}

export function RoleSelect({
    roles,
    selectedRole,
    onRoleSelect,
    disabled = false,
    className = ""
}: RoleSelectProps) {
    const { t } = useTranslation();
    const [searchTerm, setSearchTerm] = useState("");
    const [open, setOpen] = useState(false);

    const filteredRoles = roles?.filter(role =>
        role.name.toLowerCase().includes(searchTerm.toLowerCase())
    ) || [];

    const selectedRoleData = roles?.find(r => r.id === selectedRole);
    const displayValue = selectedRoleData ? (
        <div className="flex items-center gap-2">
            <span
                className="h-3 w-3 rounded-full flex-shrink-0"
                style={{ backgroundColor: selectedRoleData.color || 'transparent' }}
            />
            <span className="truncate">{selectedRoleData.name}</span>
        </div>
    ) : (
        t("common.selectRole")
    );

    return (
        <Popover open={open} onOpenChange={setOpen} modal={true}>
            <PopoverTrigger asChild>
                <Button
                    variant="outline"
                    role="combobox"
                    aria-expanded={open}
                    className={`w-full justify-between ${className}`}
                    disabled={disabled}
                >
                    <span className="truncate">{displayValue}</span>
                    <FiChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
                <Command>
                    <div className="flex items-center px-3 bg-navbar">
                        <CommandInput
                            placeholder={t("common.searchRoles")}
                            value={searchTerm}
                            onValueChange={setSearchTerm}
                        />
                    </div>
                    <ScrollArea className="bg-navbar h-80 px-4 py-2">
                        {filteredRoles.length === 0 ? (
                            <CommandEmpty>{t("common.none")}</CommandEmpty>
                        ) : (
                            filteredRoles.map(role => (
                                <CommandItem
                                    key={role.id}
                                    value={role.name}
                                    onSelect={() => {
                                        onRoleSelect(role.id);
                                        setOpen(false);
                                        setSearchTerm("");
                                    }}
                                    className="cursor-pointer"
                                >
                                    <div className="flex items-center gap-2 w-full">
                                        <span
                                            className="h-3 w-3 rounded-full flex-shrink-0"
                                            style={{ backgroundColor: role.color || 'transparent' }}
                                        />
                                        <span className="truncate">{role.name}</span>
                                    </div>
                                </CommandItem>
                            ))
                        )}
                    </ScrollArea>
                </Command>
            </PopoverContent>
        </Popover>
    );
}