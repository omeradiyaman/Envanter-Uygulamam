<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import DataState from '../components/DataState.vue'
import { actionLabels, getAuditLogs, type AuditLog } from '../services/auditLogService'
import { getDevices, getWarrantySummary, type DeviceListItem, type WarrantySummary } from '../services/deviceService'
import { getPersonnelList, type PersonnelListItem } from '../services/personnelService'
import { getSystemStatus, type SystemStatus } from '../services/systemService'
import { authStore } from '../stores/authStore'
import { inventoryVisual } from '../utils/inventoryVisuals'

const systemStatus = ref<SystemStatus | null>(null)
const warrantySummary = ref<WarrantySummary | null>(null)
const devices = ref<DeviceListItem[]>([])
const personnel = ref<PersonnelListItem[]>([])
const recentLogs = ref<AuditLog[]>([])
const errorMessage = ref('')
const loading = ref(true)
const logsLoading = ref(true)
const abortController = new AbortController()
const router = useRouter()

const assignedCount = computed(() => devices.value.filter((item) => item.status === 2).length)
const stockCount = computed(() => devices.value.filter((item) => item.status === 1).length)
const attentionCount = computed(() => devices.value.filter((item) => [3, 4, 5].includes(item.status)).length)
const activePersonnelCount = computed(() => personnel.value.filter((item) => item.aktifMi).length)
const assignmentRate = computed(() => devices.value.length ? Math.round((assignedCount.value / devices.value.length) * 100) : 0)

const inventoryCards = computed(() => [
  { label: 'Toplam cihaz', value: devices.value.length, icon: 'bi-pc-display', tone: 'primary', to: '/envanterler' },
  { label: 'Zimmetli', value: assignedCount.value, icon: 'bi-person-check', tone: 'success', to: '/zimmet' },
  { label: 'Stokta', value: stockCount.value, icon: 'bi-box-seam', tone: 'info', to: '/envanterler?view=stock' },
  { label: 'Aktif personel', value: activePersonnelCount.value, icon: 'bi-people', tone: 'violet', to: '/personeller' },
])

const categoryDistribution = computed(() => {
  const counts = devices.value.reduce<Record<string, number>>((result, item) => {
    result[item.categoryName] = (result[item.categoryName] ?? 0) + 1
    return result
  }, {})
  return Object.entries(counts).map(([name, count], index) => ({
    name,
    count,
    ratio: devices.value.length ? Math.round((count / devices.value.length) * 100) : 0,
    ...inventoryVisual(name, index),
  })).sort((a, b) => b.count - a.count).slice(0, 6)
})

const warrantyCards = computed(() => [
  { key: 'expiring' as const, label: `${warrantySummary.value?.expiringSoonDays ?? 90} gün içinde bitecek`, value: warrantySummary.value?.expiringCount ?? '—', icon: 'bi-hourglass-split', tone: 'warning' },
  { key: 'expired' as const, label: 'Garantisi bitmiş', value: warrantySummary.value?.expiredCount ?? '—', icon: 'bi-shield-x', tone: 'danger' },
  { key: 'missing' as const, label: 'Garanti bilgisi olmayan', value: warrantySummary.value?.missingCount ?? '—', icon: 'bi-shield-exclamation', tone: 'neutral' },
])

