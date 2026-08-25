import { get, post } from './apiClient'

export interface AssignmentHistory {
  id: string
  deviceId: string
  deviceName: string
  deviceSeriNo: string
  deviceEnvanterNo: string
  deviceCategoryName: string
  personnelId: string
  personnelName: string
  personnelSicilNo: string
  assignedAt: string
  returnedAt: string | null
  note: string | null
  isActive: boolean
}

export interface AssignDevicePayload {
  deviceId: string
  personnelId: string
  note?: string | null
}

export interface UnassignDevicePayload {
  deviceId: string
  note?: string | null
}

export function assignDevice(payload: AssignDevicePayload): Promise<void> {
  return post<AssignDevicePayload, void>('/api/assignments/assign', payload)
}

export function unassignDevice(payload: UnassignDevicePayload): Promise<void> {
  return post<UnassignDevicePayload, void>('/api/assignments/unassign', payload)
}

export function getDeviceAssignmentHistory(
  deviceId: string,
  signal?: AbortSignal,
): Promise<AssignmentHistory[]> {
  return get<AssignmentHistory[]>(`/api/assignments/device/${deviceId}`, signal)
}

export function getPersonnelAssignmentHistory(
  personnelId: string,
  signal?: AbortSignal,
): Promise<AssignmentHistory[]> {
  return get<AssignmentHistory[]>(`/api/assignments/personnel/${personnelId}`, signal)
}
