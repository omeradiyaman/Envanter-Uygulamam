<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ConfirmationModal from '../components/ConfirmationModal.vue'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import DeviceDetailModal from '../components/DeviceDetailModal.vue'
import DeviceFormModal from '../components/DeviceFormModal.vue'
import DeviceLabelModal from '../components/DeviceLabelModal.vue'
import {
  createDevice,
  deleteDevice,
  getDevice,
  getDevices,
  updateDevice,
  getDeviceCategories,
  getStatusColor,
  getWarrantyBadgeClass,
  type Device,
  type DeviceListItem,
  type DevicePayload,
  type DeviceCategory
} from '../services/deviceService'
import { authStore } from '../stores/authStore'
import { exportDeviceCategoryExcel, exportDevicesExcel, exportInventoryExcel } from '../services/excelService'
import { inventoryVisual } from '../utils/inventoryVisuals'

const route = useRoute()
const router = useRouter()

const devices = ref<DeviceListItem[]>([])
const categories = ref<DeviceCategory[]>([])
const loading = ref(true)
const loadError = ref('')
const searchTerm = ref('')
const successMessage = ref('')
const exporting = ref(false)

const filterCategoryId = ref<string>('')
const filterStatus = ref<string>('')
const warrantyFilter = computed(() => typeof route.query.warranty === 'string' ? route.query.warranty : '')
const requestedView = computed(() => typeof route.query.view === 'string' ? route.query.view : '')
const pageContent = computed(() => requestedView.value === 'stock'
  ? { eyebrow: 'IT DEPO YÖNETİMİ', title: 'Stok Envanteri', description: 'Stokta bulunan ve zimmete hazır cihazları görüntüleyin.', icon: 'bi-box-seam-fill', accent: 'success' }
  : requestedView.value === 'scrap'
    ? { eyebrow: 'YAŞAM DÖNGÜSÜ YÖNETİMİ', title: 'Hurda / İmha', description: 'Hurda veya imha durumundaki cihazları görüntüleyin.', icon: 'bi-archive-fill', accent: 'danger' }
    : route.name === 'inventories'
      ? { eyebrow: 'ENVANTER YÖNETİMİ', title: 'Tüm Varlıklar', description: 'Tüm envanter kategorilerini ve cihaz kayıtlarını tek ekranda görüntüleyin.', icon: 'bi-hdd-network-fill', accent: 'primary' }
      : { eyebrow: 'CİHAZ YÖNETİMİ', title: 'Cihazlar', description: 'Envanterdeki cihazları ve zimmet durumlarını yönetin.', icon: 'bi-pc-display-horizontal', accent: 'primary' })

const routeMode = computed(() => requestedView.value === 'scrap' ? 'scrap' : requestedView.value === 'stock' ? 'stock' : 'inventory')
const isInventoryOverview = computed(() => routeMode.value === 'inventory' && !filterCategoryId.value && !warrantyFilter.value && !searchTerm.value.trim())
const selectedCategory = computed(() => categories.value.find(category => category.id === filterCategoryId.value) ?? null)
const listTitle = computed(() => routeMode.value === 'scrap' ? 'Hurda Envanteri' : routeMode.value === 'stock' ? 'IT Depo / Stok' : selectedCategory.value?.name ?? 'Envanter')
const stockCount = computed(() => devices.value.filter((device) => device.status === 1).length)
const assignedCount = computed(() => devices.value.filter((device) => device.status === 2).length)
const scrapCount = computed(() => devices.value.filter((device) => device.status === 4).length)
const categorySummaries = computed(() => categories.value.map((category, index) => ({
  ...category,
  count: devices.value.filter((device) => device.categoryName === category.name && device.status !== 4).length,
  ...inventoryVisual(category.name, index),
})))

function chooseCategory(categoryId: string) {
  selectedDeviceIds.value = []
  void router.push({ name: 'inventories', query: { category: categoryId } })
}

