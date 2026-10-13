import { request } from './client';
import type {
  GameDto,
  MatchDto,
  MatchDetailDto,
  TournamentDto,
  TournamentDetailDto,
  TeamDto,
  TeamDetailDto,
  PlayerDetailDto,
  PagedResult
} from './types';

export * from './types';
export * from './client';

export const gamesApi = {
  getAll: (signal?: AbortSignal) =>
    request<GameDto[]>('/api/games', { signal })
};

export const matchesApi = {
  getLive: (game?: string, signal?: AbortSignal) => {
    const params = new URLSearchParams();
    if (game) params.append('game', game);
    const queryString = params.toString() ? `?${params.toString()}` : '';
    return request<MatchDto[]>(`/api/matches/live${queryString}`, { signal });
  },

  getUpcoming: (
    params?: { game?: string; from?: string; to?: string; page?: number; pageSize?: number },
    signal?: AbortSignal
  ) => {
    const query = new URLSearchParams();
    if (params?.game) query.append('game', params.game);
    if (params?.from) query.append('from', params.from);
    if (params?.to) query.append('to', params.to);
    if (params?.page) query.append('page', params.page.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    const queryString = query.toString() ? `?${query.toString()}` : '';
    return request<PagedResult<MatchDto>>(`/api/matches/upcoming${queryString}`, { signal });
  },

  getById: (id: string, signal?: AbortSignal) =>
    request<MatchDetailDto>(`/api/matches/${id}`, { signal })
};

export const tournamentsApi = {
  getAll: (
    params?: { game?: string; status?: string; page?: number; pageSize?: number },
    signal?: AbortSignal
  ) => {
    const query = new URLSearchParams();
    if (params?.game) query.append('game', params.game);
    if (params?.status) query.append('status', params.status);
    if (params?.page) query.append('page', params.page.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    const queryString = query.toString() ? `?${query.toString()}` : '';
    return request<PagedResult<TournamentDto>>(`/api/tournaments${queryString}`, { signal });
  },

  getById: (id: string, signal?: AbortSignal) =>
    request<TournamentDetailDto>(`/api/tournaments/${id}`, { signal })
};

export const teamsApi = {
  getAll: (
    params?: { game?: string; search?: string; page?: number; pageSize?: number },
    signal?: AbortSignal
  ) => {
    const query = new URLSearchParams();
    if (params?.game) query.append('game', params.game);
    if (params?.search) query.append('search', params.search);
    if (params?.page) query.append('page', params.page.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    const queryString = query.toString() ? `?${query.toString()}` : '';
    return request<PagedResult<TeamDto>>(`/api/teams${queryString}`, { signal });
  },

  getById: (id: string, signal?: AbortSignal) =>
    request<TeamDetailDto>(`/api/teams/${id}`, { signal })
};

export const playersApi = {
  getById: (id: string, signal?: AbortSignal) =>
    request<PlayerDetailDto>(`/api/players/${id}`, { signal })
};
