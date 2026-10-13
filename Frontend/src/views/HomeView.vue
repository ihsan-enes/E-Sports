<script setup lang="ts">
</script>

<template>
  <section class="space-y-6">
    <div class="rounded-xl border border-slate-800 bg-brand-surface p-6 shadow-lg shadow-black/40">
      <div class="flex items-center justify-between border-b border-slate-800 pb-4">
        <div>
          <h1 class="text-2xl font-bold tracking-tight text-white">Canlı ve Yaklaşan Karşılaşmalar</h1>
          <p class="text-sm text-brand-muted">League of Legends, CS2 ve Valorant arenasından anlık maç sonuçları.</p>
        </div>
        <div class="flex items-center gap-2">
          <span class="relative flex h-3 w-3">
            <span class="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-75"></span>
            <span class="relative inline-flex h-3 w-3 rounded-full bg-emerald-500"></span>
          </span>
          <span class="text-xs font-semibold uppercase tracking-wider text-emerald-400">Canlı Takip Aktif</span>
        </div>
      </div>

      <div class="mt-8 rounded-lg border border-dashed border-slate-800 p-8 text-center">
        <p class="text-brand-muted">Karşılaşma verileri yükleniyor...</p>
      </div>
    </div>
  </section>
</template>
