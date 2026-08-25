<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useSidebar } from '../composables/useSidebar'
import { authStore } from '../stores/authStore'

defineProps<{ collapsed: boolean }>()
const emit = defineEmits<{ navigate: [] }>()
const route = useRoute()
const { toggleCollapsed } = useSidebar()
const openFlyout = ref<string | null>(null)

type NavItem = { label: string; icon: string; to: string; editOnly?: boolean; adminOnly?: boolean; accent?: string }
type NavFlyout = NavItem & { key: string; children: Array<{ label: string; description: string; icon: string; to: string; tone: string; editOnly?: boolean }> }

const mainMenu: NavItem[] = [
  { label: 'Dashboard', icon: 'bi-grid-1x2-fill', to: '/' },
  { label: 'Envanter', icon: 'bi-hdd-network-fill', to: '/envanterler' },
  { label: 'Envanter Yönetimi', icon: 'bi-sliders', to: '/envanter-yonetimi', editOnly: true },
  { label: 'Cihaz Ekle', icon: 'bi-plus-circle-fill', to: '/cihazlar/yeni', editOnly: true },
]

const excelMenu: NavFlyout = {
  key: 'excel', label: 'Excel İşlemleri', icon: 'bi-file-earmark-spreadsheet-fill', to: '/excel-islemleri',
  children: [
    { label: 'Excel Export', description: 'Verileri Excel olarak indirin', icon: 'bi-file-earmark-arrow-down-fill', to: '/excel-islemleri/export', tone: 'export' },
    { label: 'Excel Import', description: 'Dosyadan veri ekleyin veya güncelleyin', icon: 'bi-file-earmark-arrow-up-fill', to: '/excel-islemleri/import', tone: 'import', editOnly: true },
  ],
}
const qrMenu: NavFlyout = {
  key: 'qr', label: 'Etiket / QR İşlemleri', icon: 'bi-qr-code-scan', to: '/etiket-qr-islemleri', accent: 'warning', editOnly: true,
  children: [
    { label: 'Toplu Etiket Bas', description: 'Envanterden etiket hazırlayın', icon: 'bi-tags-fill', to: '/toplu-etiket', tone: 'label' },
    { label: 'Toplu QR Bas', description: 'Cihaz QR kodlarını hazırlayın', icon: 'bi-qr-code', to: '/toplu-qr', tone: 'qr' },
  ],
}
const managementMenu: Array<NavItem | NavFlyout> = [
  { label: 'Personel', icon: 'bi-people-fill', to: '/personeller' },
  excelMenu,
  { label: 'Zimmet Belgesi', icon: 'bi-file-earmark-text-fill', to: '/zimmet', editOnly: true, accent: 'success' },
  qrMenu,
  { label: 'Raporlar', icon: 'bi-bar-chart-fill', to: '/raporlar' },
  { label: 'Son Hareketler', icon: 'bi-clock-history', to: '/son-hareketler', adminOnly: true },
]

const visibleMain = computed(() => mainMenu.filter(visible))
const visibleManagement = computed(() => managementMenu.filter(visible))
const systemMenu = computed<NavItem[]>(() => authStore.isAdmin.value ? [{ label: 'Kullanıcı Yönetimi', icon: 'bi-shield-lock-fill', to: '/kullanici-yonetimi', adminOnly: true }] : [])
function visible(item: NavItem) { return (!item.editOnly || authStore.canEdit.value) && (!item.adminOnly || authStore.isAdmin.value) }
function isFlyout(item: NavItem | NavFlyout): item is NavFlyout { return 'children' in item }
function isActive(item: NavItem) {
  if (item.to === '/') return route.path === '/'
  if (item.to === '/envanterler') return route.path.startsWith('/envanterler') || ['/stok', '/hurda-imha', '/cihazlar'].includes(route.path)
  if (item.to === '/excel-islemleri') return route.path.startsWith('/excel-islemleri')
  if (item.to === '/etiket-qr-islemleri') return route.path.startsWith('/etiket-qr-islemleri') || route.path.startsWith('/toplu-etiket') || route.path.startsWith('/toplu-qr')
  if (item.to === '/zimmet') return route.path.startsWith('/zimmet')
  if (item.to === '/personeller') return route.path.startsWith('/personeller')
  return route.path === item.to || route.path.startsWith(`${item.to}/`)
}
function navigate() { openFlyout.value = null; emit('navigate') }
</script>

