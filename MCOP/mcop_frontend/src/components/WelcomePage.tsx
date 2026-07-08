import { useState } from "react";
import { useTranslation } from "react-i18next";
import { BarChart3, Wallet, Gamepad2, Sparkles } from "lucide-react";
import Cube from "./Cube";
import DiscordLoginButton from "./DiscordLoginButton";
import { ScrollArea } from "@/components/ui/scroll-area";

interface WelcomePageProps {
    onLogin: () => void;
}

interface ParticleProps {
    id: number;
    x: number;
    y: number;
    size: number;
    duration: number;
    delay: number;
    opacity: number;
}

function FloatingParticle({ x, y, size, duration, delay, opacity }: ParticleProps) {
    return (
        <div
            className="absolute rounded-full bg-primary/40 pointer-events-none"
            style={{
                width: `${size}px`,
                height: `${size}px`,
                left: `${x}%`,
                bottom: `${y}%`,
                animation: `float ${duration}s ease-in-out ${delay}s infinite alternate`,
                opacity,
            }}
        />
    );
}

interface FeatureCardProps {
    icon: React.ReactNode;
    title: string;
    description: string;
}

function FeatureCard({ icon, title, description }: FeatureCardProps) {
    const [hovered, setHovered] = useState(false);

    return (
        <div
            className="card relative overflow-hidden"
            onMouseEnter={() => setHovered(true)}
            onMouseLeave={() => setHovered(false)}
        >
            <div
                className="absolute inset-0 bg-linear-to-br from-primary/5 to-transparent transition-opacity duration-300"
                style={{ opacity: hovered ? 1 : 0 }}
            />
            <div className="relative z-10">
                <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10 text-primary">
                    {icon}
                </div>
                <h3 className="mb-2 text-lg font-semibold">{title}</h3>
                <p className="text-muted-foreground text-sm leading-relaxed">{description}</p>
            </div>
        </div>
    );
}


function WelcomePage({ onLogin }: WelcomePageProps) {
    const { t } = useTranslation();

    const particles: ParticleProps[] = useState(() =>
        Array.from({ length: 15 }, (_, i) => ({
            id: i,
            x: Math.random() * 100,
            y: Math.random() * 100,
            size: Math.random() * 6 + 2,
            duration: Math.random() * 6 + 8,
            delay: Math.random() * 5,
            opacity: Math.random() * 0.3 + 0.5,
        }))
    )[0];

    const features: FeatureCardProps[] = [
        {
            icon: <BarChart3 className="h-6 w-6" />,
            title: t("welcome.features.stats.title"),
            description: t("welcome.features.stats.description"),
        },
        {
            icon: <Wallet className="h-6 w-6" />,
            title: t("welcome.features.economy.title"),
            description: t("welcome.features.economy.description"),
        },
        {
            icon: <Gamepad2 className="h-6 w-6" />,
            title: t("welcome.features.games.title"),
            description: t("welcome.features.games.description"),
        },
        {
            icon: <Sparkles className="h-6 w-6" />,
            title: t("welcome.features.leveling.title"),
            description: t("welcome.features.leveling.description"),
        },
    ];

    return (
        <ScrollArea >
            <div className="relative flex flex-col items-center">
                <style>
                    {`
                        @keyframes float {
                        0% { transform: translateY(0px) translateX(0px); }
                        50% { transform: translateY(-20px) translateX(10px); }
                        100% { transform: translateY(0px) translateX(0px); }
                        }
                        @keyframes pulse-glow {
                        0%, 100% { box-shadow: 0 0 20px rgba(240, 50, 82, 0.2); }
                        50% { box-shadow: 0 0 40px rgba(240, 50, 82, 0.4); }
                        }
                        @keyframes fade-in-up {
                        from { opacity: 0; transform: translateY(20px); }
                        to { opacity: 1; transform: translateY(0); }
                        }
                    `}
                </style>

                {particles.map((p) => (
                    <FloatingParticle key={p.id} {...p} />
                ))}

                <div className="relative z-10 flex w-full max-w-4xl flex-col items-center px-4 py-12 sm:py-16">
                    <section className="mb-8 flex justify-center">
                        <div className="relative flex items-center justify-center">
                            <div
                                className="absolute inset-0 rounded-full blur-3xl"
                                style={{
                                    background: "radial-gradient(circle, rgba(240,50,82,0.25) 0%, transparent 70%)",
                                    animation: "pulse-glow 4s ease-in-out infinite",
                                }}
                            />
                            <Cube />
                        </div>
                    </section>

                    <h1
                        className="mb-4 text-center text-4xl font-bold sm:text-5xl"
                        style={{ animation: "fade-in-up 0.6s ease-out both" }}
                    >
                        <span className="text-primary">{t("welcome.title")}</span>
                    </h1>

                    <p
                        className="mb-10 max-w-xl text-center text-lg text-muted-foreground"
                        style={{ animation: "fade-in-up 0.6s ease-out 0.15s both" }}
                    >
                        {t("welcome.subtitle")}
                    </p>

                    <div
                        className="mb-16"
                        style={{ animation: "fade-in-up 0.6s ease-out 0.3s both" }}
                    >
                        <DiscordLoginButton onLogin={onLogin} text={t("welcome.login")} />
                    </div>

                    <div className="grid w-full grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
                        {features.map((feature, i) => (
                            <div
                                key={i}
                                style={{ animation: `fade-in-up 0.5s ease-out ${0.45 + i * 0.1}s both` }}
                            >
                                <FeatureCard {...feature} />
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </ScrollArea>

    );
};

export default WelcomePage;
