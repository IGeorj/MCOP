import { ReactElement, ReactNode } from "react";

type CategoryComponent = (guildId: string) => ReactElement;

export interface SettingsCategory {
    id: string;
    name: string;
    icon: ReactNode;
    component: CategoryComponent;
    link: string;
}