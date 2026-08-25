<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import ThemeToggle from './ThemeToggle.vue'
import { useRoute, useRouter } from 'vue-router'
import { authStore } from '../stores/authStore'
import { useSidebar } from '../composables/useSidebar'

defineProps<{
  title: string
}>()

const emit = defineEmits<{
  toggleSidebar: []
}>()
const router = useRouter()
const route = useRoute()
const { toggleCollapsed } = useSidebar()
const globalSearch = ref('')
const searchInput = ref<HTMLInputElement | null>(null)
const today = new Intl.DateTimeFormat('tr-TR', { day: '2-digit', month: 'short', year: 'numeric', weekday: 'short' }).format(new Date())
const routeIcons: Record<string, string> = {
  dashboard: 'bi-grid-1x2-fill',
  'device-create': 'bi-plus-circle-fill',
  'device-detail': 'bi-pc-display-horizontal',
  'device-edit': 'bi-pencil-square',
  'device-history': 'bi-clock-history',
  'device-qr-detail': 'bi-pc-display',
  personnel: 'bi-people-fill',
  'personnel-create': 'bi-person-plus-fill',
  'personnel-detail': 'bi-person-workspace',
  'personnel-edit': 'bi-person-fill-gear',
  'personnel-documents': 'bi-folder-fill',
  'personnel-history': 'bi-clock-history',
  inventories: 'bi-hdd-network-fill',
  'inventory-management': 'bi-sliders',
  'inventory-table-editor': 'bi-table',
  assignments: 'bi-person-check-fill',
  'assignment-create': 'bi-person-check-fill',
  'active-assignments': 'bi-check2-circle',
  'assignment-history': 'bi-clock-history',
  excel: 'bi-file-earmark-spreadsheet-fill',
  'excel-import': 'bi-file-earmark-arrow-up-fill',
  'excel-export': 'bi-file-earmark-arrow-down-fill',
  'label-qr-hub': 'bi-qr-code-scan',
  'bulk-label': 'bi-tags-fill',
  'bulk-qr': 'bi-qr-code',
  reports: 'bi-bar-chart-fill',
  activity: 'bi-clock-history',
  'activity-detail': 'bi-clock-history',
  users: 'bi-shield-lock-fill',
}
const pageIcon = computed(() => routeIcons[String(route.name)] ?? 'bi-grid-1x2-fill')
const pageIconColor = computed(() => {
  const name = String(route.name)
  if (name.startsWith('personnel')) return '#3b82f6'
  if (name.startsWith('excel')) return '#10b981'
  if (name.includes('assignment') || name === 'assignments') return '#22c55e'
  if (name.includes('qr') || name.includes('label')) return '#06b6d4'
  if (name === 'reports') return '#8b5cf6'
  if (name.includes('activity')) return '#f59e0b'
  if (name === 'users') return '#ef4444'
  return '#6d74f7'
})
const userInitials = computed(() => {
  const name = authStore.state.user?.fullName?.trim()
  if (!name) return 'U'
  return name.split(/\s+/).slice(0, 2).map(part => part[0]).join('').toLocaleUpperCase('tr-TR')
})
async function logout() { await authStore.logout(); await router.replace('/login') }
function submitSearch() { const query = globalSearch.value.trim(); if (query) void router.push({ name: 'inventories', query: { q: query } }) }
function toggleNavigation() { if (window.innerWidth >= 992) toggleCollapsed(); else emit('toggleSidebar') }
function onShortcut(event: KeyboardEvent) { if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); searchInput.value?.focus() } }
onMounted(() => window.addEventListener('keydown', onShortcut))
onBeforeUnmount(() => window.removeEventListener('keydown', onShortcut))
</script>

