<script setup lang="ts">
const emit = defineEmits<{
  navigate: []
}>()

const navigationItems = [
  { label: 'Dashboard', icon: 'bi-grid-1x2', to: '/' },
  { label: 'Cihazlar', icon: 'bi-pc-display', to: '/cihazlar' },
  { label: 'Personeller', icon: 'bi-people', to: '/personeller' },
  { label: 'Envanterler', icon: 'bi-box-seam', to: '/envanterler' },
  { label: 'Stok', icon: 'bi-boxes', to: '/stok' },
  { label: 'Hurda / İmha', icon: 'bi-recycle', to: '/hurda-imha' },
  { label: 'Raporlar', icon: 'bi-bar-chart', to: '/raporlar' },
  { label: 'Son Hareketler', icon: 'bi-clock-history', to: '/son-hareketler' },
  {
    label: 'Kullanıcı Yönetimi',
    icon: 'bi-person-gear',
    to: '/kullanici-yonetimi',
  },
]
</script>

<template>
  <aside class="app-sidebar d-flex flex-column">
    <div class="brand d-flex align-items-center gap-3">
      <span class="brand-icon d-inline-flex align-items-center justify-content-center">
        <i class="bi bi-box-seam" aria-hidden="true"></i>
      </span>
      <div>
        <div class="fw-semibold text-white">Envanter Sistemi</div>
        <small>Yönetim Paneli</small>
      </div>
    </div>

    <nav class="sidebar-nav flex-grow-1" aria-label="Ana menü">
      <RouterLink
        v-for="item in navigationItems"
        :key="item.to"
        :to="item.to"
        class="nav-link d-flex align-items-center gap-3"
        @click="emit('navigate')"
      >
        <i :class="['bi', item.icon]" aria-hidden="true"></i>
        <span>{{ item.label }}</span>
      </RouterLink>
    </nav>

    <div class="sidebar-footer">
      <small>InventorySystem v1.0</small>
    </div>
  </aside>
</template>

<style scoped>
.app-sidebar {
  position: fixed;
  z-index: 1040;
  inset: 0 auto 0 0;
  width: var(--sidebar-width);
  overflow-y: auto;
  color: #b9c7d8;
  background: var(--sidebar-bg);
  transition: transform 0.25s ease;
}

.brand {
  min-height: 86px;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid rgb(255 255 255 / 8%);
}

.brand small,
.sidebar-footer small {
  color: #8fa3ba;
}

.brand-icon {
  width: 42px;
  height: 42px;
  flex: 0 0 42px;
  border-radius: 0.75rem;
  color: #fff;
  background: var(--sidebar-active);
}

.brand-icon i {
  font-size: 1.25rem;
}

.sidebar-nav {
  padding: 1.25rem 0.875rem;
}

.nav-link {
  min-height: 46px;
  margin-bottom: 0.35rem;
  padding: 0.75rem 1rem;
  border-radius: 0.625rem;
  color: #b9c7d8;
  font-size: 0.925rem;
  transition:
    color 0.15s ease,
    background-color 0.15s ease;
}

.nav-link i {
  width: 1.25rem;
  font-size: 1.1rem;
}

.nav-link:hover,
.nav-link.router-link-exact-active {
  color: #fff;
  background: var(--sidebar-active);
}

.sidebar-footer {
  padding: 1.25rem 1.5rem;
  border-top: 1px solid rgb(255 255 255 / 8%);
}

@media (max-width: 991.98px) {
  .app-sidebar {
    transform: translateX(-100%);
  }

  :global(.sidebar-open) .app-sidebar {
    transform: translateX(0);
  }
}
</style>
