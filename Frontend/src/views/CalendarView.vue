<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { matchesApi, type MatchDto } from '@/api';
import GameFilter from '@/components/GameFilter.vue';
import MatchCard from '@/components/MatchCard.vue';
import {
  formatTurkishDayHeading,
  getIstanbulDayKey,
  getDateRangeUtc,
  type DateRangePreset
} from '@/utils/date';

interface DayGroup {
  dateKey: string;
  heading: string;
  matches: MatchDto[];
}

const route = useRoute();
const router = useRouter();

const selectedRange = computed<DateRangePreset>(() => {
  const range = route.query.range;
  if (range === 'next_week' || range === 'this_month' || range === 'all') {
    return range;
  }
  return 'this_week';
});

const currentGame = computed<string | undefined>(() => {
  const g = route.query.game;
  return typeof g === 'string' && g.trim() !== '' ? g.trim() : undefined;
});

const rangeOptions: { id: DateRangePreset; label: string }[] = [
  { id: 'this_week', label: 'Bu Hafta' },
  { id: 'next_week', label: 'Gelecek Hafta' },
  { id: 'this_month', label: 'Bu Ay' },
  { id: 'all', label: 'Tümü' }
];

const matches = ref<MatchDto[]>([]);
const currentPage = ref(1);
const totalPages = ref(1);
const totalCount = ref(0);
const pageSize = 18;

const isLoading = ref(true);
const isLoadingMore = ref(false);
const error = ref<string | null>(null);

let abortController: AbortController | null = null;

const selectRange = (range: DateRangePreset) => {
  const query = { ...route.query };
  if (range === 'this_week') {
    delete query.range;
  } else {
    query.range = range;
  }
  router.push({ query });
};

const fetchMatches = async (page: number, append = false) => {
  if (abortController) {
    abortController.abort();
  }
  abortController = new AbortController();

  if (append) {
    isLoadingMore.value = true;
  } else {
    isLoading.value = true;
  }
  error.value = null;

  try {
    const { from, to } = getDateRangeUtc(selectedRange.value);

    const response = await matchesApi.getUpcoming(
      {
        game: currentGame.value,
        from,
        to,
        page,
        pageSize
      },
      abortController.signal
    );

    if (append) {
      matches.value = [...matches.value, ...response.items];
    } else {
      matches.value = response.items;
    }

    currentPage.value = response.page;
    totalPages.value = response.totalPages;
    totalCount.value = response.totalCount;
  } catch (err: unknown) {
    if ((err as Error)?.name === 'AbortError') return;
    error.value = (err as Error)?.message || 'Maç takvimi yüklenirken bir sorun oluştu.';
  } finally {
    isLoading.value = false;
    isLoadingMore.value = false;
  }
};

const loadMore = () => {
  if (currentPage.value < totalPages.value && !isLoadingMore.value) {
    fetchMatches(currentPage.value + 1, true);
  }
};

// Maçları Europe/Istanbul saat dilimine göre gün bazında grupla
const groupedMatches = computed<DayGroup[]>(() => {
  const groups: Record<string, DayGroup> = {};

  for (const match of matches.value) {
    const key = getIstanbulDayKey(match.scheduledAtUtc);

    if (!groups[key]) {
      groups[key] = {
        dateKey: key,
        heading: formatTurkishDayHeading(match.scheduledAtUtc),
        matches: []
      };
    }

    groups[key].matches.push(match);
  }

  return Object.values(groups);
});

watch([currentGame, selectedRange], () => {
  fetchMatches(1, false);
});

onMounted(() => {
  fetchMatches(1, false);
});

onUnmounted(() => {
  if (abortController) abortController.abort();
});
</script>

