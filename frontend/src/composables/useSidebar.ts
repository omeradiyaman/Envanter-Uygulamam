import { ref } from 'vue'

export const SIDEBAR_STORAGE_KEY = 'envanter-sidebar-collapsed'

const collapsed = ref(readCollapsed())

function readCollapsed(): boolean {
  if (typeof window === 'undefined') {
    return false
  }

  return localStorage.getItem(SIDEBAR_STORAGE_KEY) === 'true'
}

export function useSidebar() {
  function toggleCollapsed(): void {
    collapsed.value = !collapsed.value
    localStorage.setItem(SIDEBAR_STORAGE_KEY, String(collapsed.value))
  }

  function setCollapsed(value: boolean): void {
    collapsed.value = value
    localStorage.setItem(SIDEBAR_STORAGE_KEY, String(value))
  }

  return {
    collapsed,
    toggleCollapsed,
    setCollapsed,
  }
}
