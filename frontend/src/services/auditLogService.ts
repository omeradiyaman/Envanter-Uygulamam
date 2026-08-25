import { get, post } from './apiClient'

export interface AuditLog {
  id: string
  userId: string | null
  userDisplayName: string
  actionType: string
  entityType: string
  entityId: string
  description: string
  oldValues: string | null
  newValues: string | null
  createdAt: string
  canUndo: boolean
  undoneAt: string | null
  undoAuditLogId: string | null
  operationId: string | null
}

export interface UndoAuditLogResult {
  auditLogId: string
  undoAuditLogId: string
  undoneAt: string
}

export interface AuditLogFilters {
  actionType?: string
  userId?: string
  systemUserOnly?: boolean
  entityType?: string
  from?: string
  to?: string
}

export const actionLabels: Record<string, string> = {
  PersonnelCreated: 'Personel eklendi',
  PersonnelUpdated: 'Personel güncellendi',
  PersonnelDeleted: 'Personel silindi',
  DeviceCreated: 'Cihaz eklendi',
  DeviceUpdated: 'Cihaz güncellendi',
  DeviceDeleted: 'Cihaz silindi',
  WarrantyUpdated: 'Garanti güncellendi',
  DeviceScrapped: 'Hurda / İmha',
  DeviceAssigned: 'Zimmet verildi',
  DeviceReturned: 'Zimmet iade alındı',
  ExcelImported: 'Excel import',
  DocumentUploaded: 'Belge yüklendi',
  DocumentDeleted: 'Belge silindi',
  Undo: 'Geri alma',
}

export const entityLabels: Record<string, string> = {
  Personnel: 'Personel',
  Device: 'Cihaz',
  Assignment: 'Zimmet',
  AssignmentDocument: 'Belge',
  ExcelBatch: 'Excel işlemi',
}

export function getAuditLogs(filters: AuditLogFilters = {}, signal?: AbortSignal) {
  const query = new URLSearchParams()
  if (filters.actionType) query.set('actionType', filters.actionType)
  if (filters.userId) query.set('userId', filters.userId)
  if (filters.systemUserOnly) query.set('systemUserOnly', 'true')
  if (filters.entityType) query.set('entityType', filters.entityType)
  if (filters.from) query.set('from', filters.from)
  if (filters.to) query.set('to', filters.to)
  const suffix = query.size ? `?${query.toString()}` : ''
  return get<AuditLog[]>(`/api/audit-logs${suffix}`, signal)
}

export function undoAuditLog(id: string) {
  return post<null, UndoAuditLogResult>(`/api/audit-logs/${id}/undo`, null)
}
