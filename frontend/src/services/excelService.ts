import { getBlob, postForm } from './apiClient'

export type ImportTarget = 'devices' | 'personnel'

export interface ImportPreviewRow {
  rowNumber: number
  action: string
  identifier: string
  summary: string
  messages: string[]
}

export interface ImportPreviewSummary {
  newCount: number
  updatedCount: number
  unchangedCount: number
  errorCount: number
  rows: ImportPreviewRow[]
}

export interface ImportExecutionResult {
  addedCount: number
  updatedCount: number
  unchangedCount: number
  errorCount: number
  rows: ImportPreviewRow[]
}

function fileForm(file: File) {
  const form = new FormData()
  form.append('file', file)
  return form
}

export function previewExcelImport(target: ImportTarget, file: File) {
  return postForm<ImportPreviewSummary>(`/api/excel/${target}/preview`, fileForm(file))
}

export function executeExcelImport(target: ImportTarget, file: File) {
  return postForm<ImportExecutionResult>(`/api/excel/${target}/import`, fileForm(file))
}

async function download(path: string, fileName: string) {
  const blob = await getBlob(path)
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
  URL.revokeObjectURL(url)
}

const dateSuffix = () => new Date().toISOString().slice(0, 10)

function idQuery(ids?: string[]) {
  if (!ids?.length) return ''
  const query = new URLSearchParams()
  ids.forEach(id => query.append('ids', id))
  return `?${query.toString()}`
}

export function exportDevicesExcel(ids?: string[]) {
  return download(`/api/excel/devices/export${idQuery(ids)}`, `cihazlar-${dateSuffix()}.xlsx`)
}

export function exportDeviceCategoryExcel(categoryId: string, categoryName: string) {
  const query = new URLSearchParams({ categoryId })
  const safeName = categoryName.toLocaleLowerCase('tr-TR').replace(/[^a-z0-9çğıöşü]+/gi, '-').replace(/^-|-$/g, '')
  return download(`/api/excel/devices/export?${query.toString()}`, `${safeName || 'envanter'}-${dateSuffix()}.xlsx`)
}

export function exportPersonnelExcel(ids?: string[]) {
  return download(`/api/excel/personnel/export${idQuery(ids)}`, `personeller-${dateSuffix()}.xlsx`)
}

export function exportInventoryExcel() {
  return download('/api/excel/inventory/export', `envanter-tumu-${dateSuffix()}.xlsx`)
}
