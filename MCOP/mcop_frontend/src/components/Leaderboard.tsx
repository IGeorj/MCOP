import React from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { GuildUserStats } from "../types/GuildUserStats";
import { getDiscordAvatarUrl, getDefaultAvatarUrl } from "../utils/avatarUtils";
import { config } from "../config";
import { useIsMobile } from "../hooks/useIsMobile";
import { useParams } from "react-router-dom";
import InfiniteScroll from "react-infinite-scroll-component";
import { useTranslation } from "react-i18next";
import LevelWithProgress from "./LevelWithProgress";
import { FaSortAmountUp, FaSortAmountDown  } from "react-icons/fa";
type SortKey = "likes" | "exp" | "duelWin" | "duelLose";

interface SortOption {
  key: SortKey;
  label: string;
}

const SORT_OPTIONS: SortOption[] = [
  { key: "exp", label: "level" },
  { key: "duelWin", label: "duelWin" },
  { key: "duelLose", label: "duelLose" },
  { key: "likes", label: "likes" },
];

function mapDtoToGuildUserStats(dto: GuildUserStats): GuildUserStats {
  return {
    guildId: dto.guildId,
    userId: dto.userId,
    username: dto.username ?? "Unknown",
    avatarHash: dto.avatarHash ?? "",
    duelWin: dto.duelWin ?? 0,
    duelLose: dto.duelLose ?? 0,
    likes: dto.likes ?? 0,
    exp: dto.exp ?? 0,
    currentLevelExp: dto.currentLevelExp ?? 0,
    nextLevelExp: dto.nextLevelExp ?? 0,
    level: dto.level ?? 0,
    winRate: dto.winRate ?? 0
  };
}

const PAGE_SIZE = 20;

