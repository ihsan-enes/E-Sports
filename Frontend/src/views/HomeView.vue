<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import { matchesApi, type MatchDto } from '@/api';
import GameFilter from '@/components/GameFilter.vue';
import MatchCard from '@/components/MatchCard.vue';

const route = useRoute();

const currentGame = computed<string | undefined>(() => {
  const g = route.query.game;
  return typeof g === 'string' && g.trim() !== '' ? g.trim() : undefined;
});

// Canlı Maçlar Durumu
const liveMatches = ref<MatchDto[]>([]);
const isLiveLoading = ref(true);
const liveError = ref<string | null>(null);

// Yaklaşan Maçlar Durumu
const upcomingMatches = ref<MatchDto[]>([]);
const isUpcomingLoading = ref(true);
const upcomingError = ref<string | null>(null);

let livePollingTimer: ReturnType<typeof setInterval> | null = null;
let liveAbortController: AbortController | null = null;
let upcomingAbortController: AbortController | null = null;

// Canlı Maçları Getir
const fetchLiveMatches = async (silent = false) => {
  if (liveAbortController) {
    liveAbortController.abort();
  }
  liveAbortController = new AbortController();

  if (!silent) {
    isLiveLoading.value = true;
  }
  liveError.value = null;

  try {
    const data = await matchesApi.getLive(currentGame.value, liveAbortController.signal);
    liveMatches.value = data;
  } catch (err: unknown) {
    if ((err as Error)?.name === 'AbortError') return;
    liveError.value = (err as Error)?.message || 'Canlı karşılaşmalar yüklenirken bir sorun meydana geldi.';
  } finally {
    isLiveLoading.value = false;
  }
};

// Yaklaşan Maçları Getir
const fetchUpcomingMatches = async () => {
  if (upcomingAbortController) {
    upcomingAbortController.abort();
  }
  upcomingAbortController = new AbortController();

  isUpcomingLoading.value = true;
  upcomingError.value = null;

  try {
    const res = await matchesApi.getUpcoming(
      { game: currentGame.value, page: 1, pageSize: 12 },
      upcomingAbortController.signal
    );
    upcomingMatches.value = res.items;
  } catch (err: unknown) {
    if ((err as Error)?.name === 'AbortError') return;
    upcomingError.value = (err as Error)?.message || 'Yaklaşan karşılaşmalar yüklenirken bir sorun meydana geldi.';
  } finally {
    isUpcomingLoading.value = false;
  }
};

const refreshAll = () => {
  fetchLiveMatches(false);
  fetchUpcomingMatches();
};

const startPolling = () => {
  stopPolling();
  livePollingTimer = setInterval(() => {
    fetchLiveMatches(true);
  }, 30000);
};

const stopPolling = () => {
  if (livePollingTimer) {
    clearInterval(livePollingTimer);
    livePollingTimer = null;
  }
};

watch(currentGame, () => {
  refreshAll();
  startPolling();
});

onMounted(() => {
  refreshAll();
  startPolling();
});

onUnmounted(() => {
  stopPolling();
  if (liveAbortController) liveAbortController.abort();
  if (upcomingAbortController) upcomingAbortController.abort();
});
</script>

