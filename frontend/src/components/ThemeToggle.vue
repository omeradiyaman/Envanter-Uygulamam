<script setup lang="ts">
import { computed } from 'vue'
import { useTheme } from '../composables/useTheme'

const { theme, setTheme } = useTheme()
const nextThemeLabel = computed(() => theme.value === 'light' ? 'Koyu temaya geç' : 'Açık temaya geç')
function toggleTheme() { setTheme(theme.value === 'light' ? 'dark' : 'light') }
</script>

<template>
  <button
    type="button"
    class="theme-toggle"
    :class="{ 'is-dark': theme === 'dark' }"
    :aria-label="nextThemeLabel"
    :title="nextThemeLabel"
    @click="toggleTheme"
  >
    <i class="bi bi-sun-fill theme-toggle__sun" aria-hidden="true"></i>
    <i class="bi bi-moon-stars-fill theme-toggle__moon" aria-hidden="true"></i>
  </button>
</template>

<style scoped>
.theme-toggle {
  display: inline-grid;
  place-items: center;
  align-items: center;
  width: 38px;
  height: 38px;
  padding: 0;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: var(--surface-muted);
  color: var(--primary);
  cursor: pointer;
  transition: transform 0.2s ease, color 0.2s ease, background 0.2s ease, border-color 0.2s ease;
}

.theme-toggle i {
  grid-area: 1 / 1;
  font-size: 1rem;
  transition: opacity 0.2s ease, transform 0.25s ease;
}

.theme-toggle:hover {
  transform: translateY(-1px);
  color: var(--primary-hover);
  border-color: var(--primary-muted);
  background: var(--primary-subtle);
}

.theme-toggle:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring);
}

.theme-toggle__sun {
  opacity: 0;
  transform: scale(0.55) rotate(35deg);
}

.theme-toggle__moon {
  opacity: 1;
  transform: scale(1) rotate(0);
}

.theme-toggle.is-dark .theme-toggle__sun {
  opacity: 1;
  transform: scale(1) rotate(0);
}

.theme-toggle.is-dark .theme-toggle__moon {
  opacity: 0;
  transform: scale(0.55) rotate(-35deg);
}
</style>
