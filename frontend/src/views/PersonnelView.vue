<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import ConfirmationModal from '../components/ConfirmationModal.vue'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import PersonnelDetailModal from '../components/PersonnelDetailModal.vue'
import PersonnelFormModal from '../components/PersonnelFormModal.vue'
import { authStore } from '../stores/authStore'
import { exportPersonnelExcel } from '../services/excelService'
import {
  createPersonnel,
  deletePersonnel,
  getPersonnelList,
  updatePersonnel,
  type PersonnelDetail,
  type PersonnelListItem,
  type PersonnelPayload,
} from '../services/personnelService'

const personnel = ref<PersonnelListItem[]>([])
const router = useRouter()
const loading = ref(true)
const loadError = ref('')
const searchTerm = ref('')
const departmentFilter = ref('')
const statusFilter = ref('all')
const successMessage = ref('')
const selectedPersonnelIds = ref<string[]>([])
const exporting = ref(false)

const showFormModal = ref(false)
const editingPersonnel = ref<PersonnelListItem | null>(null)
const saving = ref(false)
const formError = ref('')

const showDetailModal = ref(false)
const detailPersonnel = ref<PersonnelDetail | null>(null)
const detailLoading = ref(false)
const detailError = ref('')

const showDeleteModal = ref(false)
const deletingPersonnel = ref<PersonnelListItem | null>(null)
const deleting = ref(false)
const deleteError = ref('')

const listAbortController = new AbortController()

const filteredPersonnel = computed(() => {
  const query = searchTerm.value.toLocaleLowerCase('tr-TR').trim()

  return personnel.value.filter((item) => {
    const matchesQuery = !query || [
      item.sicilNo,
      item.ad,
      item.soyad,
      item.departman,
      item.pozisyon,
      item.zimmetNo ?? '',
      item.aktifMi ? 'aktif' : 'pasif',
    ]
      .join(' ')
      .toLocaleLowerCase('tr-TR')
      .includes(query)
    const matchesDepartment = !departmentFilter.value || item.departman === departmentFilter.value
    const matchesStatus = statusFilter.value === 'all' || (statusFilter.value === 'active' ? item.aktifMi : !item.aktifMi)
    return matchesQuery && matchesDepartment && matchesStatus
  })
})

const activePersonnelCount = computed(
  () => personnel.value.filter((item) => item.aktifMi).length,
)
const departments = computed(() => [...new Set(personnel.value.map((item) => item.departman))].sort((a, b) => a.localeCompare(b, 'tr-TR')))
const departmentCount = computed(() => departments.value.length)
const passivePersonnelCount = computed(() => personnel.value.length - activePersonnelCount.value)
const selectedPersonnel = computed(() => personnel.value.filter(item => selectedPersonnelIds.value.includes(item.id)))
const allFilteredSelected = computed(() => filteredPersonnel.value.length > 0 && filteredPersonnel.value.every(item => selectedPersonnelIds.value.includes(item.id)))

function clearFilters() {
  searchTerm.value = ''
  departmentFilter.value = ''
  statusFilter.value = 'all'
}

function toggleAllFiltered() {
  const ids = filteredPersonnel.value.map(item => item.id)
  selectedPersonnelIds.value = allFilteredSelected.value
    ? selectedPersonnelIds.value.filter(id => !ids.includes(id))
    : [...new Set([...selectedPersonnelIds.value, ...ids])]
}

async function exportSelectedPersonnel() {
  if (!selectedPersonnelIds.value.length) return
  exporting.value = true
  loadError.value = ''
  try { await exportPersonnelExcel(selectedPersonnelIds.value) }
  catch (error: unknown) { loadError.value = getErrorMessage(error, 'Seçili personeller dışa aktarılamadı.') }
  finally { exporting.value = false }
}

async function loadPersonnel(signal?: AbortSignal) {
  loading.value = true
  loadError.value = ''

  try {
    personnel.value = await getPersonnelList(signal)
  } catch (error: unknown) {
    if (!signal?.aborted) {
      loadError.value = getErrorMessage(error, 'Personel listesi yüklenemedi.')
    }
  } finally {
    if (!signal?.aborted) {
      loading.value = false
    }
  }
}

function openCreateModal() {
  void router.push({ name: 'personnel-create' })
}

function openEditModal(item: PersonnelListItem) {
  void router.push({ name: 'personnel-edit', params: { id: item.id } })
}

function closeFormModal() {
  if (!saving.value) {
    showFormModal.value = false
  }
}

async function savePersonnel(payload: PersonnelPayload) {
  saving.value = true
  formError.value = ''

  try {
    if (editingPersonnel.value) {
      await updatePersonnel(editingPersonnel.value.id, payload)
      successMessage.value = 'Personel bilgileri güncellendi.'
    } else {
      await createPersonnel(payload)
      successMessage.value = 'Yeni personel eklendi.'
    }

    showFormModal.value = false
    await loadPersonnel()
  } catch (error: unknown) {
    formError.value = getErrorMessage(error, 'Personel kaydedilemedi.')
  } finally {
    saving.value = false
  }
}