function chooseScrapInventory() {
  selectedDeviceIds.value = []
  void router.push({ name: 'inventories', query: { view: 'scrap' } })
}

function returnToInventoryOverview() {
  selectedDeviceIds.value = []
  void router.push({ name: 'inventories' })
}

function applyRoutePreset() {
  filterStatus.value = requestedView.value === 'stock' ? '1' : requestedView.value === 'scrap' ? '4' : ''
}

const showFormModal = ref(false)
const editingDevice = ref<Device | null>(null)
const saving = ref(false)
const formError = ref('')

const showDetailModal = ref(false)
const detailDevice = ref<Device | null>(null)
const detailLoading = ref(false)
const detailError = ref('')

const showDeleteModal = ref(false)
const deletingDevice = ref<DeviceListItem | null>(null)
const deleting = ref(false)
const deleteError = ref('')
const selectedDeviceIds = ref<string[]>([])
const showBulkLabels = ref(false)
const showRangeModal = ref(false)
const rangeStart = ref(1)
const rangeEnd = ref(20)

const listAbortController = new AbortController()

const filteredDevices = computed(() => {
  const query = searchTerm.value.toLocaleLowerCase('tr-TR').trim()
  
  let result = devices.value

  if (filterCategoryId.value) {
    const category = categories.value.find(c => c.id === filterCategoryId.value)
    if (category) {
      result = result.filter(d => d.categoryName === category.name)
      result = result.filter(d => d.status !== 4)
    }
  }

  if (filterStatus.value) {
    result = result.filter(d => d.status.toString() === filterStatus.value)
  }

  const warrantyStatusByFilter: Record<string, number> = { missing: 0, expiring: 2, expired: 3 }
  if (warrantyFilter.value in warrantyStatusByFilter) {
    result = result.filter(device => device.warrantyStatus === warrantyStatusByFilter[warrantyFilter.value])
  }

  if (!query) {
    return result
  }

  return result.filter((item) =>
    [
      item.cihazAdi,
      item.seriNo,
      item.envanterNo,
      item.marka,
      item.model,
      item.categoryName,
      item.personelName ?? ''
    ]
      .join(' ')
      .toLocaleLowerCase('tr-TR')
      .includes(query),
  )
})

const selectedDevices = computed(() =>
  devices.value.filter((device) => selectedDeviceIds.value.includes(device.id)),
)

const allFilteredSelected = computed(() =>
  filteredDevices.value.length > 0 && filteredDevices.value.every((device) => selectedDeviceIds.value.includes(device.id)),
)

const hasActiveFilters = computed(
  () =>
    Boolean(searchTerm.value.trim()) ||
    Boolean(filterCategoryId.value) ||
    Boolean(filterStatus.value) ||
    Boolean(warrantyFilter.value),
)

function clearFilters() {
  searchTerm.value = ''
  applyRoutePreset()
  router.replace({ name: 'inventories', query: filterCategoryId.value ? { category: filterCategoryId.value } : requestedView.value ? { view: requestedView.value } : {} })
}

function selectRange() {
  const start = Math.max(1, Math.floor(rangeStart.value))
  const end = Math.min(filteredDevices.value.length, Math.max(start, Math.floor(rangeEnd.value)))
  const ids = filteredDevices.value.slice(start - 1, end).map(device => device.id)
  selectedDeviceIds.value = [...new Set([...selectedDeviceIds.value, ...ids])]
  showRangeModal.value = false
}

async function exportCurrentCategory() {
  if (!selectedCategory.value) return
  exporting.value = true
  loadError.value = ''
  try { await exportDeviceCategoryExcel(selectedCategory.value.id, selectedCategory.value.name) }
  catch (error: unknown) { loadError.value = getErrorMessage(error, 'Kategori Excel dosyası indirilemedi.') }
  finally { exporting.value = false }
}

