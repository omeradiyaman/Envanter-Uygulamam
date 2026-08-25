<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import DeviceFormModal from '../components/DeviceFormModal.vue'
import { getDevice, getDeviceCategories, getDevices, updateDevice, type Device, type DeviceCategory, type DeviceListItem, type DevicePayload } from '../services/deviceService'
import { exportDeviceCategoryExcel } from '../services/excelService'
import { authStore } from '../stores/authStore'

const route = useRoute()
const categoryId = computed(() => String(route.params.categoryId ?? ''))
const categories = ref<DeviceCategory[]>([])
const devices = ref<DeviceListItem[]>([])
const search = ref('')
const loading = ref(true)
const saving = ref(false)
const exporting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const editing = ref<Device | null>(null)
const controller = new AbortController()

const category = computed(() => categories.value.find(item => item.id === categoryId.value) ?? null)
const rows = computed(() => {
  const query = search.value.toLocaleLowerCase('tr-TR').trim()
  return devices.value.filter(item => item.categoryName === category.value?.name && item.status !== 4 && (!query || [item.cihazAdi, item.marka, item.model, item.seriNo, item.envanterNo, item.personelName ?? ''].join(' ').toLocaleLowerCase('tr-TR').includes(query)))
})

async function load() {
  loading.value = true
  errorMessage.value = ''
  try { [devices.value, categories.value] = await Promise.all([getDevices(controller.signal), getDeviceCategories(controller.signal)]) }
  catch (error: unknown) { if (!controller.signal.aborted) errorMessage.value = error instanceof Error ? error.message : 'Envanter tablosu yüklenemedi.' }
  finally { loading.value = false }
}

async function edit(id: string) {
  errorMessage.value = ''
  try { editing.value = await getDevice(id) }
  catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Cihaz bilgileri alınamadı.' }
}

async function save(payload: DevicePayload) {
  if (!editing.value) return
  saving.value = true
  try {
    await updateDevice(editing.value.id, payload)
    editing.value = null
    successMessage.value = 'Cihaz satırı güncellendi.'
    await load()
  } catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Cihaz güncellenemedi.' }
  finally { saving.value = false }
}

async function exportExcel() {
  if (!category.value) return
  exporting.value = true
  try { await exportDeviceCategoryExcel(category.value.id, category.value.name) }
  catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Excel dosyası indirilemedi.' }
  finally { exporting.value = false }
}

onMounted(load)
onBeforeUnmount(() => controller.abort())
</script>

<template>
  <section class="inventory-editor-page">
    <header class="inventory-page-hero">
      <span class="inventory-page-hero__visual"><i class="bi bi-pencil-square"></i></span>
      <div class="inventory-page-hero__main"><span class="inventory-page-hero__eyebrow">ODAK ÇALIŞMA ALANI</span><h1 class="inventory-page-hero__title">{{ category?.name ?? 'Envanter' }} Tablosunu Düzenle</h1><p class="inventory-page-hero__description">Kategori tablosunu yeni sekmede inceleyin ve kayıtları güvenli cihaz formuyla güncelleyin.</p></div>
      <div class="inventory-page-hero__actions"><RouterLink :to="{ name: 'inventories', query: { category: categoryId } }" class="btn btn-light border"><i class="bi bi-arrow-left me-1"></i>Envantere Dön</RouterLink><button type="button" class="btn btn-success" :disabled="exporting || !category" @click="exportExcel"><i class="bi bi-file-earmark-excel-fill me-1"></i>Excel'e Aktar</button></div>
    </header>
    <AppAlert v-if="successMessage" variant="success" dismissible @dismiss="successMessage = ''">{{ successMessage }}</AppAlert>
    <AppAlert v-if="errorMessage" variant="danger" dismissible @dismiss="errorMessage = ''">{{ errorMessage }}</AppAlert>
    <section class="card content-card inventory-table-card">
      <div class="table-card-header"><div><h2 class="table-card-header__title"><i class="bi bi-table me-2"></i>{{ category?.name }} Listesi</h2><p class="table-card-header__meta mb-0">{{ rows.length }} kayıt · Bir satırı açmak için çift tıklayabilirsiniz.</p></div><label class="search-box"><i class="bi bi-search"></i><span class="visually-hidden">Tabloda ara</span><input v-model="search" class="form-control" type="search" placeholder="Cihaz, seri veya envanter no ara..."></label></div>
      <DataState v-if="loading" type="loading" title="Tablo yükleniyor" />
      <DataState v-else-if="!category" type="error" title="Envanter bulunamadı" message="Kategori silinmiş veya bağlantı geçersiz olabilir." />
      <DataState v-else-if="!rows.length" type="no-results" title="Kayıt bulunamadı" />
      <div v-else class="table-responsive inventory-table-wrap"><table class="table data-table inventory-data-table align-middle mb-0"><thead><tr><th>Cihaz</th><th>Marka / Model</th><th>Seri No</th><th>Envanter No</th><th>Kullanıcı</th><th>Durum</th><th class="text-end">İşlemler</th></tr></thead><tbody><tr v-for="item in rows" :key="item.id" @dblclick="edit(item.id)"><td><strong>{{ item.cihazAdi }}</strong></td><td>{{ item.marka }} <span class="text-secondary">{{ item.model }}</span></td><td><code>{{ item.seriNo }}</code></td><td>{{ item.envanterNo }}</td><td>{{ item.personelName ?? 'IT Depo' }}</td><td><span class="badge text-bg-light border">{{ item.statusName }}</span></td><td class="text-end"><RouterLink :to="{ name: 'device-detail', params: { id: item.id } }" target="_blank" class="btn btn-sm action-button" title="Detayı yeni sekmede aç"><i class="bi bi-box-arrow-up-right"></i></RouterLink><button v-if="authStore.canEdit.value" type="button" class="btn btn-sm btn-warning ms-1" title="Satırı düzenle" @click="edit(item.id)"><i class="bi bi-pencil-square me-1"></i>Düzenle</button></td></tr></tbody></table></div>
    </section>
    <DeviceFormModal :show="Boolean(editing)" :device="editing" :categories="categories" :saving="saving" :error-message="errorMessage" @close="editing = null" @submit="save" />
  </section>
</template>
