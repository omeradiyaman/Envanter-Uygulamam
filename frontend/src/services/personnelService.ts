import { del, get, post, put } from './apiClient'

export interface PersonnelListItem {
  id: string
  sicilNo: string
  ad: string
  soyad: string
  departman: string
  pozisyon: string
  zimmetNo: string | null
  aktifMi: boolean
}

export interface PersonnelDetail extends PersonnelListItem {
  createdAt: string
  updatedAt: string | null
}

export interface PersonnelPayload {
  sicilNo: string
  ad: string
  soyad: string
  departman: string
  pozisyon: string
  zimmetNo: string | null
  aktifMi: boolean
}

export function getPersonnelList(
  signal?: AbortSignal,
): Promise<PersonnelListItem[]> {
  return get<PersonnelListItem[]>('/api/personnel', signal)
}

export function getPersonnelById(
  id: string,
  signal?: AbortSignal,
): Promise<PersonnelDetail> {
  return get<PersonnelDetail>(`/api/personnel/${id}`, signal)
}

export function createPersonnel(
  payload: PersonnelPayload,
): Promise<PersonnelDetail> {
  return post<PersonnelPayload, PersonnelDetail>('/api/personnel', payload)
}

export function updatePersonnel(
  id: string,
  payload: PersonnelPayload,
): Promise<PersonnelDetail> {
  return put<PersonnelPayload, PersonnelDetail>(`/api/personnel/${id}`, payload)
}

export function deletePersonnel(id: string): Promise<void> {
  return del(`/api/personnel/${id}`)
}
