export type MatchStatus = 0 | 1 | 2 | 3;
export const MatchStatusMap = {
  0: 'Scheduled',
  1: 'Live',
  2: 'Finished',
  3: 'Canceled'
} as const;

export type TournamentTier = 0 | 1 | 2 | 3 | 4 | 5 | 6;
export const TournamentTierMap = {
  0: 'Unknown',
  1: 'S',
  2: 'A',
  3: 'B',
  4: 'C',
  5: 'D',
  6: 'Unranked'
} as const;

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  instance?: string;
  [key: string]: unknown;
}

export interface GameDto {
  id: string;
  name: string;
  slug: string;
  iconUrl?: string | null;
}

export interface GameSummaryDto {
  id: string;
  name: string;
  slug: string;
  iconUrl?: string | null;
}

export interface TeamSummaryDto {
  id: string;
  name: string;
  slug: string;
  acronym?: string | null;
  country?: string | null;
  logoUrl?: string | null;
}

export interface TournamentSummaryDto {
  id: string;
  name: string;
  slug: string;
  tier: TournamentTier;
  leagueSlug?: string | null;
}

export interface MatchDto {
  id: string;
  externalId: string;
  gameSlug: string;
  gameName: string;
  tournament: TournamentSummaryDto;
  status: MatchStatus;
  scheduledAtUtc: string;
  startedAtUtc?: string | null;
  endedAtUtc?: string | null;
  bestOf: number;
  team1?: TeamSummaryDto | null;
  team2?: TeamSummaryDto | null;
  team1Score: number;
  team2Score: number;
  winnerTeamId?: string | null;
}

export interface MatchDetailDto {
  id: string;
  externalId: string;
  gameSlug: string;
  gameName: string;
  tournament: TournamentSummaryDto;
  status: MatchStatus;
  scheduledAtUtc: string;
  startedAtUtc?: string | null;
  endedAtUtc?: string | null;
  bestOf: number;
  team1?: TeamSummaryDto | null;
  team2?: TeamSummaryDto | null;
  team1Score: number;
  team2Score: number;
  winnerTeamId?: string | null;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface TournamentDto {
  id: string;
  externalId: string;
  gameSlug: string;
  gameName: string;
  name: string;
  slug: string;
  tier: TournamentTier;
  region?: string | null;
  country?: string | null;
  leagueSlug?: string | null;
  startDateUtc?: string | null;
  endDateUtc?: string | null;
  status: 'upcoming' | 'running' | 'finished' | string;
}

export interface TournamentDetailDto {
  id: string;
  externalId: string;
  game: GameSummaryDto;
  name: string;
  slug: string;
  tier: TournamentTier;
  region?: string | null;
  country?: string | null;
  leagueSlug?: string | null;
  startDateUtc?: string | null;
  endDateUtc?: string | null;
  status: 'upcoming' | 'running' | 'finished' | string;
  matchCount: number;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface TeamDto {
  id: string;
  externalId: string;
  game?: GameSummaryDto | null;
  name: string;
  slug: string;
  acronym?: string | null;
  country?: string | null;
  logoUrl?: string | null;
}

export interface PlayerSummaryDto {
  id: string;
  externalId: string;
  nickname: string;
  firstName?: string | null;
  lastName?: string | null;
  role?: string | null;
  country?: string | null;
  imageUrl?: string | null;
}

export interface TeamDetailDto {
  id: string;
  externalId: string;
  game?: GameSummaryDto | null;
  name: string;
  slug: string;
  acronym?: string | null;
  country?: string | null;
  logoUrl?: string | null;
  totalMatchesPlayed: number;
  totalWins: number;
  winRate: number;
  form: string[];
  roster: PlayerSummaryDto[];
  recentMatches: MatchDto[];
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface PlayerDetailDto {
  id: string;
  externalId: string;
  team?: TeamSummaryDto | null;
  game?: GameSummaryDto | null;
  nickname: string;
  firstName?: string | null;
  lastName?: string | null;
  role?: string | null;
  country?: string | null;
  imageUrl?: string | null;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}