const Leaderboard: React.FC = () => {
  const { t } = useTranslation();
  const { guildId } = useParams<{ guildId: string }>();
  const [sortKey, setSortKey] = React.useState<SortKey>("exp");
  const [sortDescending, setSortDescending] = React.useState<boolean>(true);

  const isMobile = useIsMobile(640);

  const {
    data,
    error,
    isFetching,
    isError,
    fetchNextPage,
    hasNextPage,
    refetch,
    isFetchingNextPage,
  } = useInfiniteQuery({
    queryKey: ["leaderboard", guildId, sortKey, sortDescending],
    queryFn: ({ pageParam }) => fetchLeaderboard(pageParam),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => lastPage.nextPage,
    enabled: !!guildId,
    retry: 1,
    staleTime: 1 * 60 * 1000, // cache for 1 minutes
  });

  const fetchLeaderboard = async (pageParam = 1) => {
    if (!guildId) throw new Error("Missing guildId");
    const url = `${config.API_URL}/leaderboard/${guildId}?sortBy=${sortKey}&sortDescending=${sortDescending}&page=${pageParam}&pageSize=${PAGE_SIZE}`;
    const resp = await fetch(url, { credentials: "include" });
    if (!resp.ok) throw new Error(t("leaderboard.error"));
    const result = await resp.json();
    return {
      items: result.map(mapDtoToGuildUserStats),
      nextPage: result.length === PAGE_SIZE ? pageParam + 1 : undefined,
    };
  };

  React.useEffect(() => {
    refetch();
  }, [sortKey, sortDescending, guildId, refetch]);

  const flatData = data?.pages ? data.pages.flatMap((p) => p.items) : [];

  const renderSortOrderIcon = (active: boolean) => {
    if (!active) return null;
    return sortDescending 
    ? <FaSortAmountDown className="h-3.5 w-3.5 ml-1" /> 
    : <FaSortAmountUp className="h-3.5 w-3.5 ml-1" />;
  };

  const handleSortClick = (key: SortKey) => {
    if (sortKey === key) {
      setSortDescending((prev) => !prev);
    } else {
      setSortKey(key);
      setSortDescending(true);
    }
  };

  // --- MOBILE CARD RENDERER ---
  const renderMobileCard = (u: GuildUserStats) => (
    <div
      key={u.userId}
      className="flex items-center mb-3 px-4 py-3 rounded-2xl bg-secondary shadow-sm"
      style={{ minWidth: 0 }}
    >
      <img
        src={getDiscordAvatarUrl(u.userId, u.avatarHash)}
        alt={u.username}
        className="rounded-full w-12 h-12 object-cover flex-shrink-0 mr-3"
        style={{ minWidth: 48, minHeight: 48 }}
        onError={(e) => {
          const target = e.target as HTMLImageElement;
          target.src = getDefaultAvatarUrl(u.userId)
          target.onerror = null;
        }}
      />
      <div className="flex flex-col flex-grow min-w-0">
        <span className="font-medium text-base break-words max-w-[150px] truncate" title={u.username}>
          {u.username}
        </span>
        <div className="flex items-center gap-3 mt-1">
          <span className="text-sm flex items-center" title={t("level")}>
            <LevelWithProgress
              level={u.level}
              exp={u.exp}
              currentLevelExp={u.currentLevelExp}
              nextLevelExp={u.nextLevelExp}
            />
          </span>
          <span className="text-sm" title={t("duelWin")}>🎖️ {u.duelWin}</span>
          <span className="text-sm" title={t("duelLose")}>☠️ {u.duelLose}</span>
          <span className="text-sm" title={t("likes")}>❤️ {u.likes}</span>
        </div>
      </div>
    </div>
  );

  // --- DESKTOP TABLE RENDERER ---
  const renderDesktopTable = () => (
    <table className="w-full table-auto border-separate [border-spacing:0_12px]">
      <thead>
        <tr></tr>
      </thead>
      <tbody>
        {flatData.map((u) => (
          <tr
            key={u.userId}
            className="group transition"
          >
            <td className="px-2 py-1 align-middle bg-secondary rounded-l-2xl">
              <img
                src={getDiscordAvatarUrl(u.userId, u.avatarHash)}
                alt={u.username}
                className="rounded-full w-10 h-10"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.src = getDefaultAvatarUrl(u.userId)
                  target.onerror = null;
                }}
              />
            </td>
            <td className="px-2 py-1 font-medium bg-secondary align-middle break-all max-w-[200px]">
              {u.username}
            </td>
            <td className="px-2 py-1 text-center bg-secondary align-middle">
              <LevelWithProgress
                level={u.level}
                exp={u.exp}
                currentLevelExp={u.currentLevelExp}
                nextLevelExp={u.nextLevelExp}
              />
            </td>
            <td className="px-2 py-1 text-left bg-secondary align-middle min-w-14" title={t("duelWin")}>
              🎖️ {u.duelWin}
            </td>
            <td className="px-2 py-1 text-left bg-secondary align-middle min-w-14" title={t("duelLose")}>
              ☠️ {u.duelLose}
            </td>
            <td className="px-2 py-1 text-left bg-secondary align-middle min-w-14 rounded-r-2xl" title={t("likes")}>
              ❤️ {u.likes}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );

  return (
    <div className="w-full max-w-2xl mx-auto my-8">
      <div className="flex gap-2 mb-4 justify-end">
        {SORT_OPTIONS.map(opt => (
          <button
            key={opt.key}
            onClick={() => handleSortClick(opt.key)}
            className={`px-3 py-1 items-center rounded text-primary cursor-pointer bg-secondary bg-hover active:opacity-90 ${sortKey === opt.key ? "border-primary border-1" : ""}`}
            disabled={isFetching || isFetchingNextPage}
            style={{ display: "flex", alignItems: "center" }}
            aria-pressed={sortKey === opt.key}
          >
            {t(opt.label)}
            {renderSortOrderIcon(sortKey === opt.key)}
          </button>
        ))}
      </div>
      {isError && (
        <div className="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded text-center">
          {error instanceof Error ? error.message : t("leaderboard.error")}
          <button
            className="ml-4 px-3 py-1 bg-red-200 hover:bg-red-300 rounded text-sm text-red-800"
            onClick={() => refetch()}
            disabled={isFetching}
          >
            {t("leaderboard.retry")}
          </button>
        </div>
      )}

      {!isError && (
        <InfiniteScroll
          dataLength={flatData.length}
          next={fetchNextPage}
          hasMore={!!hasNextPage}
          loader={<h4 style={{ textAlign: 'center' }}>{t("loading")}</h4>}
          endMessage={
            <p style={{ textAlign: 'center' }}>
              <b>{t("theEnd")}</b>
            </p>
          }
        >
          <div>
            {isMobile ? (
              <div className="flex flex-col gap-2">
                {flatData.map(renderMobileCard)}
              </div>
            ) : (
              renderDesktopTable()
            )}
          </div>
        </InfiniteScroll>
      )}
    </div>
  );
};

export default Leaderboard;
