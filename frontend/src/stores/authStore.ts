import { computed, reactive } from 'vue'
import { getCurrentUser, login as loginRequest, logout as logoutRequest, Roles, type AuthUser, type Role } from '../services/authService'

const state = reactive<{ user: AuthUser | null; initialized: boolean }>({ user: null, initialized: false })

async function initialize() {
  if (state.initialized) return
  try { state.user = await getCurrentUser() } catch { state.user = null }
  finally { state.initialized = true }
}

async function login(userName: string, password: string) {
  state.user = await loginRequest(userName, password)
  state.initialized = true
}

async function logout() {
  try { await logoutRequest() } finally { state.user = null; state.initialized = true }
}

function hasRole(...roles: Role[]) { return !!state.user && roles.includes(state.user.role) }
function clear() { state.user = null; state.initialized = true }

export const authStore = {
  state,
  user: computed(() => state.user),
  isAuthenticated: computed(() => !!state.user),
  isAdmin: computed(() => state.user?.role === Roles.Admin),
  canEdit: computed(() => state.user?.role === Roles.Admin || state.user?.role === Roles.Editor),
  initialize, login, logout, hasRole, clear,
}
