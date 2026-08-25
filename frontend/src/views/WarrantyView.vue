<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import PageHeader from '../components/PageHeader.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { getDevicesWithoutWarranty, getExpiredWarranties, getExpiringWarranties, getWarrantySummary, type WarrantyAlertDevice, type WarrantySummary } from '../services/deviceService'

type Tab = 'expiring' | 'expired' | 'missing'
const activeTab = ref<Tab>('expiring')
const summary = ref<WarrantySummary | null>(null)
const expiring = ref<WarrantyAlertDevice[]>([])
const expired = ref<WarrantyAlertDevice[]>([])
const missing = ref<WarrantyAlertDevice[]>([])
const loading = ref(true)
const errorMessage = ref('')
const search = ref('')
const abortController = new AbortController()
const currentItems = computed(() => activeTab.value === 'expiring' ? expiring.value : activeTab.value === 'expired' ? expired.value : missing.value)
const filteredItems = computed(() => { const query = search.value.toLocaleLowerCase('tr-TR').trim(); return currentItems.value.filter(item => !query || [item.cihazAdi, item.seriNo, item.envanterNo].join(' ').toLocaleLowerCase('tr-TR').includes(query)) })
function statusVariant(): 'warning' | 'danger' | 'neutral' { return activeTab.value === 'expiring' ? 'warning' : activeTab.value === 'expired' ? 'danger' : 'neutral' }
function formatDate(value: string | null) { return value ? new Intl.DateTimeFormat('tr-TR', { dateStyle: 'medium' }).format(new Date(value)) : '—' }
onMounted(async () => { try { [summary.value, expiring.value, expired.value, missing.value] = await Promise.all([getWarrantySummary(abortController.signal), getExpiringWarranties(abortController.signal), getExpiredWarranties(abortController.signal), getDevicesWithoutWarranty(abortController.signal)]) } catch (error: unknown) { if (!abortController.signal.aborted) errorMessage.value = error instanceof Error ? error.message : 'Garanti verileri yüklenemedi.' } finally { loading.value = false } })
onBeforeUnmount(() => abortController.abort())
</script>

<template>
  <section class="workflow-page warranty-page">
    <PageHeader title="Garanti Merkezi" description="Garanti süresi yaklaşan, biten veya garanti bilgisi olmayan cihazları tek ekrandan izleyin." />
    <AppAlert v-if="errorMessage" variant="danger">{{ errorMessage }}</AppAlert>
    <DataState v-if="loading" type="loading" title="Garanti verileri yükleniyor" />
    <template v-else>
      <div class="warranty-alert-grid">
        <button type="button" class="is-warning" :class="{ 'is-active': activeTab === 'expiring' }" @click="activeTab = 'expiring'"><span><i class="bi bi-hourglass-split"></i></span><div><strong>{{ summary?.expiringCount ?? 0 }}</strong><small>{{ summary?.expiringSoonDays ?? 90 }} gün içinde bitecek</small></div><i class="bi bi-arrow-right"></i></button>
        <button type="button" class="is-danger" :class="{ 'is-active': activeTab === 'expired' }" @click="activeTab = 'expired'"><span><i class="bi bi-shield-x"></i></span><div><strong>{{ summary?.expiredCount ?? 0 }}</strong><small>Garantisi bitmiş</small></div><i class="bi bi-arrow-right"></i></button>
        <button type="button" class="is-neutral" :class="{ 'is-active': activeTab === 'missing' }" @click="activeTab = 'missing'"><span><i class="bi bi-shield-exclamation"></i></span><div><strong>{{ summary?.missingCount ?? 0 }}</strong><small>Garanti bilgisi yok</small></div><i class="bi bi-arrow-right"></i></button>
      </div>
      <section class="workflow-card">
        <div class="workflow-card__header"><div><h2><i class="bi bi-shield-check"></i>{{ activeTab === 'expiring' ? 'Yakında Bitecek Garantiler' : activeTab === 'expired' ? 'Süresi Dolmuş Garantiler' : 'Garanti Bilgisi Olmayanlar' }}</h2><p>{{ filteredItems.length }} cihaz gösteriliyor</p></div><label class="search-box"><i class="bi bi-search"></i><span class="visually-hidden">Garanti cihazı ara</span><input v-model="search" class="form-control" type="search" placeholder="Cihaz, seri veya envanter no ara..."></label></div>
        <DataState v-if="filteredItems.length === 0" type="empty" title="Bu durumda cihaz bulunamadı" icon="bi-shield-check" />
        <div v-else class="table-responsive"><table class="table data-table align-middle mb-0"><thead><tr><th>Cihaz</th><th>Seri No</th><th>Envanter No</th><th>Garanti Bitişi</th><th>Durum</th><th class="text-end">İşlem</th></tr></thead><tbody><tr v-for="device in filteredItems" :key="device.id"><td><strong>{{ device.cihazAdi }}</strong></td><td><code>{{ device.seriNo }}</code></td><td>{{ device.envanterNo }}</td><td>{{ formatDate(device.warrantyEndDate) }}</td><td><StatusBadge :label="device.warrantyStatusName" :variant="statusVariant()" /><small class="d-block text-secondary mt-1">{{ device.warrantyMessage }}</small></td><td class="text-end"><RouterLink :to="`/devices/${device.id}`" class="btn btn-sm btn-light border"><i class="bi bi-eye me-1"></i>Detay</RouterLink></td></tr></tbody></table></div>
      </section>
    </template>
  </section>
</template>