<template>
  <div class="space-y-8">
    <!-- Sayfa Başlığı ve Oyun Filtresi -->
    <section class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between border-b border-slate-800 pb-6">
      <div>
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-white flex items-center gap-3">
          <svg class="h-7 w-7 text-cyan-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
            <line x1="16" y1="2" x2="16" y2="6"></line>
            <line x1="8" y1="2" x2="8" y2="6"></line>
            <line x1="3" y1="10" x2="21" y2="10"></line>
          </svg>
          Karşılaşma Takvimi
        </h1>
        <p class="mt-1 text-xs sm:text-sm text-brand-muted">
          Yaklaşan League of Legends, CS2 ve Valorant maçları takvimi (TSİ - Europe/Istanbul).
        </p>
      </div>

      <GameFilter />
    </section>

    <!-- Tarih Aralığı Seçici Sekmeleri -->
    <section class="flex flex-wrap items-center justify-between gap-4">
      <div class="inline-flex rounded-lg border border-slate-800 bg-brand-surface p-1">
        <button
          v-for="option in rangeOptions"
          :key="option.id"
          type="button"
          @click="selectRange(option.id)"
          class="rounded-md px-3.5 py-1.5 text-xs font-semibold transition-all"
          :class="[
            selectedRange === option.id
              ? 'bg-slate-800 text-white shadow-sm ring-1 ring-slate-700'
              : 'text-slate-400 hover:text-slate-200'
          ]"
        >
          {{ option.label }}
        </button>
      </div>

      <div v-if="!isLoading && totalCount > 0" class="text-xs text-brand-muted">
        Toplam <span class="font-semibold text-white">{{ totalCount }}</span> karşılaşma bulundu
      </div>
    </section>

    <!-- Yüklenme Durumu (İlk Sayfa) -->
    <div
      v-if="isLoading"
      class="space-y-8"
    >
      <div v-for="group in 2" :key="group" class="space-y-4">
        <div class="h-6 w-48 bg-slate-800/80 rounded animate-pulse"></div>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="i in 3"
            :key="i"
            class="h-28 rounded-xl border border-slate-800/80 bg-brand-surface p-4 animate-pulse space-y-3"
          >
            <div class="h-3 w-28 bg-slate-800 rounded"></div>
            <div class="h-8 bg-slate-800/60 rounded"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Hata Durumu -->
    <div
      v-else-if="error"
      class="rounded-xl border border-red-900/50 bg-red-950/20 p-8 text-center text-red-300"
    >
      <div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-red-950 border border-red-900">
        <svg class="h-6 w-6 text-red-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"></circle>
          <line x1="12" y1="8" x2="12" y2="12"></line>
          <line x1="12" y1="16" x2="12.01" y2="16"></line>
        </svg>
      </div>
      <h3 class="mt-3 text-base font-semibold text-red-200">Karşılaşmalar Alınamadı</h3>
      <p class="mt-1 text-xs text-red-300/80">{{ error }}</p>
      <button
        type="button"
        @click="fetchMatches(1, false)"
        class="mt-4 inline-flex items-center gap-1.5 rounded-lg bg-red-900/80 px-4 py-2 text-xs font-semibold text-white hover:bg-red-800 transition-colors"
      >
        Tekrar Dene
      </button>
    </div>

    <!-- Boş Durum -->
    <div
      v-else-if="matches.length === 0"
      class="rounded-xl border border-dashed border-slate-800 bg-brand-surface/40 p-12 text-center"
    >
      <div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-slate-800/70 text-slate-400">
        <svg class="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
          <line x1="16" y1="2" x2="16" y2="6"></line>
          <line x1="8" y1="2" x2="8" y2="6"></line>
          <line x1="3" y1="10" x2="21" y2="10"></line>
        </svg>
      </div>
      <h3 class="mt-4 text-base font-semibold text-slate-200">Bu Tarih Aralığında Maç Bulunmuyor</h3>
      <p class="mt-1 text-xs text-brand-muted max-w-md mx-auto">
        Seçilen tarih aralığı ve filtreye uygun planlanmış bir müsabaka bulunmamaktadır. Farklı bir tarih aralığı veya oyun deneyebilirsiniz.
      </p>
    </div>

    <!-- Günlere Göre Gruplanmış Maç Listesi -->
    <div v-else class="space-y-10">
      <section
        v-for="group in groupedMatches"
        :key="group.dateKey"
        class="space-y-4"
      >
        <!-- Gün Başlığı (Örn: 14 Ekim Salı) -->
        <div class="flex items-center gap-3">
          <h2 class="text-base sm:text-lg font-bold tracking-tight text-white flex items-center gap-2">
            <span class="h-2 w-2 rounded-full bg-brand-accent"></span>
            {{ group.heading }}
          </h2>
          <div class="h-px flex-1 bg-gradient-to-r from-slate-800 to-transparent"></div>
          <span class="rounded bg-slate-900 border border-slate-800 px-2 py-0.5 text-[11px] font-semibold text-brand-muted">
            {{ group.matches.length }} maç
          </span>
        </div>

        <!-- Günün Maçları -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <MatchCard
            v-for="match in group.matches"
            :key="match.id"
            :match="match"
          />
        </div>
      </section>

      <!-- Daha Fazla Yükle (Load More) -->
      <div
        v-if="currentPage < totalPages"
        class="flex justify-center pt-4"
      >
        <button
          type="button"
          @click="loadMore"
          :disabled="isLoadingMore"
          class="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-brand-surface px-6 py-2.5 text-sm font-semibold text-slate-200 hover:bg-slate-800 hover:text-white hover:border-slate-600 transition-all shadow-md shadow-black/40 disabled:opacity-50"
        >
          <svg
            v-if="isLoadingMore"
            class="h-4 w-4 animate-spin text-cyan-400"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          <span>{{ isLoadingMore ? 'Yükleniyor...' : 'Daha Fazla Karşılaşma Yükle' }}</span>
        </button>
      </div>
    </div>
  </div>
</template>
