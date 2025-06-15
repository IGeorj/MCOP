import { cn } from "@/lib/utils";
import { SettingsCategory } from "@/types/SettingsCategory";
import { Dispatch, SetStateAction } from "react";

export function Sidebar({
    categories,
    activeCategory,
    setActiveCategory,
    setMobileMenuOpen
}: {
    categories: SettingsCategory[]
    activeCategory: string,
    setActiveCategory: Dispatch<SetStateAction<string>>,
    setMobileMenuOpen: Dispatch<SetStateAction<boolean>>
}) {
    return (
        <nav
            className="flex flex-col gap-1 p-2"
            aria-label="Server settings categories"
        >
            {categories.map((category) => (
                <button
                    key={category.id}
                    onClick={() => {
                        setActiveCategory(category.id);
                        setMobileMenuOpen(false);
                    }}
                    className={cn(
                        "flex items-center gap-3 p-3 rounded-md text-sm font-medium transition-colors cursor-pointer hover:bg-accent dark:hover:bg-accent/50",
                        activeCategory === category.id ? "text-primary border-primary border-1" : "text-muted-foreground"
                    )}
                    aria-current={activeCategory === category.id ? "page" : undefined}
                >
                    {category.icon}
                    <span>{category.name}</span>
                </button>
            ))}
        </nav>
    );
}