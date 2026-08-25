import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '../layouts/AppLayout.vue'
import DashboardView from '../views/DashboardView.vue'
import DeviceView from '../views/DeviceView.vue'
import PersonnelView from '../views/PersonnelView.vue'
import AuditLogView from '../views/AuditLogView.vue'
import LoginView from '../views/LoginView.vue'
import UserManagementView from '../views/UserManagementView.vue'
import ReportsView from '../views/ReportsView.vue'
import AssignmentCenterView from '../views/AssignmentCenterView.vue'
import BulkQrView from '../views/BulkQrView.vue'
import ExcelOperationsView from '../views/ExcelOperationsView.vue'
import DeviceWorkspaceView from '../views/DeviceWorkspaceView.vue'
import PersonnelWorkspaceView from '../views/PersonnelWorkspaceView.vue'
import LabelQrHubView from '../views/LabelQrHubView.vue'
import BulkLabelView from '../views/BulkLabelView.vue'
import AuditDetailView from '../views/AuditDetailView.vue'
import InventoryManagementView from '../views/InventoryManagementView.vue'
import InventoryTableEditorView from '../views/InventoryTableEditorView.vue'
import { authStore } from '../stores/authStore'
import { Roles, type Role } from '../services/authService'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView, meta: { title: 'Giriş', public: true } },
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '',
          name: 'dashboard',
          component: DashboardView,
          meta: { title: 'Dashboard' },
        },
        {
          path: 'cihazlar',
          redirect: { name: 'inventories' },
        },
        { path: 'cihazlar/yeni', name: 'device-create', component: DeviceWorkspaceView, meta: { title: 'Yeni Cihaz', roles: [Roles.Admin, Roles.Editor] } },
        { path: 'cihazlar/:id', name: 'device-detail', component: DeviceWorkspaceView, meta: { title: 'Cihaz Detayı' } },
        { path: 'cihazlar/:id/duzenle', name: 'device-edit', component: DeviceWorkspaceView, meta: { title: 'Cihazı Düzenle', roles: [Roles.Admin, Roles.Editor] } },
        { path: 'cihazlar/:id/gecmis', name: 'device-history', component: DeviceWorkspaceView, meta: { title: 'Cihaz Zimmet Geçmişi' } },
        {
          path: 'devices/:id',
          name: 'device-qr-detail',
          component: DeviceWorkspaceView,
          meta: { title: 'Cihaz Detayı' },
        },
        {
          path: 'personeller',
          name: 'personnel',
          component: PersonnelView,
          meta: { title: 'Personeller' },
        },
        {
          path: 'envanterler',
          name: 'inventories',
          component: DeviceView,
          meta: { title: 'Envanter Listesi' },
        },
        {
          path: 'stok',
          redirect: { name: 'inventories', query: { view: 'stock' } },
        },
        {
          path: 'hurda-imha',
          redirect: { name: 'inventories', query: { view: 'scrap' } },
        },
        { path: 'envanter-yonetimi', name: 'inventory-management', component: InventoryManagementView, meta: { title: 'Envanter Yönetimi' } },
        { path: 'envanterler/:categoryId/tabloyu-duzenle', name: 'inventory-table-editor', component: InventoryTableEditorView, meta: { title: 'Envanter Tablosunu Düzenle', roles: [Roles.Admin, Roles.Editor] } },
        { path: 'personeller/yeni', name: 'personnel-create', component: PersonnelWorkspaceView, meta: { title: 'Yeni Personel', roles: [Roles.Admin, Roles.Editor] } },
        { path: 'personeller/:id', name: 'personnel-detail', component: PersonnelWorkspaceView, meta: { title: 'Personel Detayı' } },
        { path: 'personeller/:id/duzenle', name: 'personnel-edit', component: PersonnelWorkspaceView, meta: { title: 'Personeli Düzenle', roles: [Roles.Admin, Roles.Editor] } },
        { path: 'personeller/:id/belgeler', name: 'personnel-documents', component: PersonnelWorkspaceView, meta: { title: 'Personel Belgeleri' } },
        { path: 'personeller/:id/zimmet-gecmisi', name: 'personnel-history', component: PersonnelWorkspaceView, meta: { title: 'Personel Zimmet Geçmişi' } },
        {
          path: 'zimmet',
          name: 'assignments',
          component: AssignmentCenterView,
          meta: { title: 'Zimmet Belgesi Oluştur' },
        },
        { path: 'zimmet/yeni', name: 'assignment-create', component: AssignmentCenterView, meta: { title: 'Yeni Zimmet', roles: [Roles.Admin, Roles.Editor] } },
        { path: 'zimmet/aktif', name: 'active-assignments', component: AssignmentCenterView, meta: { title: 'Aktif Zimmetler' } },
        { path: 'zimmet/gecmis', name: 'assignment-history', component: AssignmentCenterView, meta: { title: 'Zimmet Geçmişi' } },
        {
          path: 'excel-islemleri',
          name: 'excel',
          component: ExcelOperationsView,
          meta: { title: 'Excel İşlemleri' },
        },
        { path: 'excel-islemleri/import', name: 'excel-import', component: ExcelOperationsView, meta: { title: 'Excel Import', roles: [Roles.Admin, Roles.Editor] } },
        { path: 'excel-islemleri/export', name: 'excel-export', component: ExcelOperationsView, meta: { title: 'Excel Export' } },
        { path: 'etiket-qr-islemleri', name: 'label-qr-hub', component: LabelQrHubView, meta: { title: 'Etiket / QR İşlemleri' } },
        { path: 'toplu-etiket', name: 'bulk-label', component: BulkLabelView, meta: { title: 'Toplu Etiket Bas' } },
        {
          path: 'toplu-qr',
          name: 'bulk-qr',
          component: BulkQrView,
          meta: { title: 'Toplu QR Bas' },
        },
        {
          path: 'garanti',
          redirect: { name: 'dashboard', query: { panel: 'warranty' } },
        },
        {
          path: 'raporlar',
          name: 'reports',
          component: ReportsView,
          meta: { title: 'Raporlar & Analiz' },
        },
        {
          path: 'son-hareketler',
          name: 'activity',
          component: AuditLogView,
          meta: { title: 'Son Hareketler', roles: [Roles.Admin] },
        },
        { path: 'son-hareketler/:id', name: 'activity-detail', component: AuditDetailView, meta: { title: 'Hareket Detayı', roles: [Roles.Admin] } },
        {
          path: 'kullanici-yonetimi',
          name: 'users',
          component: UserManagementView,
          meta: { title: 'Kullanıcı Yönetimi', roles: [Roles.Admin] },
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

router.beforeEach(async (to) => {
  if (to.meta.public) return authStore.isAuthenticated.value ? '/' : true
  await authStore.initialize()
  if (!authStore.isAuthenticated.value) return { name: 'login', query: { redirect: to.fullPath } }
  const roles = to.meta.roles as Role[] | undefined
  if (roles?.length && !authStore.hasRole(...roles)) return '/'
  return true
})

router.afterEach((to) => {
  document.title = `${String(to.meta.title ?? 'Dashboard')} | Envanter Sistemi`
})

export default router