<template>
  <header class="app-topbar d-flex align-items-center justify-content-between">
    <div class="d-flex align-items-center gap-2 gap-sm-3 min-w-0">
      <button
        type="button"
        class="btn btn-light border topbar-menu-btn"
        aria-label="Menüyü aç veya daralt"
        @click="toggleNavigation"
      >
        <i class="bi bi-list" aria-hidden="true"></i>
      </button>
      <span class="topbar-page-icon d-none d-sm-inline-flex align-items-center justify-content-center" :style="{ '--page-icon-color': pageIconColor }">
        <i :class="['bi', pageIcon]" aria-hidden="true"></i>
      </span>
      <div class="min-w-0"><h1 class="topbar-title mb-0">{{ title }}</h1></div>
    </div>

    <div class="topbar-tools d-flex align-items-center gap-2 gap-sm-3">
      <form class="topbar-global-search d-none d-lg-flex" role="search" @submit.prevent="submitSearch">
        <i class="bi bi-search"></i><input ref="searchInput" v-model="globalSearch" type="search" placeholder="Hızlı ara... (cihaz, marka, seri no, personel)" aria-label="Uygulamada hızlı ara"><kbd>Ctrl K</kbd>
      </form>
      <span class="topbar-date d-none d-xl-inline-flex"><i class="bi bi-calendar3"></i>{{ today }}</span>
      <ThemeToggle />
      <details class="topbar-user-menu">
        <summary class="topbar-user d-flex align-items-center gap-2" aria-label="Kullanıcı menüsü"><span class="topbar-avatar d-inline-flex align-items-center justify-content-center">{{ userInitials }}</span><div class="d-none d-sm-block"><div class="topbar-user-name">{{ authStore.state.user?.fullName }}</div><div class="topbar-user-role">{{ authStore.state.user?.role }}</div></div><i class="bi bi-chevron-down topbar-user-chevron"></i></summary>
        <div class="topbar-user-dropdown"><div class="topbar-user-dropdown__head"><strong>{{ authStore.state.user?.fullName }}</strong><small>{{ authStore.state.user?.role }}</small></div><RouterLink v-if="authStore.isAdmin.value" to="/kullanici-yonetimi"><i class="bi bi-shield-lock"></i>Kullanıcı Yönetimi</RouterLink><button type="button" @click="logout"><i class="bi bi-box-arrow-right"></i>Çıkış Yap</button></div>
      </details>
    </div>
  </header>
</template>

<style scoped>
.app-topbar {
  position: sticky;
  z-index: 1020;
  top: 0;
  min-height: var(--topbar-height);
  padding: 0.65rem 1.75rem;
  border-bottom: 1px solid var(--topbar-border);
  background: var(--topbar-bg);
  backdrop-filter: blur(20px) saturate(1.25);
  box-shadow: 0 8px 32px rgb(15 23 42 / 5%);
}