function openCategoryQr() {
  if (!selectedCategory.value) return
  void router.push({ name: 'bulk-qr', query: { category: selectedCategory.value.id, all: '1' } })
}

async function exportSelected() {
  if (!selectedDeviceIds.value.length) return
  exporting.value = true
  loadError.value = ''
  try { await exportDevicesExcel(selectedDeviceIds.value) }
  catch (error: unknown) { loadError.value = getErrorMessage(error, 'Seçili cihazlar dışa aktarılamadı.') }
  finally { exporting.value = false }
}

async function exportAllInventory() {
  exporting.value = true
  try { await exportInventoryExcel() }
  catch (error: unknown) { loadError.value = getErrorMessage(error, 'Envanter dosyası indirilemedi.') }
  finally { exporting.value = false }
}

function openSelectedQr() {
  void router.push({ name: 'bulk-qr', query: { ids: selectedDeviceIds.value.join(',') } })
}

async function loadData(signal?: AbortSignal) {
  loading.value = true
  loadError.value = ''

  try {
    const [devicesData, categoriesData] = await Promise.all([
      getDevices(signal),
      getDeviceCategories(signal)
    ])
    devices.value = devicesData
    selectedDeviceIds.value = selectedDeviceIds.value.filter((id) => devicesData.some((device) => device.id === id))
    categories.value = categoriesData
  } catch (error: unknown) {
    if (!signal?.aborted) {
      loadError.value = getErrorMessage(error, 'Cihaz listesi yüklenemedi.')
    }
  } finally {
    if (!signal?.aborted) {
      loading.value = false
    }
  }
}

function openCreateModal() {
  void router.push({ name: 'device-create' })
}

async function openEditModal(item: DeviceListItem) {
  await router.push({ name: 'device-edit', params: { id: item.id } })
}

function closeFormModal() {
  if (!saving.value) {
    showFormModal.value = false
  }
}

async function saveDevice(payload: DevicePayload) {
  saving.value = true
  formError.value = ''

  try {
    if (editingDevice.value) {
      await updateDevice(editingDevice.value.id, payload)
      successMessage.value = 'Cihaz bilgileri güncellendi.'
    } else {
      await createDevice(payload)
      successMessage.value = 'Yeni cihaz eklendi.'
    }

    showFormModal.value = false
    await loadData()
  } catch (error: unknown) {
    formError.value = getErrorMessage(error, 'Cihaz kaydedilemedi.')
  } finally {
    saving.value = false
  }
}

async function openDetailModal(id: string) {
  await router.push({ name: 'device-detail', params: { id } })
}

async function refreshDetail() {
  const id = detailDevice.value?.id
  if (!id) return
  try {
    const [freshDevice] = await Promise.all([getDevice(id), loadData()])
    detailDevice.value = freshDevice
  } catch (error: unknown) {
    detailError.value = getErrorMessage(error, 'Cihaz detayı yenilenemedi.')
  }
}

function closeDetailModal() {
  showDetailModal.value = false
  if (route.name === 'device-qr-detail') {
    router.replace({ name: 'inventories' })
  }
}

function toggleAllFiltered() {
  const ids = filteredDevices.value.map((device) => device.id)
  selectedDeviceIds.value = allFilteredSelected.value
    ? selectedDeviceIds.value.filter((id) => !ids.includes(id))
    : [...new Set([...selectedDeviceIds.value, ...ids])]
}

function setWarrantyFilter(event: Event) {
  const value = (event.target as HTMLSelectElement).value
  router.replace({ name: route.name || 'devices', query: value ? { warranty: value } : {} })
}

function openDeleteModal(item: DeviceListItem) {
  deletingDevice.value = item
  deleteError.value = ''
  showDeleteModal.value = true
}

function closeDeleteModal() {
  if (!deleting.value) {
    showDeleteModal.value = false
    deletingDevice.value = null
  }
}

