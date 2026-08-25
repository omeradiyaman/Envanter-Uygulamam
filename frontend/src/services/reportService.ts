import { get, getBlob } from './apiClient'
export interface CountRow{key:string;label:string;count:number}
export interface ReportDevice{id:string;deviceName:string;inventoryNumber:string;serialNumber:string;category:string;status:string;department:string|null;warrantyStatus:string}
export interface AssignmentRow{id:string;deviceName:string;inventoryNumber:string;personnelName:string;department:string;assignedAt:string;returnedAt:string|null}
export interface InventoryReport{byCategory:CountRow[];byStatus:CountRow[];byDepartment:CountRow[];stockDevices:ReportDevice[];scrapDevices:ReportDevice[];expiringWarranties:ReportDevice[];expiredWarranties:ReportDevice[];assignmentMovements:AssignmentRow[];filterOptions:{categories:CountRow[];departments:string[]};generatedAt:string}
export interface ReportFilters{from?:string;to?:string;categoryId?:string;department?:string;status?:string}
function query(filters:ReportFilters){const q=new URLSearchParams();Object.entries(filters).forEach(([k,v])=>{if(v)q.set(k,v)});return q.size?`?${q}`:''}
export function getInventoryReport(filters:ReportFilters){return get<InventoryReport>(`/api/reports/inventory${query(filters)}`)}
export async function exportInventoryReport(filters:ReportFilters){const blob=await getBlob(`/api/reports/inventory/export${query(filters)}`);const url=URL.createObjectURL(blob);const a=document.createElement('a');a.href=url;a.download=`envanter-raporu-${new Date().toISOString().slice(0,10)}.xlsx`;a.click();URL.revokeObjectURL(url)}
