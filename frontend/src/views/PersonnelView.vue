<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import ConfirmationModal from '../components/ConfirmationModal.vue'
import PersonnelDetailModal from '../components/PersonnelDetailModal.vue'
import PersonnelFormModal from '../components/PersonnelFormModal.vue'
import {
  createPersonnel,
  deletePersonnel,
  getPersonnelById,
  getPersonnelList,
  updatePersonnel,
  type PersonnelDetail,
  type PersonnelListItem,
  type PersonnelPayload,
} from '../services/personnelService'

const personnel = ref<PersonnelListItem[]>([])
const loading = ref(true)
const loadError = ref('')
const searchTerm = ref('')
const successMessage = ref('')

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
let detailAbortController: AbortController | null = null

const filteredPersonnel = computed(() => {
  const query = searchTerm.value.toLocaleLowerCase('tr-TR').trim()

  if (!query) {
    return personnel.value
  }

  return personnel.value.filter((item) =>
    [
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
      .includes(query),
  )
})

const activePersonnelCount = computed(
  () => personnel.value.filter((item) => item.aktifMi).length,
)

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
  editingPersonnel.value = null
  formError.value = ''
  showFormModal.value = true
}

function openEditModal(item: PersonnelListItem) {
  editingPersonnel.value = item
  formError.value = ''
  showFormModal.value = true
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
  detailAbortController?.abort()
  const controller = new AbortController()
  detailAbortController = controller
  detailPersonnel.value = null
  detailError.value = ''
  detailLoading.value = true
  showDetailModal.value = true

  try {
    detailPersonnel.value = await getPersonnelById(
      id,
      controller.signal,
    )
  } catch (error: unknown) {
    if (!controller.signal.aborted) {
      detailError.value = getErrorMessage(error, 'Personel detayı yüklenemedi.')
    }
  } finally {
    if (!controller.signal.aborted) {
      detailLoading.value = false
    }
  }
}

function closeDetailModal() {
  detailAbortController?.abort()
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
  detailAbortController?.abort()
})
</script>

<template>
  <section>
    <div
      class="page-heading d-flex flex-column flex-lg-row align-items-lg-center justify-content-between gap-3 mb-4"
    >
      <div>
        <h2 class="h4 fw-semibold mb-1">Personel Yönetimi</h2>
        <p class="text-secondary mb-0">
          Personel kayıtlarını, görev bilgilerini ve zimmet numaralarını yönetin.
        </p>
      </div>
      <button type="button" class="btn btn-primary add-button" @click="openCreateModal">
        <i class="bi bi-person-plus me-2" aria-hidden="true"></i>
        Yeni Personel
      </button>
    </div>

    <div
      v-if="successMessage"
      class="alert alert-success alert-dismissible fade show"
      role="status"
    >
      {{ successMessage }}
      <button
        type="button"
        class="btn-close"
        aria-label="Kapat"
        @click="successMessage = ''"
      ></button>
    </div>

    <div class="row g-3 mb-4">
      <div class="col-12 col-md-6">
        <div class="summary-card d-flex align-items-center gap-3">
          <span class="summary-icon d-inline-flex align-items-center justify-content-center">
            <i class="bi bi-people" aria-hidden="true"></i>
          </span>
          <div>
            <div class="summary-label">Toplam Personel</div>
            <div class="summary-value">{{ personnel.length }}</div>
          </div>
        </div>
      </div>
      <div class="col-12 col-md-6">
        <div class="summary-card d-flex align-items-center gap-3">
          <span
            class="summary-icon active d-inline-flex align-items-center justify-content-center"
          >
            <i class="bi bi-person-check" aria-hidden="true"></i>
          </span>
          <div>
            <div class="summary-label">Aktif Personel</div>
            <div class="summary-value">{{ activePersonnelCount }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="card content-card personnel-card">
      <div
        class="card-header table-toolbar d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-3"
      >
        <div>
          <h3 class="h6 fw-semibold mb-1">Personel Listesi</h3>
          <p class="small text-secondary mb-0">
            {{ filteredPersonnel.length }} kayıt gösteriliyor
          </p>
        </div>
        <label class="search-box">
          <i class="bi bi-search" aria-hidden="true"></i>
          <span class="visually-hidden">Personel ara</span>
          <input
            v-model="searchTerm"
            type="search"
            class="form-control"
            placeholder="Sicil, ad, departman veya pozisyon ara..."
            autocomplete="off"
          />
        </label>
      </div>

      <div v-if="loading" class="state-panel">
        <div class="spinner-border text-primary mb-3" role="status">
          <span class="visually-hidden">Yükleniyor</span>
        </div>
        <strong>Personeller yükleniyor</strong>
        <span>API bağlantısı kontrol ediliyor...</span>
      </div>

      <div v-else-if="loadError" class="state-panel">
        <span class="state-icon error">
          <i class="bi bi-exclamation-triangle" aria-hidden="true"></i>
        </span>
        <strong>Personel listesi alınamadı</strong>
        <span>{{ loadError }}</span>
        <button type="button" class="btn btn-outline-primary mt-2" @click="loadPersonnel()">
          Yeniden Dene
        </button>
      </div>

      <div v-else-if="personnel.length === 0" class="state-panel">
        <span class="state-icon">
          <i class="bi bi-person-plus" aria-hidden="true"></i>
        </span>
        <strong>Henüz personel kaydı yok</strong>
        <span>İlk personel kaydını ekleyerek başlayın.</span>
        <button type="button" class="btn btn-primary mt-2" @click="openCreateModal">
          Yeni Personel Ekle
        </button>
      </div>

      <div v-else-if="filteredPersonnel.length === 0" class="state-panel">
        <span class="state-icon">
          <i class="bi bi-search" aria-hidden="true"></i>
        </span>
        <strong>Eşleşen personel bulunamadı</strong>
        <span>Arama ifadenizi değiştirip yeniden deneyin.</span>
        <button
          type="button"
          class="btn btn-light border mt-2"
          @click="searchTerm = ''"
        >
          Aramayı Temizle
        </button>
      </div>

      <div v-else class="table-responsive">
        <table class="table personnel-table align-middle mb-0">
          <thead>
            <tr>
              <th>Sicil No</th>
              <th>Ad Soyad</th>
              <th>Departman</th>
              <th>Pozisyon</th>
              <th>Zimmet No</th>
              <th>Durum</th>
              <th class="text-end">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredPersonnel" :key="item.id">
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
                  <div>
                    <div class="fw-semibold">{{ item.ad }} {{ item.soyad }}</div>
                    <small class="text-secondary">{{ item.pozisyon }}</small>
                  </div>
                </div>
              </td>
              <td>{{ item.departman }}</td>
              <td>{{ item.pozisyon }}</td>
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
              <td class="text-end">
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
                  <button
                    type="button"
                    class="btn btn-sm action-button"
                    title="Düzenle"
                    aria-label="Personeli düzenle"
                    @click="openEditModal(item)"
                  >
                    <i class="bi bi-pencil" aria-hidden="true"></i>
                  </button>
                  <button
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
      @cancel="closeDeleteModal"
      @confirm="confirmDelete"
    />
  </section>
</template>

<style scoped>
.add-button {
  min-height: 44px;
  padding-inline: 1.15rem;
  box-shadow: 0 0.4rem 1rem rgb(35 100 210 / 16%);
}

.summary-card {
  min-height: 92px;
  padding: 1.1rem 1.25rem;
  border: 1px solid var(--border-color);
  border-radius: 0.875rem;
  background: #fff;
  box-shadow: 0 0.25rem 1.25rem rgb(17 38 63 / 4%);
}

.summary-icon {
  width: 48px;
  height: 48px;
  border-radius: 0.85rem;
  color: #2364d2;
  background: #e9f0fc;
}

.summary-icon.active {
  color: #16844b;
  background: #e5f6ed;
}

.summary-icon i {
  font-size: 1.2rem;
}

.summary-label {
  color: #78879a;
  font-size: 0.78rem;
  font-weight: 600;
  text-transform: uppercase;
}

.summary-value {
  color: #202c3b;
  font-size: 1.45rem;
  font-weight: 700;
}

.personnel-card {
  overflow: hidden;
}

.table-toolbar {
  min-height: 82px;
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--border-color);
  background: #fff;
}

