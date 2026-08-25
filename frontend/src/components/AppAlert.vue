<script setup lang="ts">
defineProps<{
  variant?: 'success' | 'danger' | 'warning' | 'info'
  dismissible?: boolean
}>()

const emit = defineEmits<{
  dismiss: []
}>()
</script>

<template>
  <div
    class="app-alert"
    :class="variant ?? 'info'"
    :role="variant === 'danger' ? 'alert' : 'status'"
    :aria-live="variant === 'danger' ? 'assertive' : 'polite'"
  >
    <div class="app-alert__content">
      <slot />
    </div>
    <button
      v-if="dismissible"
      type="button"
      class="app-alert__close btn-close"
      aria-label="Kapat"
      @click="emit('dismiss')"
    ></button>
  </div>
</template>
