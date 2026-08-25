import { get, post, put } from './apiClient'
import type { Role } from './authService'

export interface UserItem { id:string; userName:string; fullName:string; role:Role; isActive:boolean; lastLoginAt:string|null; createdAt:string }
export interface CreateUserPayload { userName:string; fullName:string; password:string; role:Role; isActive:boolean }
export interface UpdateUserPayload { userName:string; fullName:string; role:Role; isActive:boolean }
export function getUsers() { return get<UserItem[]>('/api/users') }
export function createUser(payload:CreateUserPayload) { return post<CreateUserPayload,UserItem>('/api/users',payload) }
export function updateUser(id:string,payload:UpdateUserPayload) { return put<UpdateUserPayload,UserItem>(`/api/users/${id}`,payload) }
export function resetUserPassword(id:string,newPassword:string) { return post<{newPassword:string},void>(`/api/users/${id}/reset-password`,{newPassword}) }
