<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import type { MatchDto } from '@/api/types';
import { formatMatchTime, formatTimeOnly } from '@/utils/date';

const props = defineProps<{
  match: MatchDto;
}>();

const isLive = computed(() => props.match.status === 1);

const gameBadgeColor = computed(() => {
  const slug = props.match.gameSlug.toLowerCase();
  if (slug.includes('lol') || slug.includes('league')) return 'border-blue-900 bg-blue-950/60 text-blue-400';
  if (slug.includes('cs')) return 'border-amber-900 bg-amber-950/60 text-amber-400';
  if (slug.includes('valorant')) return 'border-rose-900 bg-rose-950/60 text-rose-400';
  return 'border-slate-800 bg-slate-900 text-slate-400';
});
</script>

<template>
  <div
    class="relative flex flex-col justify-between overflow-hidden rounded-xl border bg-brand-surface transition-all duration-200 hover:border-slate-700 hover:shadow-lg hover:shadow-black/60"
    :class="isLive ? 'border-red-900/60 ring-1 ring-red-500/20' : 'border-slate-800'"
  >
    <!-- Header: Oyun ve Turnuva Bilgisi -->
    <div class="flex items-center justify-between border-b border-slate-800/80 bg-brand-dark/40 px-4 py-2.5 text-xs">
      <div class="flex items-center gap-2 overflow-hidden pr-2">
        <span
          class="inline-flex shrink-0 items-center rounded px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider border"
          :class="gameBadgeColor"
        >
          {{ match.gameName }}
        </span>
        <RouterLink
          :to="`/tournaments/${match.tournament.id}`"
          class="truncate font-medium text-slate-400 hover:text-white transition-colors"
          :title="match.tournament.name"
        >
          {{ match.tournament.name }}
        </RouterLink>
      </div>

      <div class="flex shrink-0 items-center gap-2">
        <span class="rounded bg-slate-800/80 px-1.5 py-0.5 text-[10px] font-semibold text-slate-400">
          BO{{ match.bestOf }}
        </span>
        <span
          v-if="isLive"
          class="inline-flex items-center gap-1 rounded bg-red-950 px-2 py-0.5 text-[10px] font-bold tracking-wider uppercase text-red-400 border border-red-800"
        >
          <span class="h-1.5 w-1.5 rounded-full bg-red-500 animate-pulse"></span>
          CANLI
        </span>
      </div>
    </div>

    <!-- Match Body: Takımlar ve Skor/Saat -->
    <div class="grid grid-cols-[1fr_auto_1fr] items-center gap-2 p-4">
      <!-- Takım 1 -->
      <div class="flex items-center gap-3 overflow-hidden">
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-slate-900/90 border border-slate-800 p-1">
          <img
            v-if="match.team1?.logoUrl"
            :src="match.team1.logoUrl"
            :alt="match.team1.name"
            class="h-full w-full object-contain"
            loading="lazy"
          />
          <span v-else class="text-xs font-bold text-slate-500">
            {{ match.team1?.acronym || 'T1' }}
          </span>
        </div>

        <div class="min-w-0">
          <RouterLink
            v-if="match.team1"
            :to="`/teams/${match.team1.id}`"
            class="block truncate text-sm font-semibold text-slate-100 hover:text-brand-accent transition-colors"
          >
            {{ match.team1.name }}
          </RouterLink>
          <span v-else class="block truncate text-sm font-semibold text-slate-500 italic">
            Belirlenmedi
          </span>

          <div class="flex items-center gap-1 text-[11px] text-brand-muted">
            <span v-if="match.team1?.country" class="uppercase text-[10px] bg-slate-800 px-1 rounded">
              {{ match.team1.country }}
            </span>
            <span v-if="match.team1?.acronym" class="truncate">
              {{ match.team1.acronym }}
            </span>
          </div>
        </div>
      </div>

      <!-- Skor ya da Başlangıç Saati -->
      <div class="flex flex-col items-center justify-center px-2 text-center">
        <template v-if="isLive || match.status === 2">
          <div class="flex items-center gap-1.5 font-mono text-xl font-black">
            <span :class="match.team1Score > match.team2Score ? 'text-emerald-400' : 'text-white'">
              {{ match.team1Score }}
            </span>
            <span class="text-slate-600">:</span>
            <span :class="match.team2Score > match.team1Score ? 'text-emerald-400' : 'text-white'">
              {{ match.team2Score }}
            </span>
          </div>
          <span v-if="isLive" class="mt-0.5 text-[10px] text-red-400 font-medium">
            Oynanıyor
          </span>
        </template>

        <template v-else>
          <div class="font-mono text-sm font-bold text-cyan-400">
            {{ formatTimeOnly(match.scheduledAtUtc) }}
          </div>
          <div class="text-[10px] text-slate-500 whitespace-nowrap">
            {{ formatMatchTime(match.scheduledAtUtc).split(' ')[0] }}
          </div>
        </template>
      </div>

      <!-- Takım 2 -->
      <div class="flex flex-row-reverse items-center gap-3 overflow-hidden text-right">
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-slate-900/90 border border-slate-800 p-1">
          <img
            v-if="match.team2?.logoUrl"
            :src="match.team2.logoUrl"
            :alt="match.team2.name"
            class="h-full w-full object-contain"
            loading="lazy"
          />
          <span v-else class="text-xs font-bold text-slate-500">
            {{ match.team2?.acronym || 'T2' }}
          </span>
        </div>

        <div class="min-w-0">
          <RouterLink
            v-if="match.team2"
            :to="`/teams/${match.team2.id}`"
            class="block truncate text-sm font-semibold text-slate-100 hover:text-brand-accent transition-colors"
          >
            {{ match.team2.name }}
          </RouterLink>
          <span v-else class="block truncate text-sm font-semibold text-slate-500 italic">
            Belirlenmedi
          </span>

          <div class="flex flex-row-reverse items-center gap-1 text-[11px] text-brand-muted">
            <span v-if="match.team2?.country" class="uppercase text-[10px] bg-slate-800 px-1 rounded">
              {{ match.team2.country }}
            </span>
            <span v-if="match.team2?.acronym" class="truncate">
              {{ match.team2.acronym }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
