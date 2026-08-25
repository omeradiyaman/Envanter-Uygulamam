<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import DeviceLabelModal from '../components/DeviceLabelModal.vue'
import WorkflowSteps from '../components/WorkflowSteps.vue'
import { getDeviceCategories, getDevices, type DeviceCategory, type DeviceListItem } from '../services/deviceService'

const devices = ref<DeviceListItem[]>([])
const categories = ref<DeviceCategory[]>([])
const selectedIds = ref<string[]>([])
const search = ref('')
const categoryId = ref('')
const loading = ref(true)
const errorMessage = ref('')
const showPreview = ref(false)
const abortController = new AbortController()
const filtered = computed(() => {
  const query = search.value.toLocaleLowerCase('tr-TR').trim()
  const category = categories.value.find(item => item.id === categoryId.value)?.name
  return devices.value.filter(device => (!category || device.categoryName === category) && (!query || [device.cihazAdi, device.marka, device.model, device.seriNo, device.envanterNo].join(' ').toLocaleLowerCase('tr-TR').includes(query)))
})
const selectedDevices = computed(() => devices.value.filter(device => selectedIds.value.includes(device.id)))
const allSelected = computed(() => filtered.value.length > 0 && filtered.value.every(device => selectedIds.value.includes(device.id)))
function toggleAll() { const ids = filtered.value.map(item => item.id); selectedIds.value = allSelected.value ? selectedIds.value.filter(id => !ids.includes(id)) : [...new Set([...selectedIds.value, ...ids])] }
onMounted(async () => { try { [devices.value, categories.value] = await Promise.all([getDevices(abortController.signal), getDeviceCategories(abortController.signal)]) } catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Cihazlar yüklenemedi.' } finally { loading.value = false } })
onBeforeUnmount(() => abortController.abort())
</script>

<template>
  <section class="workflow-page qr-workflow-page">
    <header class="inventory-page-hero"><span class="inventory-page-hero__visual"><i class="bi bi-tags-fill"></i></span><div class="inventory-page-hero__main"><span class="inventory-page-hero__eyebrow">YAZDIRILABİLİR CİHAZ ETİKETLERİ</span><h1 class="inventory-page-hero__title">Toplu Etiket Bas</h1><p class="inventory-page-hero__description">Envanterden cihaz seçin ve standart ölçülü QR etiketlerini toplu yazdırın.</p></div><RouterLink to="/etiket-qr-islemleri" class="btn btn-light border"><i class="bi bi-arrow-left me-1"></i>Merkeze Dön</RouterLink></header>
    <AppAlert v-if="errorMessage" variant="danger" dismissible @dismiss="errorMessage = ''">{{ errorMessage }}</AppAlert>
    <section class="workflow-card"><WorkflowSteps :steps="['Cihazları seç', 'Etiket önizle', 'Yazdır']" :current="showPreview ? 2 : selectedIds.length ? 1 : 0" />
      <div class="qr-record-toolbar"><label class="search-box"><i class="bi bi-search"></i><span class="visually-hidden">Cihaz ara</span><input v-model="search" class="form-control" type="search" placeholder="Cihaz, seri veya envanter no ara..."></label><select v-model="categoryId" class="form-select"><option value="">Tüm Kategoriler</option><option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option></select><button class="btn btn-light border" type="button" @click="toggleAll"><i class="bi bi-check2-square me-1"></i>{{ allSelected ? 'Seçimi Kaldır' : 'Görünenlerin Tümünü Seç' }}</button><button class="btn btn-light border" type="button" :disabled="!selectedIds.length" @click="selectedIds = []">Temizle</button><span class="qr-selection-count"><strong>{{ selectedIds.length }}</strong> seçili</span></div>
      <DataState v-if="loading" type="loading" title="Cihazlar yükleniyor" /><DataState v-else-if="!filtered.length" type="no-results" title="Cihaz bulunamadı" />
      <div v-else class="qr-device-grid"><label v-for="device in filtered" :key="device.id" class="qr-device-card" :class="{ 'is-selected': selectedIds.includes(device.id) }"><input v-model="selectedIds" type="checkbox" :value="device.id"><span class="qr-device-card__icon"><i class="bi bi-tag"></i></span><span class="qr-device-card__body"><strong>{{ device.cihazAdi }}</strong><small>{{ device.categoryName }} · {{ device.marka }} {{ device.model }}</small><code>{{ device.envanterNo }} · {{ device.seriNo }}</code></span><i class="bi bi-check-circle-fill qr-device-card__check"></i></label></div>
      <div class="workflow-action-row"><span class="text-secondary small">Etiket ölçüsü merkezi CSS değişkenlerinden yönetilir.</span><button class="btn btn-primary" type="button" :disabled="!selectedIds.length" @click="showPreview = true"><i class="bi bi-eye-fill me-2"></i>Seçilenleri Önizle</button></div>
    </section>
    <DeviceLabelModal :show="showPreview" :devices="selectedDevices" @close="showPreview = false" />
  </section>
</template>
