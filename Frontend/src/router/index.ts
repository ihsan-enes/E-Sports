import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '@/views/HomeView.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/calendar',
      name: 'calendar',
      component: () => import('@/views/CalendarView.vue')
    },
    {
      path: '/tournaments/:id',
      name: 'tournament-detail',
      component: () => import('@/views/TournamentDetailView.vue'),
      props: true
    },
    {
      path: '/teams/:id',
      name: 'team-detail',
      component: () => import('@/views/TeamDetailView.vue'),
      props: true
    },
    {
      path: '/players/:id',
      name: 'player-detail',
      component: () => import('@/views/PlayerDetailView.vue'),
      props: true
    }
  ]
});

export default router;