<template>
  <div class="space-y-10">
    <!-- Üst Başlık ve Oyun Filtresi -->
    <section class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between border-b border-slate-800 pb-6">
      <div>
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-white">
          E-Spor Karşılaşmaları
        </h1>
        <p class="mt-1 text-xs sm:text-sm text-brand-muted">
          Türkiye ve dünyadan seçkin League of Legends, CS2 ve Valorant maçları.
        </p>
      </div>

      <GameFilter />
    </section>

    <!-- 1. Bölüm: Canlı Maçlar -->
    <section class="space-y-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2.5">
          <span class="relative flex h-3 w-3">
            <span class="absolute inline-flex h-full w-full animate-ping rounded-full bg-red-400 opacity-75"></span>
            <span class="relative inline-flex h-3 w-3 rounded-full bg-red-500"></span>
          </span>
          <h2 class="text-xl font-bold tracking-tight text-white">Canlı Maçlar</h2>
          <span
            v-if="liveMatches.length > 0"
            class="rounded-full bg-red-950 px-2 py-0.5 text-xs font-bold text-red-400 border border-red-800/70"
          >
            {{ liveMatches.length }}
          </span>
        </div>

        <button
          type="button"
          @click="fetchLiveMatches(false)"
          class="inline-flex items-center gap-1.5 rounded-lg border border-slate-800 bg-brand-surface px-3 py-1.5 text-xs font-semibold text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
          :disabled="isLiveLoading"
        >
          <svg
            class="h-3.5 w-3.5"
            :class="{ 'animate-spin': isLiveLoading }"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Yenile
        </button>
      </div>

      <!-- Canlı Yükleniyor Durumu -->
      <div
        v-if="isLiveLoading && liveMatches.length === 0"
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
      >
        <div
          v-for="i in 3"
          :key="i"
          class="h-28 rounded-xl border border-slate-800/80 bg-brand-surface p-4 animate-pulse space-y-3"
        >
          <div class="h-3 w-24 bg-slate-800 rounded"></div>
          <div class="h-8 bg-slate-800/60 rounded"></div>
        </div>
      </div>

      <!-- Canlı Hata Durumu -->
      <div
        v-else-if="liveError"
        class="rounded-xl border border-red-900/50 bg-red-950/20 p-6 text-center text-red-300"
      >
        <p class="font-medium text-sm">{{ liveError }}</p>
        <button
          type="button"
          @click="fetchLiveMatches(false)"
          class="mt-3 inline-flex items-center gap-1.5 rounded-md bg-red-900/60 px-3 py-1 text-xs font-semibold text-white hover:bg-red-800"
        >
          Tekrar Dene
        </button>
      </div>

      <!-- Canlı Boş Durumu -->
      <div
        v-else-if="liveMatches.length === 0"
        class="rounded-xl border border-dashed border-slate-800 bg-brand-surface/50 p-8 text-center"
      >
        <div class="mx-auto flex h-10 w-10 items-center justify-center rounded-full bg-slate-800/70 text-slate-400">
          <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <path d="m4.93 4.93 14.14 14.14"></path>
          </svg>
        </div>
        <h3 class="mt-3 text-sm font-semibold text-slate-200">Şu Anda Canlı Maç Bulunmuyor</h3>
        <p class="mt-1 text-xs text-brand-muted">
          Seçili kategoride devam eden bir karşılaşma yok. Yaklaşan maçlar takvimine göz atabilirsiniz.
        </p>
      </div>

      <!-- Canlı Liste -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <MatchCard
          v-for="match in liveMatches"
          :key="match.id"
          :match="match"
        />
      </div>
    </section>

    <!-- 2. Bölüm: Yaklaşan Maçlar -->
    <section class="space-y-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2.5">
          <svg class="h-5 w-5 text-cyan-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
            <line x1="16" y1="2" x2="16" y2="6"></line>
            <line x1="8" y1="2" x2="8" y2="6"></line>
            <line x1="3" y1="10" x2="21" y2="10"></line>
          </svg>
          <h2 class="text-xl font-bold tracking-tight text-white">Yaklaşan Maçlar</h2>
        </div>

        <RouterLink
          to="/calendar"
          class="text-xs font-semibold text-cyan-400 hover:text-cyan-300 transition-colors"
        >
          Tüm Takvimi Gör &rarr;
        </RouterLink>
      </div>

      <!-- Yaklaşan Yükleniyor Durumu -->
      <div
        v-if="isUpcomingLoading"
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
      >
        <div
          v-for="i in 6"
          :key="i"
          class="h-28 rounded-xl border border-slate-800/80 bg-brand-surface p-4 animate-pulse space-y-3"
        >
          <div class="h-3 w-28 bg-slate-800 rounded"></div>
          <div class="h-8 bg-slate-800/60 rounded"></div>
        </div>
      </div>

      <!-- Yaklaşan Hata Durumu -->
      <div
        v-else-if="upcomingError"
        class="rounded-xl border border-red-900/50 bg-red-950/20 p-6 text-center text-red-300"
      >
        <p class="font-medium text-sm">{{ upcomingError }}</p>
        <button
          type="button"
          @click="fetchUpcomingMatches"
          class="mt-3 inline-flex items-center gap-1.5 rounded-md bg-red-900/60 px-3 py-1 text-xs font-semibold text-white hover:bg-red-800"
        >
          Tekrar Dene
        </button>
      </div>

      <!-- Yaklaşan Boş Durumu -->
      <div
        v-else-if="upcomingMatches.length === 0"
        class="rounded-xl border border-dashed border-slate-800 bg-brand-surface/50 p-8 text-center"
      >
        <div class="mx-auto flex h-10 w-10 items-center justify-center rounded-full bg-slate-800/70 text-slate-400">
          <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="12"></line>
            <line x1="12" y1="16" x2="12.01" y2="16"></line>
          </svg>
        </div>
        <h3 class="mt-3 text-sm font-semibold text-slate-200">Planlanmış Maç Bulunamadı</h3>
        <p class="mt-1 text-xs text-brand-muted">
          Yakın gelecekte planlanmış karşılaşma bulunmuyor. Farklı bir oyun kategorisi seçebilirsiniz.
        </p>
      </div>

      <!-- Yaklaşan Liste -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <MatchCard
          v-for="match in upcomingMatches"
          :key="match.id"
          :match="match"
        />
      </div>
    </section>
  </div>
</template>
