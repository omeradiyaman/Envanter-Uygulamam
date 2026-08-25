<script setup lang="ts">
defineProps<{
  steps: string[]
  current: number
}>()
</script>

<template>
  <ol class="workflow-steps" aria-label="İşlem adımları">
    <li v-for="(step, index) in steps" :key="step" :class="{ 'is-active': current === index + 1, 'is-done': current > index + 1 }">
      <span class="workflow-steps__number"><i v-if="current > index + 1" class="bi bi-check-lg"></i><template v-else>{{ index + 1 }}</template></span>
      <strong>{{ step }}</strong>
    </li>
  </ol>
</template>
