<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import PageHeader from '../components/PageHeader.vue'
import WorkflowSteps from '../components/WorkflowSteps.vue'
import { getDeviceCategories, getDeviceQrCode, getDevices, type DeviceCategory, type DeviceListItem, type DeviceQrCode } from '../services/deviceService'

interface QrPreview { device: DeviceListItem; qr: DeviceQrCode }

const devices = ref<DeviceListItem[]>([])
const categories = ref<DeviceCategory[]>([])
const selectedIds = ref<string[]>([])
const previews = ref<QrPreview[]>([])
const search = ref('')
const categoryId = ref('')
const loading = ref(true)
const generating = ref(false)
const errorMessage = ref('')
const abortController = new AbortController()
const route = useRoute()

const filtered = computed(() => {
  const query = search.value.toLocaleLowerCase('tr-TR').trim()
  const category = categories.value.find(item => item.id === categoryId.value)?.name
  return devices.value.filter(device => (!category || device.categoryName === category) && (!query || [device.cihazAdi, device.marka, device.model, device.seriNo, device.envanterNo].join(' ').toLocaleLowerCase('tr-TR').includes(query)))
})
const allSelected = computed(() => filtered.value.length > 0 && filtered.value.every(device => selectedIds.value.includes(device.id)))
const currentStep = computed(() => previews.value.length ? 3 : selectedIds.value.length ? 2 : 1)

function toggleAll() {
  const visibleIds = filtered.value.map(device => device.id)
  selectedIds.value = allSelected.value ? selectedIds.value.filter(id => !visibleIds.includes(id)) : [...new Set([...selectedIds.value, ...visibleIds])]
}

async function generatePreviews() {
  generating.value = true
  errorMessage.value = ''
  previews.value = []
  const selected = devices.value.filter(device => selectedIds.value.includes(device.id))
  try {
    const output: QrPreview[] = []
    for (let index = 0; index < selected.length; index += 4) {
      const batch = selected.slice(index, index + 4)
      const codes = await Promise.all(batch.map(device => getDeviceQrCode(device.id)))
      output.push(...batch.map((device, batchIndex) => ({ device, qr: codes[batchIndex]! })))
    }
    previews.value = output
  } catch (error: unknown) {
    errorMessage.value = error instanceof Error ? error.message : 'QR kodları oluşturulamadı.'
  } finally {
    generating.value = false
  }
}

function escapeHtml(value: string) {
  return value.replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]!)
}

function printQrCodes() {
  const popup = window.open('', '_blank')
  if (!popup) { errorMessage.value = 'Yazdırma penceresi engellendi. Tarayıcı pop-up iznini kontrol edin.'; return }
  popup.opener = null
  const cards = previews.value.map(({ device, qr }) => `<article><img src="data:image/png;base64,${qr.pngBase64}" alt="QR"><strong>${escapeHtml(device.cihazAdi)}</strong><span>${escapeHtml(device.envanterNo)}</span><small>${escapeHtml(device.seriNo)}</small></article>`).join('')
  popup.document.write(`<!doctype html><html lang="tr"><head><meta charset="utf-8"><title>Toplu QR Baskısı</title><style>*{box-sizing:border-box}body{margin:0;padding:24px;font-family:Arial;color:#111827;background:#eef2f7}.head{display:flex;justify-content:space-between;align-items:center;max-width:1100px;margin:0 auto 20px;padding:16px 20px;border-radius:14px;background:#fff}.head h1{margin:0;font-size:20px}.head p{margin:4px 0 0;color:#64748b}.head button{padding:10px 16px;border:0;border-radius:9px;color:#fff;background:#4f46e5;font-weight:700}.sheet{display:grid;max-width:1100px;margin:auto;grid-template-columns:repeat(4,46mm);gap:5mm;justify-content:center}article{width:46mm;height:52mm;padding:3mm;overflow:hidden;break-inside:avoid;border:1px solid #cbd5e1;border-radius:2.5mm;background:#fff;text-align:center}article img{display:block;width:30mm;height:30mm;margin:0 auto 1.5mm}article strong,article span,article small{display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}article strong{font-size:8pt}article span{font-size:6.5pt;color:#475569}article small{font-size:6pt;color:#64748b}@page{size:A4 portrait;margin:8mm}@media print{body{padding:0;background:#fff}.head{display:none}.sheet{grid-template-columns:repeat(4,46mm);gap:5mm}}</style></head><body><header class="head"><div><h1>Toplu QR Baskısı</h1><p>${previews.value.length} QR kodu hazır</p></div><button onclick="window.print()">Yazdır</button></header><main class="sheet">${cards}</main></body></html>`)
  popup.document.close()
}

