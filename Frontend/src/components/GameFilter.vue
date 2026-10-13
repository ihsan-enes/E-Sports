<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

interface GameChip {
  id: string;
  name: string;
  slug: string;
  badgeClass: string;
}

const chips: GameChip[] = [
  { id: 'all', name: 'Tümü', slug: '', badgeClass: 'hover:border-slate-500' },
  { id: 'lol', name: 'LoL', slug: 'lol', badgeClass: 'hover:border-blue-500' },
  { id: 'cs2', name: 'CS2', slug: 'cs2', badgeClass: 'hover:border-amber-500' },
  { id: 'valorant', name: 'Valorant', slug: 'valorant', badgeClass: 'hover:border-rose-500' }
];

const route = useRoute();
const router = useRouter();

const selectedSlug = computed<string>(() => {
  const queryVal = route.query.game;
  if (!queryVal || typeof queryVal !== 'string') return '';
  return queryVal.toLowerCase();
});

const selectGame = (slug: string) => {
  const currentQuery = { ...route.query };

  if (slug) {
    currentQuery.game = slug;
  } else {
    delete currentQuery.game;
  }

  router.push({
    query: currentQuery
  });
};
</script>

<template>
  <div class="flex flex-wrap items-center gap-2">
    <button
      v-for="chip in chips"
      :key="chip.id"
      type="button"
      @click="selectGame(chip.slug)"
      class="group relative inline-flex items-center gap-2 rounded-lg px-4 py-2 text-xs sm:text-sm font-semibold transition-all duration-200 border"
      :class="[
        chip.badgeClass,
        selectedSlug === chip.slug
          ? 'bg-slate-800 text-white border-brand-accent shadow-sm shadow-brand-accent/30'
          : 'bg-brand-surface text-slate-400 border-slate-800 hover:text-white'
      ]"
    >
      <span
        v-if="chip.slug === 'lol'"
        class="h-2 w-2 rounded-full"
        :class="selectedSlug === chip.slug ? 'bg-blue-400' : 'bg-blue-600'"
      ></span>
      <span
        v-else-if="chip.slug === 'cs2'"
        class="h-2 w-2 rounded-full"
        :class="selectedSlug === chip.slug ? 'bg-amber-400' : 'bg-amber-600'"
      ></span>
      <span
        v-else-if="chip.slug === 'valorant'"
        class="h-2 w-2 rounded-full"
        :class="selectedSlug === chip.slug ? 'bg-rose-400' : 'bg-rose-600'"
      ></span>
      <span
        v-else
        class="h-2 w-2 rounded-full"
        :class="selectedSlug === chip.slug ? 'bg-emerald-400' : 'bg-slate-600'"
      ></span>

      <span>{{ chip.name }}</span>
    </button>
  </div>
</template>
