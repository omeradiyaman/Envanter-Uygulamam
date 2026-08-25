<script setup lang="ts">
defineProps<{
  label: string
  variant?: 'primary' | 'success' | 'warning' | 'danger' | 'neutral' | 'info'
  dot?: boolean
}>()
</script>

<template>
  <span class="app-status-badge" :class="`is-${variant ?? 'neutral'}`">
    <span v-if="dot" class="app-status-badge__dot"></span>{{ label }}
  </span>
</template>
