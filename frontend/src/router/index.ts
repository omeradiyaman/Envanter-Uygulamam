import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '../layouts/AppLayout.vue'
import DashboardView from '../views/DashboardView.vue'
import PersonnelView from '../views/PersonnelView.vue'
import PlaceholderView from '../views/PlaceholderView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '',
          name: 'dashboard',
          component: DashboardView,
          meta: { title: 'Dashboard' },
        },
        {
          path: 'cihazlar',
          name: 'devices',
          component: PlaceholderView,
          meta: { title: 'Cihazlar' },
        },
        {
          path: 'personeller',
          name: 'personnel',
          component: PersonnelView,
          meta: { title: 'Personeller' },
        },
        {
          path: 'envanterler',
          name: 'inventories',
          component: PlaceholderView,
          meta: { title: 'Envanterler' },
        },
        {
          path: 'stok',
          name: 'stock',
          component: PlaceholderView,
          meta: { title: 'Stok' },
        },
        {
          path: 'hurda-imha',
          name: 'scrap',
          component: PlaceholderView,
          meta: { title: 'Hurda / İmha' },
        },
        {
          path: 'raporlar',
          name: 'reports',
          component: PlaceholderView,
          meta: { title: 'Raporlar' },
        },
        {
          path: 'son-hareketler',
          name: 'activity',
          component: PlaceholderView,
          meta: { title: 'Son Hareketler' },
        },
        {
          path: 'kullanici-yonetimi',
          name: 'users',
          component: PlaceholderView,
          meta: { title: 'Kullanıcı Yönetimi' },
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

router.afterEach((to) => {
  document.title = `${String(to.meta.title ?? 'Dashboard')} | Envanter Sistemi`
})

export default router
