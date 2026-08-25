<script setup lang="ts">
defineProps<{
  type: 'loading' | 'error' | 'empty' | 'no-results'
  title: string
  message?: string
  icon?: string
}>()
</script>

<template>
  <div class="state-panel" :role="type === 'error' ? 'alert' : 'status'" :aria-live="type === 'error' ? 'assertive' : 'polite'">
    <div v-if="type === 'loading'" class="spinner-border text-primary mb-3" role="status">
      <span class="visually-hidden">Yükleniyor</span>
    </div>
    <span v-else class="state-icon" :class="{ error: type === 'error' }">
      <i
        :class="[
          'bi',
          icon ??
            (type === 'error'
              ? 'bi-exclamation-triangle'
              : type === 'no-results'
                ? 'bi-search'
                : 'bi-inbox'),
        ]"
        aria-hidden="true"
      ></i>
    </span>
    <strong>{{ title }}</strong>
    <span v-if="message">{{ message }}</span>
    <div v-if="$slots.actions" class="state-panel__actions mt-2 d-flex flex-wrap gap-2 justify-content-center">
      <slot name="actions" />
    </div>
  </div>
</template>
