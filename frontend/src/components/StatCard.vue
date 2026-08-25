<script setup lang="ts">
defineProps<{
  label: string
  value: string | number
  icon: string
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info'
}>()
</script>

<template>
  <div class="stat-card h-100">
    <span
      class="stat-card__icon d-inline-flex align-items-center justify-content-center"
      :class="variant ?? 'default'"
    >
      <i :class="['bi', icon]" aria-hidden="true"></i>
    </span>
    <div class="stat-card__content">
      <div class="stat-card__label">{{ label }}</div>
      <div class="stat-card__value">{{ value }}</div>
    </div>
  </div>
</template>