function formatDate(value: string) {
  return new Intl.DateTimeFormat('tr-TR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value))
}

function openWarrantyFilter(filter: 'expiring' | 'expired' | 'missing') {
  router.push({ name: 'inventories', query: { warranty: filter } })
}

onMounted(async () => {
  try {
    const [status, warranties, deviceList, personnelList, logs] = await Promise.all([
      getSystemStatus(abortController.signal),
      getWarrantySummary(abortController.signal),
      getDevices(abortController.signal),
      getPersonnelList(abortController.signal),
      getAuditLogs(undefined, abortController.signal).catch(() => []),
    ])
    systemStatus.value = status
    warrantySummary.value = warranties
    devices.value = deviceList
    personnel.value = personnelList
    recentLogs.value = logs.slice(0, 6)
  } catch (error: unknown) {
    if (!abortController.signal.aborted) errorMessage.value = error instanceof Error ? error.message : 'Veriler yüklenemedi.'
  } finally {
    loading.value = false
    logsLoading.value = false
  }
})

onBeforeUnmount(() => abortController.abort())
</script>

<template>
  <section class="modern-dashboard">
    <header class="dashboard-hero">
      <div class="dashboard-hero__content">
        <span class="dashboard-hero__eyebrow"><i class="bi bi-stars"></i> ENVANTER KONTROL MERKEZİ</span>
        <h1>Varlıkları tek ekrandan yönetin.</h1>
        <p>Envanteri takip edin, personele zimmetleyin ve güncel raporlara hızlıca ulaşın.</p>
        <div class="dashboard-hero__actions">
          <RouterLink to="/envanterler" class="btn btn-primary"><i class="bi bi-box-seam me-2"></i>Tüm envanteri görüntüle</RouterLink>
          <RouterLink v-if="authStore.canEdit.value" to="/cihazlar/yeni" class="btn btn-light border"><i class="bi bi-plus-circle me-2"></i>Yeni cihaz ekle</RouterLink>
        </div>
      </div>
      <div class="dashboard-hero__meter" aria-label="Zimmet oranı">
        <div class="dashboard-hero__ring" :style="{ '--progress': `${assignmentRate * 3.6}deg` }">
          <span><strong>{{ assignmentRate }}%</strong><small>Zimmet oranı</small></span>
        </div>
        <div class="dashboard-hero__summary">
          <span><i class="bi bi-check-circle-fill"></i> {{ assignedCount }} cihaz kullanımda</span>
          <span><i class="bi bi-box-seam"></i> {{ stockCount }} cihaz stokta</span>
        </div>
      </div>
    </header>

    <div class="dashboard-kpi-grid" aria-label="Envanter özeti">
      <RouterLink v-for="card in inventoryCards" :key="card.label" :to="card.to" class="dashboard-kpi" :class="`is-${card.tone}`">
        <span class="dashboard-kpi__icon"><i :class="['bi', card.icon]"></i></span>
        <span class="dashboard-kpi__body"><small>{{ card.label }}</small><strong>{{ loading ? '—' : card.value }}</strong></span>
        <i class="bi bi-arrow-up-right dashboard-kpi__arrow"></i>
      </RouterLink>
    </div>

    <div v-if="errorMessage" class="app-alert danger mb-4" role="alert"><div class="app-alert__content"><strong>Veriler yüklenemedi</strong><div>{{ errorMessage }}</div></div></div>

    <div class="dashboard-main-grid">
      <article class="dashboard-panel">
        <div class="dashboard-panel__header">
          <div><span class="section-kicker">ENVANTER DAĞILIMI</span><h2>Kategori görünümü</h2></div>
          <RouterLink to="/envanterler" class="panel-link">Tüm envanter <i class="bi bi-arrow-right"></i></RouterLink>
        </div>
        <DataState v-if="loading" type="loading" title="Dağılım yükleniyor" />
        <div v-else-if="categoryDistribution.length" class="distribution-list">
          <div v-for="category in categoryDistribution" :key="category.name" class="distribution-row">
            <span class="distribution-row__icon" :class="`tone-${category.tone}`"><i :class="['bi', category.icon]"></i></span>
            <div class="distribution-row__main"><div><strong>{{ category.name }}</strong><span>{{ category.count }} cihaz</span></div><div class="distribution-bar"><span :style="{ width: `${category.ratio}%` }"></span></div></div>
            <b>{{ category.ratio }}%</b>
          </div>
        </div>
        <DataState v-else type="empty" title="Henüz cihaz yok" message="Kategori dağılımı cihaz eklendiğinde görünecek." />
      </article>

      <article class="dashboard-panel">
        <div class="dashboard-panel__header"><div><span class="section-kicker">AKILLI UYARILAR</span><h2>Garanti takibi</h2></div><span class="panel-status"><i class="bi bi-shield-check"></i> Dashboard</span></div>
        <div class="warranty-alert-list">
          <button v-for="card in warrantyCards" :key="card.key" type="button" class="warranty-alert" :class="`is-${card.tone}`" @click="openWarrantyFilter(card.key)">
            <span class="warranty-alert__icon"><i :class="['bi', card.icon]"></i></span><span><strong>{{ card.value }}</strong><small>{{ card.label }}</small></span><i class="bi bi-chevron-right"></i>
          </button>
        </div>
        <div class="health-strip" :class="systemStatus ? 'is-online' : 'is-offline'"><span><i class="bi bi-circle-fill"></i> API {{ systemStatus ? 'çevrimiçi' : 'erişilemiyor' }}</span><small v-if="systemStatus">{{ systemStatus.service }} · v{{ systemStatus.apiVersion }}</small></div>
      </article>
    </div>

    <div class="dashboard-lower-grid">
      <article class="dashboard-panel">
        <div class="dashboard-panel__header"><div><span class="section-kicker">SON AKTİVİTE</span><h2>Son hareketler</h2></div><RouterLink v-if="authStore.isAdmin.value" to="/son-hareketler" class="panel-link">Tümünü gör <i class="bi bi-arrow-right"></i></RouterLink></div>
        <DataState v-if="logsLoading" type="loading" title="Hareketler yükleniyor" />
        <div v-else-if="recentLogs.length" class="activity-list activity-list--modern">
          <div v-for="log in recentLogs" :key="log.id" class="activity-item"><span class="activity-item__icon"><i class="bi bi-activity"></i></span><div class="min-w-0 flex-grow-1"><div class="activity-item__title cell-truncate">{{ log.description }}</div><div class="activity-item__meta">{{ actionLabels[log.actionType] ?? log.actionType }} · {{ log.userDisplayName }} · {{ formatDate(log.createdAt) }}</div></div></div>
        </div>
        <DataState v-else type="empty" title="Henüz hareket yok" message="Yeni işlemler burada görüntülenecek." />
      </article>

      <article class="dashboard-panel quick-actions-panel">
        <div class="dashboard-panel__header"><div><span class="section-kicker">KISAYOLLAR</span><h2>Hızlı işlemler</h2></div></div>
        <div class="quick-action-grid">
          <RouterLink v-if="authStore.canEdit.value" to="/cihazlar/yeni" class="quick-action"><i class="bi bi-plus-circle-fill"></i><span>Yeni Cihaz Ekle</span></RouterLink>
          <RouterLink to="/etiket-qr-islemleri" class="quick-action"><i class="bi bi-qr-code-scan"></i><span>Etiket / QR</span></RouterLink>
          <RouterLink to="/toplu-etiket" class="quick-action"><i class="bi bi-tags-fill"></i><span>Toplu Etiket Bas</span></RouterLink>
          <RouterLink to="/raporlar" class="quick-action"><i class="bi bi-bar-chart"></i><span>Raporlar</span></RouterLink>
          <RouterLink v-if="authStore.isAdmin.value" to="/son-hareketler" class="quick-action"><i class="bi bi-clock-history"></i><span>Son Hareketler</span></RouterLink>
        </div>
        <div class="attention-card"><span><i class="bi bi-exclamation-triangle"></i></span><div><strong>{{ attentionCount }} cihaz dikkat bekliyor</strong><small>Servis, hurda veya kayıp durumundaki cihazlar</small></div></div>
      </article>
    </div>
  </section>
</template>
