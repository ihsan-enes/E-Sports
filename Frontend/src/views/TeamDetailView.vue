<script setup lang="ts">
defineProps<{
  id: string;
}>();
</script>

<template>
  <section class="space-y-6">
    <div class="rounded-xl border border-slate-800 bg-brand-surface p-6 shadow-lg shadow-black/40">
      <h1 class="text-2xl font-bold tracking-tight text-white">Takım Profili</h1>
      <p class="mt-1 text-sm text-brand-muted">Takım ID: <span class="font-mono text-cyan-400">{{ id }}</span></p>

      <div class="mt-8 rounded-lg border border-dashed border-slate-800 p-8 text-center">
        <p class="text-brand-muted">Takım kadrosu, kazanma oranı ve maç geçmişi bu alanda görüntülenecektir.</p>
      </div>
    </div>
  </section>
</template>
