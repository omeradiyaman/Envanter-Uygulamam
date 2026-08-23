<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const title = computed(() => String(route.meta.title ?? 'Modül'))
</script>

<template>
  <div class="card content-card">
    <div class="card-body placeholder-content d-flex flex-column align-items-center justify-content-center text-center">
      <span class="placeholder-icon d-inline-flex align-items-center justify-content-center mb-3">
        <i class="bi bi-tools" aria-hidden="true"></i>
      </span>
      <h2 class="h5 fw-semibold">{{ title }}</h2>
      <p class="text-secondary mb-0">
        Bu modül sonraki geliştirme aşamalarında hazırlanacaktır.
      </p>
    </div>
  </div>
</template>

<style scoped>
.placeholder-content {
  min-height: 420px;
}

.placeholder-icon {
  width: 54px;
  height: 54px;
  border-radius: 1rem;
  color: #2364d2;
  background: #e9f0fc;
}

.placeholder-icon i {
  font-size: 1.35rem;
}
</style>
