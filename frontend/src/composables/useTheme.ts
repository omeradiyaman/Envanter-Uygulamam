import { ref } from 'vue'

export type Theme = 'light' | 'dark'

export const THEME_STORAGE_KEY = 'envanter-theme'

const theme = ref<Theme>(readStoredTheme())

function readStoredTheme(): Theme {
  if (typeof window === 'undefined') {
    return 'light'
  }

  const stored = localStorage.getItem(THEME_STORAGE_KEY)
  return stored === 'dark' ? 'dark' : 'light'
}

function applyTheme(nextTheme: Theme): void {
  theme.value = nextTheme
  document.documentElement.setAttribute('data-theme', nextTheme)
  document.querySelector<HTMLMetaElement>('meta[name="theme-color"]')?.setAttribute(
    'content',
    nextTheme === 'dark' ? '#060a12' : '#f1f5f9',
  )
  localStorage.setItem(THEME_STORAGE_KEY, nextTheme)
}

export function initTheme(): void {
  applyTheme(readStoredTheme())
}

export function useTheme() {
  function setTheme(nextTheme: Theme): void {
    applyTheme(nextTheme)
  }

  function toggleTheme(): void {
    applyTheme(theme.value === 'light' ? 'dark' : 'light')
  }

  return {
    theme,
    setTheme,
    toggleTheme,
  }
}