onMounted(async () => {
  try { [devices.value, categories.value] = await Promise.all([getDevices(abortController.signal), getDeviceCategories(abortController.signal)]) }
  catch (error: unknown) { if (!abortController.signal.aborted) errorMessage.value = error instanceof Error ? error.message : 'Cihazlar yüklenemedi.' }
  finally {
    const requestedCategory = typeof route.query.category === 'string' ? route.query.category : ''
    if (categories.value.some(category => category.id === requestedCategory)) categoryId.value = requestedCategory
    const requested = typeof route.query.ids === 'string' ? route.query.ids.split(',').filter(Boolean) : []
    selectedIds.value = requested.filter(id => devices.value.some(device => device.id === id))
    if (route.query.all === '1' && categoryId.value) selectedIds.value = filtered.value.map(device => device.id)
    loading.value = false
  }
})
onBeforeUnmount(() => abortController.abort())
</script>

<template>
  <section class="workflow-page qr-workflow-page">
    <PageHeader title="Toplu QR Bas" description="Envanterden cihaz seçin, cihaz detay URL’lerini içeren QR kodlarını önizleyin ve toplu yazdırın." />
    <AppAlert v-if="errorMessage" variant="danger" dismissible @dismiss="errorMessage = ''">{{ errorMessage }}</AppAlert>
    <section class="workflow-card">
      <WorkflowSteps :steps="['Cihazları seç', 'QR oluştur', 'Önizle ve yazdır']" :current="currentStep" />
      <div class="qr-section-heading"><span>1</span><div><h2>Envanterden cihaz seçin</h2><p>Kategori veya arama ile listeyi daraltın; tek tek ya da toplu seçim yapın.</p></div></div>

      <div class="qr-record-toolbar">
        <label class="search-box"><i class="bi bi-search"></i><span class="visually-hidden">Cihaz ara</span><input v-model="search" class="form-control" type="search" placeholder="Cihaz, seri veya envanter no ara..."></label>
        <select v-model="categoryId" class="form-select" aria-label="Kategori filtresi"><option value="">Tüm Kategoriler</option><option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option></select>
        <button type="button" class="btn btn-light border" @click="toggleAll"><i class="bi bi-check2-square me-1"></i>{{ allSelected ? 'Görünen Seçimi Kaldır' : 'Görünenlerin Tümünü Seç' }}</button>
        <button type="button" class="btn btn-light border" :disabled="selectedIds.length === 0" @click="selectedIds = []; previews = []">Seçimi Temizle</button>
        <span class="qr-selection-count"><strong>{{ selectedIds.length }}</strong> seçili</span>
      </div>

      <DataState v-if="loading" type="loading" title="Cihazlar yükleniyor" />
      <DataState v-else-if="filtered.length === 0" type="no-results" title="Cihaz bulunamadı" message="Arama veya kategori filtresini değiştirin." />
      <div v-else class="qr-device-grid">
        <label v-for="device in filtered" :key="device.id" class="qr-device-card" :class="{ 'is-selected': selectedIds.includes(device.id) }">
          <input v-model="selectedIds" type="checkbox" :value="device.id">
          <span class="qr-device-card__icon"><i class="bi bi-pc-display-horizontal"></i></span>
          <span class="qr-device-card__body"><strong>{{ device.cihazAdi }}</strong><small>{{ device.categoryName }} · {{ device.marka }} {{ device.model }}</small><code>{{ device.envanterNo }} · {{ device.seriNo }}</code></span>
          <i class="bi bi-check-circle-fill qr-device-card__check"></i>
        </label>
      </div>

      <div class="workflow-action-row"><span class="text-secondary small">Her QR yalnızca cihaz detay URL’sini içerir.</span><button type="button" class="btn btn-primary" :disabled="selectedIds.length === 0 || generating" @click="generatePreviews"><span v-if="generating" class="spinner-border spinner-border-sm me-2"></span><i v-else class="bi bi-qr-code me-2"></i>{{ generating ? 'QR Kodları Hazırlanıyor...' : 'Seçilenlerden QR Oluştur' }}</button></div>
    </section>

    <section v-if="previews.length" class="workflow-card qr-preview-section">
      <div class="qr-preview-header"><div class="qr-section-heading mb-0"><span>3</span><div><h2>Önizleyin ve yazdırın</h2><p>QR kartlarını kontrol edin; ardından tek seferde yazdırın.</p></div></div><div class="qr-preview-actions"><span>{{ previews.length }} QR</span><button class="btn btn-light border" @click="previews = []">Temizle</button><button class="btn btn-success" @click="printQrCodes"><i class="bi bi-printer-fill me-1"></i>Toplu Yazdır</button></div></div>
      <div class="qr-preview-grid"><article v-for="(item, index) in previews" :key="item.device.id" class="qr-preview-card"><span class="qr-preview-card__number">{{ index + 1 }}</span><img :src="`data:image/png;base64,${item.qr.pngBase64}`" :alt="`${item.device.cihazAdi} QR kodu`"><div><strong>{{ item.device.cihazAdi }}</strong><small>{{ item.device.categoryName }} · {{ item.device.seriNo }}</small><a :href="item.qr.deviceUrl" target="_blank" rel="noopener">Bağlantıyı kontrol et <i class="bi bi-box-arrow-up-right"></i></a></div></article></div>
    </section>
  </section>
</template>
