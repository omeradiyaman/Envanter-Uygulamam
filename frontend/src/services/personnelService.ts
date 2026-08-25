import { del, get, getBlob, post, postForm, put } from './apiClient'
import type { DeviceListItem } from './deviceService'

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
  devices: DeviceListItem[]
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

export interface AssignmentDocument {
  id: string
  personnelId: string
  assignmentHistoryId: string | null
  originalFileName: string
  contentType: string
  fileSize: number
  uploadedAt: string
  description: string | null
  replacesDocumentId: string | null
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

export function getGeneratedAssignmentDocument(personnelId: string): Promise<Blob> {
  return getBlob(`/api/personnel/${personnelId}/assignment-documents/generated`)
}

export function getAssignmentDocuments(personnelId: string): Promise<AssignmentDocument[]> {
  return get<AssignmentDocument[]>(`/api/personnel/${personnelId}/assignment-documents`)
}

export function getAssignmentDocumentContent(personnelId: string, documentId: string): Promise<Blob> {
  return getBlob(`/api/personnel/${personnelId}/assignment-documents/${documentId}/content`)
}

export function uploadAssignmentDocument(
  personnelId: string,
  file: File,
  description: string,
  replacesDocumentId?: string,
): Promise<AssignmentDocument> {
  const form = new FormData()
  form.append('file', file)
  if (description.trim()) form.append('description', description.trim())
  if (replacesDocumentId) form.append('replacesDocumentId', replacesDocumentId)
  return postForm<AssignmentDocument>(`/api/personnel/${personnelId}/assignment-documents`, form)
}

export function deleteAssignmentDocument(personnelId: string, documentId: string): Promise<void> {
  return del(`/api/personnel/${personnelId}/assignment-documents/${documentId}`)
}
