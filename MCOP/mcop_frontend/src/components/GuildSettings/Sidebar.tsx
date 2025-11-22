import React from "react";
import { cn } from "@/lib/utils";
import { Link } from "react-router-dom";
import { SettingsCategory } from "@/types/SettingsCategory";
import { Dispatch, SetStateAction } from "react";
import { FaAngleRight } from "react-icons/fa";
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
            className="flex flex-col gap-1 pl-3 pr-2 py-2"
            aria-label="Server settings categories"
        >
            {categories.map((category) => (
                <Link
                    key={category.name}
                    to={category.link}
                    onClick={() => {
                        setActiveCategory(category.id);
                        setMobileMenuOpen(false);
                    }}
                    className={cn(
                        "flex items-center gap-2 p-3 rounded-md text-sm font-medium transition-colors cursor-pointer hover:bg-accent dark:hover:bg-accent/50",
                        activeCategory === category.id ? "text-primary" : "text-muted-foreground"
                    )}
                    aria-current={activeCategory === category.id ? "page" : undefined}
                >
                    {category.icon}
                    <div className="flex justify-between items-center w-full">
                        <span>{category.name}</span>
                        {activeCategory === category.id && <FaAngleRight />}
                    </div>
                </Link>
            ))}
        </nav>
    );
}