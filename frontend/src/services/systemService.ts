import { get } from './apiClient'

export interface SystemStatus {
  status: string
  service: string
  client: string
  timestamp: string
  apiVersion: string
}

export function getSystemStatus(signal?: AbortSignal): Promise<SystemStatus> {
  return get<SystemStatus>('/api/system/status?client=frontend', signal)
}
