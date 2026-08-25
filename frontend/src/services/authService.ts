import { get, post } from './apiClient'

export const Roles = { Admin: 'Admin', Editor: 'Editor', Viewer: 'Görüntüleyici' } as const
export type Role = typeof Roles[keyof typeof Roles]

export interface AuthUser {
  id: string
  userName: string
  fullName: string
  role: Role
  isActive: boolean
  lastLoginAt: string | null
}

export function login(userName: string, password: string) {
  return post<{ userName: string; password: string }, AuthUser>('/api/auth/login', { userName, password })
}
export function logout() { return post<null, void>('/api/auth/logout', null) }
export function getCurrentUser() { return get<AuthUser>('/api/auth/me') }
