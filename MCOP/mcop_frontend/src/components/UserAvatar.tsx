import { getDiscordAvatarUrl } from '../utils/avatarUtils';

interface UserAvatarProps {
  userId: string;
  avatarHash: string | null;
  username: string;
  size?: number;
  className?: string;
}

export const UserAvatar: React.FC<UserAvatarProps> = ({
  userId,
  avatarHash,
  username,
  size = 32,
  className = ''
}) => {
  const avatarUrl = getDiscordAvatarUrl(userId, avatarHash, size);

  return (
    <div className={`flex items-center ${className}`}>
      <img
        src={avatarUrl}
        alt={`${username}'s avatar`}
        className="rounded-full"
        style={{ width: size, height: size }}
      />
      <span className="ml-2 font-medium">{username}</span>
    </div>
  );
};