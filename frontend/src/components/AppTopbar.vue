<script setup lang="ts">
defineProps<{
  title: string
}>()

const emit = defineEmits<{
  toggleSidebar: []
}>()
</script>

<template>
  <header class="app-topbar d-flex align-items-center justify-content-between">
    <div class="d-flex align-items-center gap-3">
      <button
        type="button"
        class="btn btn-light border d-lg-none"
        aria-label="Menüyü aç"
        @click="emit('toggleSidebar')"
      >
        <i class="bi bi-list" aria-hidden="true"></i>
      </button>
      <div>
        <h1 class="h5 fw-semibold mb-0">{{ title }}</h1>
        <small class="text-secondary d-none d-sm-inline">Envanter yönetim paneli</small>
      </div>
    </div>

    <div class="user-badge d-flex align-items-center gap-2">
      <span class="avatar d-inline-flex align-items-center justify-content-center">
        <i class="bi bi-person" aria-hidden="true"></i>
      </span>
      <div class="d-none d-sm-block">
        <div class="small fw-semibold">Yönetici</div>
        <div class="user-role">Sistem kullanıcısı</div>
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-topbar {
  position: sticky;
  z-index: 1020;
  top: 0;
  min-height: 86px;
  padding: 1rem 1.75rem;
  border-bottom: 1px solid var(--border-color);
  background: rgb(255 255 255 / 96%);
  backdrop-filter: blur(8px);
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  color: #2364d2;
  background: #e9f0fc;
}

.user-role {
  color: #78879a;
  font-size: 0.75rem;
}

@media (max-width: 991.98px) {
  .app-topbar {
    min-height: 74px;
    padding: 0.75rem 1rem;
  }
}
</style>