.search-box {
  position: relative;
  width: min(100%, 360px);
}

.search-box i {
  position: absolute;
  z-index: 2;
  top: 50%;
  left: 0.9rem;
  color: #78879a;
  transform: translateY(-50%);
}

.search-box .form-control {
  min-height: 42px;
  padding-left: 2.55rem;
  border-color: #dce4ee;
  border-radius: 0.7rem;
}

.state-panel {
  min-height: 350px;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: #78879a;
}

.state-panel strong {
  margin-bottom: 0.35rem;
  color: #263548;
  font-size: 1rem;
}

.state-icon {
  width: 56px;
  height: 56px;
  margin-bottom: 1rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 1rem;
  color: #2364d2;
  background: #e9f0fc;
}

.state-icon.error {
  color: #c73737;
  background: #fceaea;
}

.state-icon i {
  font-size: 1.35rem;
}

.personnel-table {
  min-width: 1040px;
}

.personnel-table th {
  padding: 0.85rem 1rem;
  border-bottom-width: 1px;
  color: #66768b;
  background: #f8fafc;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.035em;
  text-transform: uppercase;
  white-space: nowrap;
}

.personnel-table td {
  padding: 1rem;
  border-color: #edf1f5;
  color: #34445a;
  font-size: 0.875rem;
}

.personnel-table tbody tr {
  transition: background-color 0.15s ease;
}

.personnel-table tbody tr:hover {
  background: #fbfcfe;
}

.person-avatar {
  width: 38px;
  height: 38px;
  flex: 0 0 38px;
  border-radius: 0.75rem;
  color: #2364d2;
  background: #e9f0fc;
  font-size: 0.78rem;
  font-weight: 700;
  text-transform: uppercase;
}

.registration-number,
.debit-number {
  display: inline-flex;
  padding: 0.32rem 0.55rem;
  border-radius: 0.45rem;
  color: #34445a;
  background: #f2f5f9;
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 0.78rem;
  font-weight: 600;
}

.debit-number {
  color: #73510c;
  background: #fff4d8;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  padding: 0.35rem 0.62rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.status-badge.active {
  color: #147442;
  background: #e5f6ed;
}

.status-badge.passive {
  color: #66768b;
  background: #edf1f5;
}

.status-dot {
  width: 0.42rem;
  height: 0.42rem;
  border-radius: 50%;
  background: currentColor;
}

.action-button {
  width: 34px;
  height: 34px;
  padding: 0;
  border: 1px solid transparent;
  color: #66768b;
  background: transparent;
}

.action-button:hover {
  border-color: #cfdcf0;
  color: #2364d2;
  background: #f1f5fc;
}

.action-button.danger:hover {
  border-color: #f0caca;
  color: #c73737;
  background: #fceaea;
}

@media (max-width: 767.98px) {
  .search-box {
    width: 100%;
  }
}
</style>