async function confirmDelete() {
  if (!deletingDevice.value) {
    return
  }

  deleting.value = true
  deleteError.value = ''

  try {
    await deleteDevice(deletingDevice.value.id)
    devices.value = devices.value.filter(
      (item) => item.id !== deletingDevice.value?.id,
    )
    successMessage.value = 'Cihaz kaydı silindi.'
    showDeleteModal.value = false
    deletingDevice.value = null
  } catch (error: unknown) {
    deleteError.value = getErrorMessage(error, 'Cihaz silinemedi.')
  } finally {
    deleting.value = false
  }
}

function getErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}

onMounted(async () => {
  applyRoutePreset()
  await loadData(listAbortController.signal)
  if (typeof route.query.category === 'string') filterCategoryId.value = route.query.category
  if (typeof route.query.q === 'string') searchTerm.value = route.query.q
})

watch(() => route.query.view, applyRoutePreset)
watch(() => route.query.category, value => { filterCategoryId.value = typeof value === 'string' ? value : '' })
watch(() => route.query.q, value => { searchTerm.value = typeof value === 'string' ? value : '' })

onBeforeUnmount(() => {
  listAbortController.abort()
})
</script>

<template>
  <section class="inventory-page" :class="`inventory-page--${routeMode}`">
    <section class="inventory-page-hero" :class="`inventory-page-hero--${pageContent.accent}`">
      <span class="inventory-page-hero__visual">
        <i :class="['bi', pageContent.icon]" aria-hidden="true"></i>
      </span>
      <div class="inventory-page-hero__main">
        <span class="inventory-page-hero__eyebrow">
          <i class="bi bi-grid-3x3-gap-fill" aria-hidden="true"></i>
          {{ pageContent.eyebrow }}
        </span>
        <h1 class="inventory-page-hero__title">{{ pageContent.title }}</h1>
        <p class="inventory-page-hero__description">{{ pageContent.description }}</p>
      </div>
      <div class="inventory-page-hero__actions">
        <button v-if="!isInventoryOverview" type="button" class="btn btn-light border" @click="returnToInventoryOverview">
          <i class="bi bi-arrow-left me-2" aria-hidden="true"></i>Envanterlere Dön
        </button>
        <button v-if="authStore.canEdit.value" type="button" class="btn btn-primary add-button" @click="openCreateModal">
          <i class="bi bi-plus-circle-fill me-2" aria-hidden="true"></i>
          Yeni Cihaz
        </button>
        <div class="inventory-page-hero__count">
          <i :class="['bi', pageContent.icon]" aria-hidden="true"></i>
          <div>
            <strong>{{ filteredDevices.length }}</strong>
            <span>{{ routeMode === 'scrap' ? 'hurda kayıt' : 'cihaz' }}</span>
          </div>
        </div>
      </div>
    </section>

    <AppAlert
      v-if="successMessage"
      variant="success"
      dismissible
      @dismiss="successMessage = ''"
    >
      {{ successMessage }}
    </AppAlert>

    <section v-if="isInventoryOverview" class="inventory-overview-panel">
      <div class="inventory-overview-panel__head">
        <div>
          <h2><i class="bi bi-grid-3x3-gap-fill" aria-hidden="true"></i> Envanter Kategorileri</h2>
          <p>Bir envanter seçin; o envantere ait cihaz listesi ve işlem araçları altta açılsın.</p>
        </div>
        <div class="inventory-overview-actions">
          <RouterLink v-if="authStore.canEdit.value" to="/envanter-yonetimi" class="btn btn-outline-primary btn-sm"><i class="bi bi-collection-fill me-1"></i>Envanter Ekle</RouterLink>
          <RouterLink v-if="authStore.canEdit.value" to="/cihazlar/yeni" class="btn btn-primary btn-sm"><i class="bi bi-plus-circle-fill me-1"></i>Cihaz Ekle</RouterLink>
          <button type="button" class="btn btn-success btn-sm" :disabled="exporting" @click="exportAllInventory"><i class="bi bi-download me-1"></i>Envanter Excel — Tümü</button>
          <RouterLink to="/excel-islemleri/import" class="btn btn-outline-primary btn-sm"><i class="bi bi-upload me-1"></i>Import Merkezi</RouterLink>
        </div>
      </div>
      <div class="inventory-overview-meta"><span class="inventory-overview-panel__total">{{ devices.length }} toplam varlık</span><span>İkonlara tıklayarak envanteri açın</span></div>
      <div class="inventory-category-grid">
        <button
          v-for="category in categorySummaries"
          :key="category.id"
          type="button"
          class="inventory-category-item"
          :class="[`is-${category.tone}`, { 'is-active': filterCategoryId === category.id }]"
          @click="chooseCategory(category.id)"
        >
          <span class="inventory-category-item__icon"><i :class="['bi', category.icon]" aria-hidden="true"></i></span>
          <span class="inventory-category-item__label">{{ category.name }}</span>
          <strong class="inventory-category-item__count">{{ category.count }}</strong>
        </button>
        <button type="button" class="inventory-category-item is-rose inventory-category-item--scrap" @click="chooseScrapInventory">
          <span class="inventory-category-item__icon"><i class="bi bi-archive-fill" aria-hidden="true"></i></span>
          <span class="inventory-category-item__label">Hurda Envanteri</span>
          <strong class="inventory-category-item__count">{{ scrapCount }}</strong>
        </button>
      </div>
    </section>

    <div v-if="!isInventoryOverview && routeMode !== 'scrap'" class="inventory-kpi-grid">
      <article class="inventory-kpi inventory-kpi--primary">
        <span class="inventory-kpi__icon"><i class="bi bi-hdd-stack-fill"></i></span>
        <strong>{{ devices.length }}</strong><span>Toplam Varlık</span>
      </article>
      <article class="inventory-kpi inventory-kpi--success">
        <span class="inventory-kpi__icon"><i class="bi bi-box-seam-fill"></i></span>
        <strong>{{ stockCount }}</strong><span>IT Depo / Stok</span>
      </article>
      <article class="inventory-kpi inventory-kpi--info">
        <span class="inventory-kpi__icon"><i class="bi bi-person-check-fill"></i></span>
        <strong>{{ assignedCount }}</strong><span>Zimmetli Cihaz</span>
      </article>
      <article class="inventory-kpi inventory-kpi--danger">
        <span class="inventory-kpi__icon"><i class="bi bi-archive-fill"></i></span>
        <strong>{{ scrapCount }}</strong><span>Hurda / İmha</span>
      </article>
    </div>

    <section v-if="!isInventoryOverview" class="inventory-filter-panel">
      <div class="inventory-filter-panel__search">
        <label for="device-page-search">Tabloda Ara</label>
        <div class="inventory-search-input">
          <i class="bi bi-search" aria-hidden="true"></i>
          <input
            id="device-page-search"
            v-model="searchTerm"
            type="search"
            class="form-control"
            placeholder="Cihaz, marka, model, seri veya envanter no ara..."
            autocomplete="off"
          />
        </div>
      </div>
      <div class="inventory-filter-panel__filters">
        <label>
          <span>Kategori</span>
          <select v-model="filterCategoryId" class="form-select" aria-label="Kategori filtresi" :disabled="Boolean(selectedCategory)">
            <option value="">Tüm Kategoriler</option>
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
          </select>
        </label>
        <label>
          <span>Durum</span>
          <select v-model="filterStatus" class="form-select" aria-label="Durum filtresi">
            <option value="">Tüm Durumlar</option>
            <option value="1">Stokta</option><option value="2">Zimmetli</option><option value="3">Serviste</option>
            <option value="4">Hurda</option><option value="5">Kayıp</option><option value="6">Pasif</option>
          </select>
        </label>
        <label>
          <span>Garanti</span>
          <select :value="warrantyFilter" class="form-select" aria-label="Garanti filtresi" @change="setWarrantyFilter">
            <option value="">Tüm Garantiler</option><option value="expiring">Yakında Bitecek</option>
            <option value="expired">Süresi Dolmuş</option><option value="missing">Bilgi Yok</option>
          </select>
        </label>
        <button v-if="hasActiveFilters" type="button" class="btn btn-filter-clear" @click="clearFilters">
          <i class="bi bi-x-lg me-1"></i>Temizle
        </button>
      </div>
    </section>

    <div v-if="!isInventoryOverview && selectedDevices.length" class="inventory-selection-bar">
      <i class="bi bi-check2-square" aria-hidden="true"></i>
      <span><strong>{{ selectedDevices.length }}</strong> cihaz seçildi</span>
      <button type="button" class="btn btn-sm btn-light" @click="showBulkLabels = true">
        <i class="bi bi-tags-fill me-1"></i>Toplu Etiket
      </button>
      <button type="button" class="btn btn-sm btn-light" @click="openSelectedQr"><i class="bi bi-qr-code me-1"></i>Toplu QR</button>
      <button type="button" class="btn btn-sm btn-light" :disabled="exporting" @click="exportSelected"><i class="bi bi-file-earmark-excel me-1"></i>Excel</button>
      <button type="button" class="inventory-selection-bar__clear" @click="selectedDeviceIds = []">Seçimi Temizle</button>
    </div>

    <div v-if="!isInventoryOverview" class="card content-card list-card inventory-table-card">
      <div class="table-card-header">
        <div>
          <h3 class="table-card-header__title"><i class="bi bi-table me-2" aria-hidden="true"></i>{{ listTitle }} Listesi</h3>
          <p class="table-card-header__meta mb-0"><span class="result-count">{{ filteredDevices.length }} kayıt</span> gösteriliyor</p>
        </div>
        <div class="inventory-list-actions">
          <button v-if="authStore.canEdit.value" type="button" class="btn btn-primary btn-sm" @click="openCreateModal">
            <i class="bi bi-plus-circle-fill me-1"></i>Cihaz Ekle
          </button>
          <button v-if="selectedCategory" type="button" class="btn btn-success btn-sm" :disabled="exporting" @click="exportCurrentCategory"><i class="bi bi-file-earmark-excel-fill me-1"></i>{{ selectedCategory.name }} Excel İndir</button>
          <RouterLink v-if="selectedCategory && authStore.canEdit.value" :to="{ name: 'excel-import', query: { target: 'devices', category: selectedCategory.id } }" class="btn btn-outline-primary btn-sm"><i class="bi bi-upload me-1"></i>{{ selectedCategory.name }} Excel Import</RouterLink>
          <RouterLink v-if="selectedCategory && authStore.canEdit.value" :to="{ name: 'inventory-table-editor', params: { categoryId: selectedCategory.id } }" target="_blank" class="btn btn-warning btn-sm"><i class="bi bi-pencil-square me-1"></i>Tabloyu Düzenle</RouterLink>
          <button v-if="authStore.canEdit.value" type="button" class="btn btn-outline-primary btn-sm" @click="showRangeModal = true"><i class="bi bi-ui-checks-grid me-1"></i>Satır Aralığı Seç</button>
          <button v-if="selectedCategory && authStore.canEdit.value" type="button" class="btn btn-info btn-sm text-white" @click="openCategoryQr"><i class="bi bi-qr-code me-1"></i>Kategorinin Tüm QR’ları</button>
          <button type="button" class="btn btn-success btn-sm" :disabled="exporting" @click="exportAllInventory"><i class="bi bi-download me-1"></i>Envanter Excel — Tümü</button>
        </div>
      </div>

      <DataState v-if="loading" type="loading" title="Cihazlar yükleniyor" message="API bağlantısı kontrol ediliyor..." />

      <DataState
        v-else-if="loadError"
        type="error"
        title="Cihaz listesi alınamadı"
        :message="loadError"
      >
        <template #actions>
          <button type="button" class="btn btn-outline-primary" @click="loadData()">
            Yeniden Dene
          </button>
        </template>
      </DataState>

      <DataState
        v-else-if="devices.length === 0"
        type="empty"
        title="Henüz cihaz kaydı yok"
        message="İlk cihaz kaydını ekleyerek başlayın."
        icon="bi-laptop"
      >
        <template #actions>
            <button v-if="authStore.canEdit.value" type="button" class="btn btn-primary" @click="openCreateModal">
            Yeni Cihaz Ekle
          </button>
        </template>
      </DataState>

      <DataState
        v-else-if="filteredDevices.length === 0"
        type="no-results"
        title="Eşleşen cihaz bulunamadı"
        message="Filtreleri değiştirip yeniden deneyin."
      >
        <template #actions>
          <button type="button" class="btn btn-light border" @click="clearFilters">
            Filtreleri Temizle
          </button>
        </template>
      </DataState>

      <div v-else class="table-responsive inventory-table-wrap">
        <table class="table data-table inventory-data-table align-middle mb-0">
          <thead>
            <tr>
              <th class="selection-column"><input type="checkbox" :checked="allFilteredSelected" aria-label="Tümünü seç" @change="toggleAllFiltered"></th>
              <th class="col-device">Cihaz</th>
              <th class="col-brand">Marka / Model</th>
              <th class="col-serial">Kimlik Bilgileri</th>
              <th class="col-person">Kullanıcı</th>
              <th class="col-status">Durum</th>
              <th class="col-warranty">Garanti</th>
              <th class="col-actions text-end">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredDevices" :key="item.id" :class="{ 'is-selected': selectedDeviceIds.includes(item.id) }" @dblclick="openDetailModal(item.id)">
              <td class="selection-column"><input v-model="selectedDeviceIds" type="checkbox" :value="item.id" :aria-label="`${item.cihazAdi} seç`"></td>
              <td class="col-device">
                <div class="inventory-device-cell">
                  <span class="inventory-device-cell__icon"><i class="bi bi-pc-display-horizontal" aria-hidden="true"></i></span>
                  <div>
                    <div class="cell-primary cell-truncate" :title="item.cihazAdi">{{ item.cihazAdi }}</div>
                    <span class="inventory-category-badge" :title="item.categoryName">{{ item.categoryName }}</span>
                  </div>
                </div>
              </td>
              <td class="col-brand">
                <div class="cell-truncate" :title="item.marka">{{ item.marka }}</div>
                <span class="cell-secondary cell-truncate" :title="item.model">{{ item.model }}</span>
              </td>
              <td class="col-serial">
                <code class="inventory-serial" :title="item.seriNo">{{ item.seriNo }}</code>
                <span class="inventory-number" :title="item.envanterNo"><i class="bi bi-upc-scan"></i>{{ item.envanterNo }}</span>
              </td>
              <td class="col-person">
                <div v-if="item.personelName" class="inventory-user-chip" :title="item.personelName">
                  <span>{{ item.personelName.charAt(0).toLocaleUpperCase('tr-TR') }}</span>
                  <strong class="cell-truncate">{{ item.personelName }}</strong>
                </div>
                <span v-else class="inventory-unassigned"><i class="bi bi-box-seam"></i>IT Depo</span>
              </td>
              <td class="col-status">
                <span class="badge rounded-pill" :class="`text-bg-${getStatusColor(item.status)}`">
                  {{ item.statusName }}
                </span>
              </td>
              <td class="col-warranty"><span class="badge rounded-pill" :class="getWarrantyBadgeClass(item.warrantyStatus)">{{ item.warrantyStatusName }}</span><div v-if="item.warrantyEndDate" class="small text-secondary mt-1">{{ item.warrantyEndDate }}</div></td>
              <td class="col-actions text-end">
                <div class="action-buttons d-inline-flex gap-1">
                  <button
                    type="button"
                    class="btn btn-sm action-button"
                    title="Detayı görüntüle"
                    aria-label="Detayı görüntüle"
                    @click="openDetailModal(item.id)"
                  >
                    <i class="bi bi-eye" aria-hidden="true"></i>
                  </button>
                  <RouterLink :to="{ name: 'device-history', params: { id: item.id } }" class="btn btn-sm action-button" title="Zimmet geçmişi" :aria-label="`${item.cihazAdi} zimmet geçmişini aç`"><i class="bi bi-clock-history" aria-hidden="true"></i></RouterLink>
                  <button
                    v-if="authStore.canEdit.value"
                    type="button"
                    class="btn btn-sm action-button"
                    title="Düzenle"
                    aria-label="Cihazı düzenle"
                    @click="openEditModal(item)"
                  >
                    <i class="bi bi-pencil" aria-hidden="true"></i>
                  </button>
                  <button
                    v-if="authStore.isAdmin.value"
                    type="button"
                    class="btn btn-sm action-button danger"
                    title="Sil"
                    aria-label="Cihazı sil"
                    @click="openDeleteModal(item)"
                  >
                    <i class="bi bi-trash3" aria-hidden="true"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="showRangeModal" class="app-modal-backdrop" role="presentation" @click.self="showRangeModal = false">
      <section class="app-modal app-modal--compact" role="dialog" aria-modal="true" aria-labelledby="range-title">
        <header class="app-modal__header"><div><span class="section-kicker">HIZLI SEÇİM</span><h2 id="range-title">Satır Aralığı Seç</h2></div><button type="button" class="btn-close" aria-label="Kapat" @click="showRangeModal = false"></button></header>
        <div class="app-modal__body"><p class="text-secondary">Ekrandaki liste sırasına göre başlangıç ve bitiş numarasını girin.</p><div class="range-input-grid"><label><span>Başlangıç</span><input v-model.number="rangeStart" type="number" min="1" class="form-control"></label><i class="bi bi-arrow-right"></i><label><span>Bitiş</span><input v-model.number="rangeEnd" type="number" min="1" :max="filteredDevices.length" class="form-control"></label></div><div class="app-alert info mt-3">{{ rangeStart }}–{{ Math.min(rangeEnd, filteredDevices.length) }} arasındaki görünen satırlar seçilecek.</div></div>
        <footer class="app-modal__footer"><button type="button" class="btn btn-light border" @click="showRangeModal = false">Vazgeç</button><button type="button" class="btn btn-primary" @click="selectRange"><i class="bi bi-check2-square me-1"></i>Aralığı Seç</button></footer>
      </section>
    </div>

    <DeviceFormModal
      :show="showFormModal"
      :device="editingDevice"
      :categories="categories"
      :saving="saving"
      :error-message="formError"
      @close="closeFormModal"
      @submit="saveDevice"
    />

    <DeviceDetailModal
      :show="showDetailModal"
      :device="detailDevice"
      :loading="detailLoading"
      :error-message="detailError"
      @close="closeDetailModal"
      @refresh="refreshDetail"
    />

    <DeviceLabelModal
      :show="showBulkLabels"
      :devices="selectedDevices"
      @close="showBulkLabels = false"
    />

    <ConfirmationModal
      :show="showDeleteModal"
      title="Cihaz silinsin mi?"
      message="Kayıt listeden kaldırılacak ancak veritabanında korunacaktır."
      :detail="
        deletingDevice
          ? `${deletingDevice.cihazAdi} · ${deletingDevice.seriNo}`
          : ''
      "
      :confirming="deleting"
      :error-message="deleteError"
      confirm-label="Cihazı Sil"
      @cancel="closeDeleteModal"
      @confirm="confirmDelete"
    />
  </section>
</template>