.topbar-page-icon {
  position: relative;
  width: 38px;
  height: 38px;
  flex: 0 0 38px;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--page-icon-color) 62%, #fff);
  border-radius: 11px;
  color: #fff;
  background: linear-gradient(145deg, color-mix(in srgb, var(--page-icon-color) 78%, #fff), var(--page-icon-color));
  box-shadow: 0 7px 18px color-mix(in srgb, var(--page-icon-color) 25%, transparent), inset 0 1px rgb(255 255 255 / 24%);
}

.topbar-page-icon::after {
  position: absolute;
  inset: 0;
  content: '';
  pointer-events: none;
  background: linear-gradient(135deg, rgb(255 255 255 / 20%), transparent 48%);
}

.topbar-page-icon i {
  position: relative;
  z-index: 1;
}

.topbar-menu-btn {
  width: 38px;
  height: 38px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.topbar-title {
  font-size: var(--font-size-page-title);
  font-weight: var(--font-weight-semibold);
  color: var(--text-heading);
  line-height: var(--line-height-tight);
}

.topbar-subtitle {
  font-size: var(--font-size-label);
  color: var(--text-muted);
}

.topbar-avatar {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-full);
  color: #fff;
  background: linear-gradient(135deg, var(--primary), #7c3aed);
  box-shadow: 0 4px 14px var(--primary-glow);
  font-size: 0.7rem;
  font-weight: var(--font-weight-bold);
}

.topbar-user-name {
  font-size: var(--font-size-secondary);
  font-weight: var(--font-weight-semibold);
  color: var(--text-heading);
  line-height: 1.2;
}

.topbar-user-role {
  font-size: var(--font-size-label);
  color: var(--text-muted);
  line-height: 1.2;
}

@media (max-width: 991.98px) {
  .app-topbar {
    padding: 0.6rem 1rem;
  }
}

.topbar-user {
  padding: 0.3rem 0.75rem 0.3rem 0.3rem;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-full);
  background: var(--surface-muted);
}
.topbar-global-search { position: relative; align-items: center; width: min(360px, 28vw); }.topbar-global-search i { position: absolute; left: .75rem; color: var(--text-muted); }.topbar-global-search input { width: 100%; min-height: 40px; padding: .55rem 3.6rem .55rem 2.25rem; border: 1px solid var(--border-color); border-radius: 12px; outline: 0; color: var(--text-primary); background: var(--surface-muted); font-size: .72rem; transition:width .2s ease,border-color .2s ease,box-shadow .2s ease }.topbar-global-search input:focus { border-color: var(--focus-border); box-shadow: var(--focus-ring); }.topbar-global-search kbd{position:absolute;right:.55rem;padding:.18rem .34rem;border:1px solid var(--border-color);border-radius:5px;color:var(--text-muted);background:var(--surface);box-shadow:none;font:600 .55rem var(--font-family)}.topbar-date { align-items: center; gap: .45rem; min-height: 40px; padding: .5rem .75rem; border: 1px solid var(--border-color); border-radius: 999px; color: var(--text-secondary); background: var(--surface-muted); font-size: .68rem; white-space: nowrap; }.topbar-user-menu { position: relative; }.topbar-user-menu summary { list-style: none; cursor: pointer; }.topbar-user-menu summary::-webkit-details-marker { display: none; }.topbar-user-chevron { color: var(--text-muted); font-size: .62rem; }.topbar-user-menu[open] .topbar-user-chevron { transform: rotate(180deg); }.topbar-user-dropdown { position: absolute; z-index: 1100; top: calc(100% + .55rem); right: 0; width: 220px; padding: .45rem; border: 1px solid var(--border-color); border-radius: 13px; background: var(--surface-elevated); box-shadow: var(--shadow-lg); }.topbar-user-dropdown__head { padding: .55rem .65rem .7rem; border-bottom: 1px solid var(--border-color); }.topbar-user-dropdown__head strong, .topbar-user-dropdown__head small { display: block; }.topbar-user-dropdown__head strong { color: var(--text-heading); font-size: .75rem; }.topbar-user-dropdown__head small { color: var(--text-muted); font-size: .65rem; }.topbar-user-dropdown a, .topbar-user-dropdown button { display: flex; align-items: center; gap: .55rem; width: 100%; margin-top: .25rem; padding: .55rem .65rem; border: 0; border-radius: 8px; color: var(--text-secondary); background: transparent; font-size: .7rem; text-decoration: none; }.topbar-user-dropdown a:hover { color: var(--primary); background: var(--primary-subtle); }.topbar-user-dropdown button:hover { color: var(--danger); background: var(--danger-subtle); }

.topbar-logout {
  border-radius: var(--radius-md);
}

@media (max-width: 575.98px) {
  .app-topbar {
    gap: 0.5rem;
    padding-inline: 0.75rem;
  }

  .topbar-title {
    max-width: 9rem;
    overflow: hidden;
    font-size: 1rem;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .topbar-user {
    display: none !important;
  }

  .topbar-logout {
    width: 38px;
    padding: 0;
  }
}
</style>
