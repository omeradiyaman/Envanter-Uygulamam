import { get, post, put, del } from './apiClient'

export interface DeviceCategory {
  id: string
  name: string
  description?: string | null
  deviceCount?: number
}

export interface Device {
  id: string
  cihazAdi: string
  seriNo: string
  envanterNo: string
  barkodNo: string | null
  marka: string
  model: string
  categoryId: string
  categoryName: string
  status: number
  statusName: string
  personelId: string | null
  personelName: string | null
  createdAt: string
  updatedAt: string | null
  warrantyStartDate: string | null
  warrantyEndDate: string | null
  warrantyProvider: string | null
  warrantyNote: string | null
  warrantyStatus: number
  warrantyStatusName: string
  warrantyRemainingDays: number | null
  warrantyMessage: string
}

export interface DeviceListItem {
  id: string
  cihazAdi: string
  seriNo: string
  envanterNo: string
  marka: string
  model: string
  categoryName: string
  status: number
  statusName: string
  personelName: string | null
  createdAt: string
  warrantyStartDate: string | null
  warrantyEndDate: string | null
  warrantyProvider: string | null
  warrantyNote: string | null
  warrantyStatus: number
  warrantyStatusName: string
  warrantyRemainingDays: number | null
  warrantyMessage: string
}

export interface DevicePayload {
  cihazAdi: string
  seriNo: string
  envanterNo: string
  barkodNo: string | null
  marka: string
  model: string
  categoryId: string
  status: number
  personelId: string | null
  warrantyStartDate: string | null
  warrantyEndDate: string | null
  warrantyProvider: string | null
  warrantyNote: string | null
}

export interface WarrantyAlertDevice {
  id: string
  cihazAdi: string
  seriNo: string
  envanterNo: string
  warrantyEndDate: string | null
  warrantyStatus: number
  warrantyStatusName: string
  warrantyRemainingDays: number | null
  warrantyMessage: string
}

export interface WarrantySummary {
  expiringSoonDays: number
  expiringCount: number
  expiredCount: number
  missingCount: number
}

export const WarrantyStatuses = {
  NoWarranty: 0,
  Active: 1,
  ExpiringSoon: 2,
  Expired: 3,
} as const

export interface DeviceQrCode {
  deviceId: string
  deviceUrl: string
  pngBase64: string
}

export const DeviceStatuses = [
  { id: 1, name: 'Stokta', color: 'primary' },
  { id: 2, name: 'Zimmetli', color: 'success' },
  { id: 3, name: 'Serviste', color: 'warning' },
  { id: 4, name: 'Hurda', color: 'danger' },
  { id: 5, name: 'Kayıp', color: 'dark' },
  { id: 6, name: 'Pasif', color: 'secondary' },
]

export function getStatusColor(statusId: number): string {
  const status = DeviceStatuses.find((s) => s.id === statusId)
  return status ? status.color : 'secondary'
}

export function getDevices(signal?: AbortSignal): Promise<DeviceListItem[]> {
  return get<DeviceListItem[]>('/api/devices', signal)
}

export function getDevice(id: string, signal?: AbortSignal): Promise<Device> {
  return get<Device>(`/api/devices/${id}`, signal)
}

export function getDeviceQrCode(id: string, signal?: AbortSignal): Promise<DeviceQrCode> {
  return get<DeviceQrCode>(`/api/devices/${id}/qr-code`, signal)
}

export function createDevice(payload: DevicePayload): Promise<string> {
  return post<DevicePayload, string>('/api/devices', payload)
}

export function updateDevice(id: string, payload: DevicePayload): Promise<void> {
  return put(`/api/devices/${id}`, payload)
}

export function deleteDevice(id: string): Promise<void> {
  return del(`/api/devices/${id}`)
}

export function getDeviceCategories(signal?: AbortSignal): Promise<DeviceCategory[]> {
  return get<DeviceCategory[]>('/api/categories', signal)
}

export function createDeviceCategory(payload: { name: string; description: string | null }): Promise<string> {
  return post('/api/categories', payload)
}

export function updateDeviceCategory(id: string, payload: { name: string; description: string | null }): Promise<void> {
  return put(`/api/categories/${id}`, { id, ...payload })
}

export function deleteDeviceCategory(id: string): Promise<void> {
  return del(`/api/categories/${id}`)
}

export function getExpiringWarranties(signal?: AbortSignal): Promise<WarrantyAlertDevice[]> {
  return get<WarrantyAlertDevice[]>('/api/warranties/expiring', signal)
}

export function getWarrantySummary(signal?: AbortSignal): Promise<WarrantySummary> {
  return get<WarrantySummary>('/api/warranties/summary', signal)
}

export function getExpiredWarranties(signal?: AbortSignal): Promise<WarrantyAlertDevice[]> {
  return get<WarrantyAlertDevice[]>('/api/warranties/expired', signal)
}

export function getDevicesWithoutWarranty(signal?: AbortSignal): Promise<WarrantyAlertDevice[]> {
  return get<WarrantyAlertDevice[]>('/api/warranties/missing', signal)
}

export function getWarrantyBadgeClass(status: number): string {
  if (status === WarrantyStatuses.Active) return 'text-bg-success'
  if (status === WarrantyStatuses.ExpiringSoon) return 'text-bg-warning'
  if (status === WarrantyStatuses.Expired) return 'text-bg-danger'
  return 'text-bg-secondary'
}
