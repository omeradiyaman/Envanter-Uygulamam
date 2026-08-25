<script setup lang="ts">
defineProps<{
  title: string
  icon?: string
  description?: string
}>()
</script>

<template>
  <section class="detail-section">
    <header class="detail-section__header">
      <div>
        <h3 class="detail-section__title">
          <i v-if="icon" :class="['bi', icon, 'me-2']" aria-hidden="true"></i>
          {{ title }}
        </h3>
        <p v-if="description" class="detail-section__description mb-0">{{ description }}</p>
      </div>
      <div v-if="$slots.actions" class="detail-section__actions">
        <slot name="actions" />
      </div>
    </header>
    <div class="detail-section__body">
      <slot />
    </div>
  </section>
</template>