<template>
  <aside class="app-sidebar" :class="{ collapsed }">
    <div class="sidebar-brand"><span class="brand-icon"><img src="/favicon.svg" alt="IT Envanter"></span><div class="brand-text"><strong>IT Envanter</strong><small>Yönetim Sistemi</small></div></div>
    <nav class="sidebar-nav" aria-label="Ana menü">
      <div class="nav-section-label">Ana Menü</div>
      <RouterLink v-for="item in visibleMain" :key="item.to" :to="item.to" class="nav-item" :class="{ active: isActive(item) }" :title="collapsed ? item.label : undefined" @click="navigate"><i :class="['bi', item.icon]"></i><span>{{ item.label }}</span></RouterLink>
      <div class="nav-section-label nav-section-label--spaced">Yönetim</div>
      <template v-for="item in visibleManagement" :key="item.to">
        <div v-if="isFlyout(item)" class="nav-flyout-group" :class="{ 'is-open': openFlyout === item.key, active: isActive(item) }" :style="{ '--flyout-top': item.key === 'excel' ? '270px' : '365px' }">
          <div class="nav-flyout-trigger"><RouterLink :to="item.to" class="nav-item" :class="{ active: isActive(item) }" :title="collapsed ? item.label : undefined" @click="navigate"><i :class="['bi', item.icon, item.accent ? `text-${item.accent}` : '']"></i><span>{{ item.label }}</span></RouterLink><button type="button" class="nav-flyout-toggle" :aria-label="`${item.label} alt menüsünü aç`" :aria-expanded="openFlyout === item.key" @click="openFlyout = openFlyout === item.key ? null : item.key"><i class="bi bi-chevron-right"></i></button></div>
          <div class="nav-flyout" role="menu"><RouterLink v-for="child in item.children.filter(visible)" :key="child.to" :to="child.to" role="menuitem" @click="navigate"><span :class="`is-${child.tone}`"><i :class="['bi', child.icon]"></i></span><div><strong>{{ child.label }}</strong><small>{{ child.description }}</small></div><i class="bi bi-arrow-right"></i></RouterLink></div>
        </div>
        <RouterLink v-else :to="item.to" class="nav-item" :class="{ active: isActive(item) }" :title="collapsed ? item.label : undefined" @click="navigate"><i :class="['bi', item.icon, item.accent ? `text-${item.accent}` : '']"></i><span>{{ item.label }}</span></RouterLink>
      </template>
      <template v-if="systemMenu.length"><div class="nav-section-label nav-section-label--spaced">Sistem</div><RouterLink v-for="item in systemMenu" :key="item.to" :to="item.to" class="nav-item" :class="{ active: isActive(item) }" @click="navigate"><i :class="['bi', item.icon]"></i><span>{{ item.label }}</span></RouterLink></template>
    </nav>
    <div class="sidebar-footer"><span><i class="bi bi-circle-fill"></i><strong>Sistem Çevrimiçi</strong></span><button type="button" class="sidebar-collapse-btn d-none d-lg-grid" :aria-label="collapsed ? 'Menüyü genişlet' : 'Menüyü daralt'" @click="toggleCollapsed"><i :class="['bi', collapsed ? 'bi-chevron-right' : 'bi-chevron-left']"></i></button></div>
  </aside>
</template>

