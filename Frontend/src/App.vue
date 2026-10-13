<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router';
</script>

<template>
  <div class="flex min-h-screen flex-col bg-brand-dark">
    <!-- Header -->
    <header class="sticky top-0 z-50 border-b border-slate-800/80 bg-brand-dark/90 backdrop-blur-md">
      <div class="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <div class="flex items-center gap-8">
          <RouterLink to="/" class="flex items-center gap-2 group">
            <div class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-tr from-brand-accent to-red-600 shadow-md shadow-brand-accent/20">
              <span class="text-lg font-black tracking-wider text-white">E</span>
            </div>
            <div class="flex flex-col">
              <span class="font-extrabold text-lg tracking-wider text-white group-hover:text-red-400 transition-colors">
                E-SPOR
              </span>
              <span class="text-[10px] uppercase font-semibold text-brand-muted -mt-1 tracking-widest">
                Türkiye & Global
              </span>
            </div>
          </RouterLink>

          <!-- Navigasyon -->
          <nav class="hidden md:flex items-center gap-1">
            <RouterLink
              to="/"
              class="rounded-md px-3 py-2 text-sm font-medium transition-colors"
              :class="$route.name === 'home' ? 'bg-brand-surface text-white shadow-sm' : 'text-slate-400 hover:text-white hover:bg-brand-surface/50'"
            >
              Ana Sayfa
            </RouterLink>
            <RouterLink
              to="/calendar"
              class="rounded-md px-3 py-2 text-sm font-medium transition-colors"
              :class="$route.name === 'calendar' ? 'bg-brand-surface text-white shadow-sm' : 'text-slate-400 hover:text-white hover:bg-brand-surface/50'"
            >
              Takvim
            </RouterLink>
          </nav>
        </div>

        <div class="flex items-center gap-4">
          <div class="hidden sm:flex items-center gap-2 rounded-full border border-slate-800 bg-brand-surface px-3 py-1 text-xs text-brand-muted">
            <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
            <span>LoL • CS2 • Valorant</span>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="mx-auto w-full max-w-7xl flex-1 px-4 py-8 sm:px-6 lg:px-8">
      <RouterView />
    </main>

    <!-- Footer -->
    <footer class="border-t border-slate-800/80 bg-brand-surface/40 py-6 text-center text-xs text-brand-muted">
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <p>© 2025 E-Spor Takipçisi. Tüm hakları saklıdır.</p>
      </div>
    </footer>
  </div>
</template>
