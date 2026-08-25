<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from '../components/AppSidebar.vue'
import AppTopbar from '../components/AppTopbar.vue'

const route = useRoute()
const sidebarOpen = ref(false)
const pageTitle = computed(() => String(route.meta.title ?? 'Dashboard'))

function closeSidebar() {
  sidebarOpen.value = false
}
</script>

<template>
  <div class="app-shell" :class="{ 'sidebar-open': sidebarOpen }">
    <AppSidebar @navigate="closeSidebar" />

    <button
      v-if="sidebarOpen"
      type="button"
      class="sidebar-backdrop d-lg-none"
      aria-label="Menüyü kapat"
      @click="closeSidebar"
    ></button>

    <div class="app-main">
      <AppTopbar :title="pageTitle" @toggle-sidebar="sidebarOpen = !sidebarOpen" />
      <main class="page-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
.sidebar-backdrop {
  position: fixed;
  z-index: 1030;
  inset: 0;
  width: 100%;
  height: 100%;
  padding: 0;
  border: 0;
  background: rgb(17 38 63 / 55%);
}
</style>
