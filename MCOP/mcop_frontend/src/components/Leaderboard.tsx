import React, { useEffect, useState, useCallback } from "react";
import { GuildUserStats } from "../types/GuildUserStats";
import { getDiscordAvatarUrl } from "../utils/avatarUtils";
import { config } from "../config";
import { useIsMobile } from "../hooks/useIsMobile";
import { useParams } from "react-router-dom";
import InfiniteScroll from "react-infinite-scroll-component";
import { useTranslation } from "react-i18next";
import LevelWithProgress from "./LevelWithProgress";

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

function mapDtoToGuildUserStats(dto: any): GuildUserStats {
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
    winRate: "winRate" in dto
      ? dto.winRate
      : (dto.duelWin + dto.duelLose > 0
        ? Math.round((dto.duelWin / (dto.duelWin + dto.duelLose)) * 10000) / 100
        : 0),
  };
}

const PAGE_SIZE = 20;

const Leaderboard: React.FC = () => {
  const { t } = useTranslation();
  const { guildId } = useParams<{ guildId: string }>();
  const [sortKey, setSortKey] = useState<SortKey>("exp");
  const [sortDescending, setSortDescending] = useState<boolean>(true);
  const [data, setData] = useState<GuildUserStats[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const isMobile = useIsMobile(640); // Tailwind 'sm' breakpoint

  const fetchLeaderboard = useCallback(async (
    sortBy: SortKey,
    pageNum: number,
    descending: boolean,
    reset: boolean = false
  ): Promise<void> => {
    if (!guildId || loading) return;
    
    setError(null);
    const url = `${config.API_URL}/leaderboard/${guildId}?sortBy=${sortBy}&sortDescending=${descending}&page=${pageNum}&pageSize=${PAGE_SIZE}`;
    try {
      setLoading(true);
      const resp = await fetch(url, {
        credentials: "include",
      });
      if (!resp.ok) {
        setLoading(false);
        setError(t("leaderboard.error"));
        return;
      }
      const result = await resp.json();
      
      setLoading(false);
      setData(prev => reset ? result.map(mapDtoToGuildUserStats) : [...prev, ...result.map(mapDtoToGuildUserStats)]);
      setHasMore(result.length === PAGE_SIZE);
    } catch (err) {
      setLoading(false);
      setError(t("leaderboard.error"));
    }
  }, [guildId, loading, t]);

  useEffect(() => {
    setData([]);
    setPage(1);
    fetchLeaderboard(sortKey, 1, sortDescending, true);
  }, [sortKey, sortDescending, guildId]);

  const loadMore = useCallback(() => {
    if (hasMore && !loading) {
      const nextPage = page + 1;
      setPage(nextPage);
      fetchLeaderboard(sortKey, nextPage, sortDescending);
    }
  }, [hasMore, loading, page, sortKey, sortDescending, fetchLeaderboard]);

  const renderSortOrderIcon = (active: boolean) => {
    if (!active) return null;
    return (
      <span style={{ marginLeft: 4 }}>
        {sortDescending ? "↓" : "↑"}
      </span>
    );
  };

  const handleSortClick = (key: SortKey) => {
    if (sortKey === key) {
      setSortDescending((prev) => !prev);
    } else {
      setSortKey(key);
      setSortDescending(true);
    }
  };

  const handleRetry = () => {
    fetchLeaderboard(sortKey, 1, sortDescending, true);
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
        {data.map((u) => (
          <tr
            key={u.userId}
            className="group transition"
          >
            <td className="px-2 py-1 align-middle bg-secondary rounded-l-2xl">
              <img
                src={getDiscordAvatarUrl(u.userId, u.avatarHash)}
                alt={u.username}
                className="rounded-full w-10 h-10"
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
            className={`px-3 py-1 rounded bg-secondary bg-hover ${sortKey === opt.key ? "selected" : ""}`}
            disabled={loading}
            style={{ display: "flex", alignItems: "center" }}
            aria-pressed={sortKey === opt.key}
          >
            {t(opt.label)}
            {renderSortOrderIcon(sortKey === opt.key)}
          </button>
        ))}
      </div>
      {error && (
        <div className="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded text-center">
          {error}
          <button
            className="ml-4 px-3 py-1 bg-red-200 hover:bg-red-300 rounded text-sm text-red-800"
            onClick={handleRetry}
            disabled={loading}
          >
            {t("leaderboard.retry")}
          </button>
        </div>
      )}

      {!error && (
        <InfiniteScroll
          dataLength={data.length}
          next={loadMore}
          hasMore={hasMore}
          loader={<h4 style={{ textAlign: 'center' }}>{t("loading")}</h4>}
          endMessage={
            <p style={{ textAlign: 'center' }}>
              <b>{t("theEnd")}</b>
            </p>
          }
        >
          <div>
            {isMobile ? (
              // Mobile: render as cards in a vertical list
              <div className="flex flex-col gap-2">
                {data.map(renderMobileCard)}
              </div>
            ) : (
              // Desktop: table layout
              renderDesktopTable()
            )}
          </div>
        </InfiniteScroll>
      )}
    </div>
  );
};

export default Leaderboard;