<style scoped>
.app-sidebar{position:fixed;z-index:1040;inset:0 auto 0 0;display:flex;width:var(--sidebar-width);flex-direction:column;color:var(--sidebar-text);border-right:1px solid var(--sidebar-border);background:radial-gradient(circle at 0 0,rgb(99 102 241/8%),transparent 38%),linear-gradient(180deg,var(--sidebar-bg),var(--sidebar-bg-end));box-shadow:8px 0 32px rgb(15 23 42/6%);transition:width .22s ease,transform .25s ease}.app-sidebar.collapsed{width:var(--sidebar-width-collapsed)}
.sidebar-brand{display:flex;min-height:var(--topbar-height);align-items:center;gap:.75rem;padding:.8rem 1rem;border-bottom:1px solid var(--sidebar-border);background:linear-gradient(135deg,rgb(99 102 241/9%),transparent 72%)}.brand-icon{display:grid;width:40px;height:40px;flex:0 0 40px;overflow:hidden;place-items:center;border-radius:12px;box-shadow:0 8px 24px rgb(79 70 229/25%)}.brand-icon img{width:100%;height:100%}.brand-text{min-width:0;white-space:nowrap;transition:opacity .15s ease}.brand-text strong,.brand-text small{display:block}.brand-text strong{color:var(--text-heading);font-size:.9rem}.brand-text small{color:var(--sidebar-text-muted);font-size:.62rem;letter-spacing:.04em}.collapsed .brand-text{width:0;opacity:0;overflow:hidden}
.sidebar-nav{flex:1;padding:.7rem .75rem 1rem;overflow-x:hidden;overflow-y:auto;scrollbar-color:var(--primary-muted) transparent;scrollbar-width:thin}.nav-section-label{position:relative;margin:.45rem .6rem .35rem;padding-left:.62rem;overflow:hidden;color:var(--sidebar-text-muted);font-size:.57rem;font-weight:800;letter-spacing:.12em;text-transform:uppercase;white-space:nowrap}.nav-section-label::before{position:absolute;top:50%;left:0;width:3px;height:3px;border-radius:50%;background:var(--primary);box-shadow:0 0 6px var(--primary-glow);content:'';transform:translateY(-50%)}.nav-section-label--spaced{margin-top:1rem}.collapsed .nav-section-label{height:0;margin:0;padding:0;opacity:0}
.nav-item{display:flex;min-height:40px;align-items:center;gap:.7rem;margin-bottom:3px;padding:.58rem .75rem;border:1px solid transparent;border-radius:10px;color:var(--sidebar-text);font-size:.75rem;font-weight:550;text-decoration:none;transition:color .18s ease,background .18s ease,border-color .18s ease}.nav-item>i{width:20px;flex:0 0 20px;font-size:1rem;text-align:center;opacity:.88}.nav-item>span{overflow:hidden;white-space:nowrap}.nav-item:hover{color:var(--text-heading);border-color:var(--sidebar-border);background:var(--sidebar-hover)}.nav-item.active{color:var(--sidebar-active-text);border-color:var(--primary-muted);background:var(--sidebar-active-bg);box-shadow:inset 3px 0 0 var(--primary),0 7px 20px rgb(99 102 241/8%);font-weight:700}.nav-item.active>i{color:var(--primary);opacity:1}.collapsed .nav-item{justify-content:center;padding-inline:.5rem}.collapsed .nav-item>span{display:none}
.nav-flyout-group{position:relative}.nav-flyout-trigger{position:relative}.nav-flyout-trigger>.nav-item{padding-right:2rem}.nav-flyout-toggle{position:absolute;top:4px;right:4px;display:grid;width:30px;height:32px;padding:0;place-items:center;border:0;border-radius:8px;color:var(--sidebar-text-muted);background:transparent;font-size:.65rem}.nav-flyout-toggle:hover{color:var(--primary);background:var(--primary-subtle)}.nav-flyout-group:is(:hover,:focus-within,.is-open) .nav-flyout-toggle i{transform:translateX(2px)}.nav-flyout{position:fixed;z-index:1400;top:var(--flyout-top);left:calc(var(--sidebar-width) - 5px);display:grid;width:286px;gap:.4rem;padding:.65rem;border:1px solid var(--border-color);border-radius:15px;opacity:0;visibility:hidden;color:var(--text-primary);background:var(--surface-elevated);box-shadow:0 22px 55px rgb(15 23 42/16%);pointer-events:none;transform:translateX(-8px) scale(.98);transform-origin:left center;transition:opacity .16s ease,transform .16s ease,visibility .16s ease}.nav-flyout::before{position:absolute;inset:0 auto 0 -12px;width:14px;content:''}.nav-flyout-group:is(:hover,:focus-within,.is-open) .nav-flyout{opacity:1;visibility:visible;pointer-events:auto;transform:translateX(0) scale(1)}:global(.sidebar-collapsed) .nav-flyout{left:calc(var(--sidebar-width-collapsed) - 5px)}.nav-flyout>a{display:grid;grid-template-columns:36px minmax(0,1fr) 14px;align-items:center;gap:.68rem;padding:.65rem;border:1px solid transparent;border-radius:11px;color:var(--text-primary);text-decoration:none}.nav-flyout>a:hover,.nav-flyout>a.router-link-active{border-color:var(--primary-muted);background:var(--primary-subtle)}.nav-flyout>a>span{display:grid;width:36px;height:36px;place-items:center;border-radius:10px}.nav-flyout .is-export{color:var(--success);background:var(--success-subtle)}.nav-flyout .is-import{color:var(--primary);background:var(--primary-subtle)}.nav-flyout .is-label{color:var(--warning);background:var(--warning-subtle)}.nav-flyout .is-qr{color:var(--info);background:var(--info-subtle)}.nav-flyout strong,.nav-flyout small{display:block}.nav-flyout strong{font-size:.76rem}.nav-flyout small{color:var(--text-muted);font-size:.63rem;line-height:1.35}.nav-flyout>a>i{color:var(--text-muted);font-size:.72rem}
.sidebar-footer{display:flex;align-items:center;justify-content:space-between;gap:.4rem;margin:.5rem .75rem .75rem;padding:.55rem .65rem;border:1px solid color-mix(in srgb,var(--success) 18%,var(--sidebar-border));border-radius:10px;background:color-mix(in srgb,var(--success) 6%,var(--surface))}.sidebar-footer>span{display:flex;align-items:center;gap:.4rem;overflow:hidden;color:var(--success-on);font-size:.62rem;white-space:nowrap}.sidebar-footer>span i{font-size:.42rem}.sidebar-collapse-btn{width:28px;height:28px;padding:0;place-items:center;border:1px solid var(--sidebar-border);border-radius:8px;color:var(--sidebar-text);background:transparent}.sidebar-collapse-btn:hover{color:var(--primary);background:var(--primary-subtle)}.collapsed .sidebar-footer>span{display:none}.collapsed .sidebar-footer{justify-content:center}
@media(max-width:991.98px){.app-sidebar,.app-sidebar.collapsed{width:var(--sidebar-width);transform:translateX(-100%)}:global(.sidebar-open .app-sidebar){transform:translateX(0)}.collapsed .brand-text{width:auto;opacity:1}.collapsed .nav-section-label{height:auto;margin:.45rem .6rem .35rem;padding-left:.62rem;opacity:1}.collapsed .nav-item{justify-content:flex-start;padding:.58rem .75rem}.collapsed .nav-item>span{display:block}.collapsed .nav-flyout-toggle{display:grid}.collapsed .sidebar-footer>span{display:flex}.nav-flyout{position:static;display:none;width:auto;margin:.15rem 0 .45rem .65rem;padding:.3rem;border:0;border-left:1px solid var(--primary-muted);border-radius:0;opacity:1;visibility:visible;background:transparent;box-shadow:none;pointer-events:auto;transform:none}.nav-flyout-group.is-open .nav-flyout{display:grid}.nav-flyout-group:not(.is-open) .nav-flyout{display:none}}
</style>