async function openDetailModal(id: string) {
  await router.push({ name: 'personnel-detail', params: { id } })
}

function closeDetailModal() {
  showDetailModal.value = false
}

function openDeleteModal(item: PersonnelListItem) {
  deletingPersonnel.value = item
  deleteError.value = ''
  showDeleteModal.value = true
}

function closeDeleteModal() {
  if (!deleting.value) {
    showDeleteModal.value = false
    deletingPersonnel.value = null
  }
}

async function confirmDelete() {
  if (!deletingPersonnel.value) {
    return
  }

  deleting.value = true
  deleteError.value = ''

  try {
    await deletePersonnel(deletingPersonnel.value.id)
    personnel.value = personnel.value.filter(
      (item) => item.id !== deletingPersonnel.value?.id,
    )
    successMessage.value = 'Personel kaydı silindi.'
    showDeleteModal.value = false
    deletingPersonnel.value = null
  } catch (error: unknown) {
    deleteError.value = getErrorMessage(error, 'Personel silinemedi.')
  } finally {
    deleting.value = false
  }
}

function getErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}

onMounted(() => loadPersonnel(listAbortController.signal))

onBeforeUnmount(() => {
  listAbortController.abort()
})
</script>

<template>
  <section>
    <header class="people-hero">
      <div class="people-hero__icon"><i class="bi bi-people"></i></div>
      <div class="people-hero__content">
        <span class="people-hero__eyebrow">ORGANİZASYON YÖNETİMİ</span>
        <h1>Personeller</h1>
        <p>Çalışan kayıtlarını, departmanları ve zimmet durumlarını merkezi olarak yönetin.</p>
      </div>
      <div class="people-hero__actions">
        <div class="people-hero__count"><strong>{{ personnel.length }}</strong><span>Toplam kayıt</span></div>
        <button v-if="authStore.canEdit.value" type="button" class="btn btn-primary add-button" @click="openCreateModal">
          <i class="bi bi-person-plus me-2" aria-hidden="true"></i>
          Yeni Personel
        </button>
      </div>
    </header>

    <AppAlert
      v-if="successMessage"
      variant="success"
      dismissible
      @dismiss="successMessage = ''"
    >
      {{ successMessage }}
    </AppAlert>

    <div class="people-kpi-grid">
      <div class="people-kpi"><span><i class="bi bi-people"></i></span><div><strong>{{ personnel.length }}</strong><small>Toplam Personel</small></div></div>
      <div class="people-kpi is-success"><span><i class="bi bi-person-check"></i></span><div><strong>{{ activePersonnelCount }}</strong><small>Aktif Personel</small></div></div>
      <div class="people-kpi is-violet"><span><i class="bi bi-diagram-3"></i></span><div><strong>{{ departmentCount }}</strong><small>Departman</small></div></div>
      <div class="people-kpi is-muted"><span><i class="bi bi-person-dash"></i></span><div><strong>{{ passivePersonnelCount }}</strong><small>Pasif Personel</small></div></div>
    </div>

    <div v-if="selectedPersonnel.length" class="inventory-selection-bar">
      <i class="bi bi-check2-square"></i><span><strong>{{ selectedPersonnel.length }}</strong> personel seçildi</span>
      <button type="button" class="btn btn-sm btn-light" :disabled="exporting" @click="exportSelectedPersonnel"><i class="bi bi-file-earmark-excel me-1"></i>Seçilenleri Excel’e Aktar</button>
      <button type="button" class="inventory-selection-bar__clear" @click="selectedPersonnelIds = []">Seçimi Temizle</button>
    </div>

    <div class="card content-card list-card">
      <div class="table-card-header">
        <div>
          <h3 class="table-card-header__title">Personel Listesi</h3>
          <p class="table-card-header__meta mb-0">{{ filteredPersonnel.length }} kayıt gösteriliyor</p>
        </div>
        <span class="people-list-count"><i class="bi bi-funnel"></i> {{ filteredPersonnel.length }} sonuç</span>
      </div>

      <div class="people-filter-bar">
        <label class="search-box people-filter-bar__search"><i class="bi bi-search" aria-hidden="true"></i><span class="visually-hidden">Personel ara</span><input v-model="searchTerm" type="search" class="form-control" placeholder="Sicil, ad, departman veya pozisyon ara..." autocomplete="off" /></label>
        <label><span class="visually-hidden">Departman</span><select v-model="departmentFilter" class="form-select"><option value="">Tüm departmanlar</option><option v-for="department in departments" :key="department" :value="department">{{ department }}</option></select></label>
        <label><span class="visually-hidden">Durum</span><select v-model="statusFilter" class="form-select"><option value="all">Tüm durumlar</option><option value="active">Aktif</option><option value="passive">Pasif</option></select></label>
        <button v-if="searchTerm || departmentFilter || statusFilter !== 'all'" type="button" class="btn btn-light border" @click="clearFilters"><i class="bi bi-x-lg me-1"></i>Temizle</button>
      </div>

      <DataState v-if="loading" type="loading" title="Personeller yükleniyor" message="API bağlantısı kontrol ediliyor..." />

      <DataState
        v-else-if="loadError"
        type="error"
        title="Personel listesi alınamadı"
        :message="loadError"
      >
        <template #actions>
          <button type="button" class="btn btn-outline-primary" @click="loadPersonnel()">
            Yeniden Dene
          </button>
        </template>
      </DataState>

      <DataState
        v-else-if="personnel.length === 0"
        type="empty"
        title="Henüz personel kaydı yok"
        message="İlk personel kaydını ekleyerek başlayın."
        icon="bi-person-plus"
      >
        <template #actions>
          <button v-if="authStore.canEdit.value" type="button" class="btn btn-primary" @click="openCreateModal">
            Yeni Personel Ekle
          </button>
        </template>
      </DataState>

      <DataState
        v-else-if="filteredPersonnel.length === 0"
        type="no-results"
        title="Eşleşen personel bulunamadı"
        message="Arama ifadenizi değiştirip yeniden deneyin."
      >
        <template #actions>
          <button type="button" class="btn btn-light border" @click="clearFilters">
            Aramayı Temizle
          </button>
        </template>
      </DataState>

      <div v-else class="table-responsive people-table-wrap">
        <table class="table data-table people-data-table align-middle mb-0">
          <thead>
            <tr>
              <th class="selection-column"><input type="checkbox" :checked="allFilteredSelected" aria-label="Görünen personellerin tümünü seç" @change="toggleAllFiltered"></th>
              <th class="col-sicil">Sicil No</th>
              <th class="col-name">Ad Soyad</th>
              <th class="col-dept">Departman</th>
              <th class="col-position">Pozisyon</th>
              <th class="col-sicil">Zimmet No</th>
              <th class="col-status">Durum</th>
              <th class="col-actions text-end">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredPersonnel" :key="item.id">
              <td class="selection-column"><input v-model="selectedPersonnelIds" type="checkbox" :value="item.id" :aria-label="`${item.ad} ${item.soyad} seç`"></td>
              <td>
                <span class="registration-number">{{ item.sicilNo }}</span>
              </td>
              <td>
                <div class="person-cell d-flex align-items-center gap-3">
                  <span
                    class="person-avatar d-inline-flex align-items-center justify-content-center"
                  >
                    {{ item.ad.charAt(0) }}{{ item.soyad.charAt(0) }}
                  </span>
                  <div class="min-w-0">
                    <div class="cell-primary cell-truncate" :title="`${item.ad} ${item.soyad}`">{{ item.ad }} {{ item.soyad }}</div>
                    <span class="cell-secondary cell-truncate" :title="item.pozisyon">{{ item.pozisyon }}</span>
                  </div>
                </div>
              </td>
              <td class="cell-truncate col-dept" :title="item.departman">{{ item.departman }}</td>
              <td class="cell-truncate col-position" :title="item.pozisyon">{{ item.pozisyon }}</td>
              <td>
                <span v-if="item.zimmetNo" class="debit-number">
                  {{ item.zimmetNo }}
                </span>
                <span v-else class="text-secondary">—</span>
              </td>
              <td>
                <span
                  class="status-badge"
                  :class="item.aktifMi ? 'active' : 'passive'"
                >
                  <span class="status-dot"></span>
                  {{ item.aktifMi ? 'Aktif' : 'Pasif' }}
                </span>
              </td>
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
                  <RouterLink :to="{ name: 'personnel-documents', params: { id: item.id } }" class="btn btn-sm action-button" title="Zimmet belgesi ve imzalı belgeler" :aria-label="`${item.ad} ${item.soyad} belgelerini aç`"><i class="bi bi-file-earmark-text" aria-hidden="true"></i></RouterLink>
                  <button
                    v-if="authStore.canEdit.value"
                    type="button"
                    class="btn btn-sm action-button"
                    title="Düzenle"
                    aria-label="Personeli düzenle"
                    @click="openEditModal(item)"
                  >
                    <i class="bi bi-pencil" aria-hidden="true"></i>
                  </button>
                  <button
                    v-if="authStore.isAdmin.value"
                    type="button"
                    class="btn btn-sm action-button danger"
                    title="Sil"
                    aria-label="Personeli sil"
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

    <PersonnelFormModal
      :show="showFormModal"
      :personnel="editingPersonnel"
      :saving="saving"
      :error-message="formError"
      @close="closeFormModal"
      @submit="savePersonnel"
    />

    <PersonnelDetailModal
      :show="showDetailModal"
      :personnel="detailPersonnel"
      :loading="detailLoading"
      :error-message="detailError"
      @close="closeDetailModal"
    />

    <ConfirmationModal
      :show="showDeleteModal"
      title="Personel silinsin mi?"
      message="Kayıt listeden kaldırılacak ancak veritabanında korunacaktır."
      :detail="
        deletingPersonnel
          ? `${deletingPersonnel.ad} ${deletingPersonnel.soyad} · ${deletingPersonnel.sicilNo}`
          : ''
      "
      :confirming="deleting"
      :error-message="deleteError"
      confirm-label="Personeli Sil"
      @cancel="closeDeleteModal"
      @confirm="confirmDelete"
    />
  </section>
</template>
