<script setup lang="ts">
import { ref, toRef, watch } from 'vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import { getDeviceQrCode, getWarrantyBadgeClass, type Device, type DeviceQrCode } from '../services/deviceService'
import { getStatusColor } from '../services/deviceService'
import type { AssignmentHistory } from '../services/assignmentService'
import { getDeviceAssignmentHistory, unassignDevice } from '../services/assignmentService'
import AssignDeviceModal from './AssignDeviceModal.vue'
import DetailSection from './DetailSection.vue'
import DeviceLabelModal from './DeviceLabelModal.vue'
import { authStore } from '../stores/authStore'

const props = defineProps<{
  show: boolean
  device: Device | null
  loading: boolean
  errorMessage: string
}>()

const emit = defineEmits<{
  close: []
  refresh: []
}>()

const modalRoot = ref<HTMLElement | null>(null)
const qrModalRoot = ref<HTMLElement | null>(null)
useModalAccessibility(toRef(props, 'show'), modalRoot, () => emit('close'))

// Assignment history
const history = ref<AssignmentHistory[]>([])
const historyLoading = ref(false)
const historyError = ref('')

// Assign modal
const showAssignModal = ref(false)
const showQrModal = ref(false)
const showLabelModal = ref(false)
useModalAccessibility(showQrModal, qrModalRoot, () => { showQrModal.value = false })
const qrCode = ref<DeviceQrCode | null>(null)
const qrLoading = ref(false)
const qrError = ref('')

// Unassign
const unassigning = ref(false)
const unassignError = ref('')
const showUnassignConfirm = ref(false)

async function loadHistory() {
  if (!props.device) return
  historyLoading.value = true
  historyError.value = ''
  try {
    history.value = await getDeviceAssignmentHistory(props.device.id)
  } catch (e: unknown) {
    historyError.value = e instanceof Error ? e.message : 'Geçmiş yüklenemedi.'
  } finally {
    historyLoading.value = false
  }
}

async function openQrModal() {
  if (!props.device) return
  showQrModal.value = true
  qrError.value = ''
  qrLoading.value = true
  try {
    qrCode.value = await getDeviceQrCode(props.device.id)
  } catch (error: unknown) {
    qrError.value = error instanceof Error ? error.message : 'QR kod oluşturulamadı.'
  } finally {
    qrLoading.value = false
  }
}

async function confirmUnassign() {
  if (!props.device) return
  unassigning.value = true
  unassignError.value = ''
  try {
    await unassignDevice({ deviceId: props.device.id })
    showUnassignConfirm.value = false
    emit('refresh')
    loadHistory()
  } catch (e: unknown) {
    unassignError.value = e instanceof Error ? e.message : 'İade işlemi başarısız.'
    showUnassignConfirm.value = false
  } finally {
    unassigning.value = false
  }
}

function onAssigned() {
  emit('refresh')
  loadHistory()
}

watch(() => props.show, (show) => {
  if (show && props.device) {
    loadHistory()
    showUnassignConfirm.value = false
    unassignError.value = ''
    showQrModal.value = false
    showLabelModal.value = false
  }
})

watch(() => props.device, (device) => {
  if (device && props.show) {
    loadHistory()
  }
})

function formatDate(value: string | null): string {
  if (!value) return '—'
  return new Intl.DateTimeFormat('tr-TR', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="show"
      ref="modalRoot"
      class="modal fade show d-block"
      tabindex="-1"
      role="dialog"
      aria-modal="true"
      aria-labelledby="deviceDetailTitle"
    >
      <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable device-detail-dialog">
        <div class="modal-content app-modal-content device-detail-modal">
          <div class="modal-header px-4 py-3">
            <div>
              <span class="modal-eyebrow">ENVANTER GÖRÜNÜMÜ</span>
              <h2 id="deviceDetailTitle" class="modal-title h5 fw-semibold"><i class="bi bi-hdd-network-fill me-2 text-primary"></i>Cihaz Detayı</h2>
            </div>
            <button type="button" class="btn-close" aria-label="Kapat" @click="emit('close')"></button>
          </div>

          <div class="modal-body p-4">
            <div v-if="loading" class="text-center py-5">
              <div class="spinner-border text-primary mb-3" role="status">
                <span class="visually-hidden">Yükleniyor</span>
              </div>
              <p class="text-secondary mb-0">Cihaz bilgileri yükleniyor...</p>
            </div>

            <div v-else-if="errorMessage" class="alert alert-danger mb-0" role="alert">
              {{ errorMessage }}
            </div>

            <template v-else-if="device">
              <section class="device-reference-hero mb-4">
                <span class="device-reference-hero__visual">
                  <i class="bi bi-pc-display-horizontal"></i>
                </span>
                <div class="device-reference-hero__main">
                  <span class="device-reference-hero__eyebrow"><i class="bi bi-hdd-stack-fill"></i>{{ device.categoryName.toLocaleUpperCase('tr-TR') }}</span>
                  <h3 class="device-reference-hero__title">{{ device.marka }} {{ device.model }}</h3>
                  <p class="device-reference-hero__name">{{ device.cihazAdi }}</p>
                  <div class="device-reference-hero__identity">
                    <span><small>ENVANTER KİMLİĞİ</small><code>{{ device.envanterNo }}</code></span>
                    <span><small>SERİ NUMARASI</small><strong>{{ device.seriNo }}</strong></span>
                  </div>
                </div>
                <span class="badge rounded-pill device-reference-hero__badge" :class="`text-bg-${getStatusColor(device.status)}`">{{ device.statusName }}</span>
              </section>

              <div class="device-reference-toolbar mb-4">
                <section class="device-reference-toolbar__group device-reference-toolbar__group--edit">
                  <span class="device-reference-toolbar__label">ETİKETLEME</span>
                  <div>
                    <button type="button" class="btn btn-info btn-sm text-white" @click="openQrModal"><i class="bi bi-qr-code me-1"></i>QR Kod</button>
                    <button type="button" class="btn btn-outline-primary btn-sm" @click="showLabelModal = true"><i class="bi bi-tag-fill me-1"></i>Etiket</button>
                  </div>
                </section>
                <section class="device-reference-toolbar__group device-reference-toolbar__group--assignment">
                  <span class="device-reference-toolbar__label">ZİMMET</span>
                  <div>
                    <button v-if="device.status === 1 && authStore.canEdit.value" type="button" class="btn btn-success btn-sm" @click="showAssignModal = true"><i class="bi bi-person-check-fill me-1"></i>Personele Zimmetle</button>
                    <span v-else-if="device.personelName" class="device-reference-toolbar__hint"><i class="bi bi-person-fill"></i>{{ device.personelName }}</span>
                    <span v-else class="device-reference-toolbar__hint">İşlem kullanılamıyor</span>
                  </div>
                </section>
                <section class="device-reference-toolbar__group device-reference-toolbar__group--danger">
                  <span class="device-reference-toolbar__label">YAŞAM DÖNGÜSÜ</span>
                  <div><span class="device-reference-toolbar__status"><i class="bi bi-activity"></i>{{ device.statusName }}</span></div>
                </section>
              </div>

              <div class="device-detail-warranty-note mb-4">
                <i class="bi bi-shield-fill-exclamation"></i>
                <span>Garanti hesabı yalnızca kayıttaki gerçek tarihlere göre yapılır; tarihi olmayan cihazlara otomatik süre atanmaz.</span>
              </div>

              <DetailSection title="Genel Bilgiler" icon="bi-info-circle" class="is-first">
                <dl class="detail-grid mb-0">
                  <div>
                    <dt>Marka / Model</dt>
                    <dd>{{ device.marka }} {{ device.model }}</dd>
                  </div>
                  <div>
                    <dt>Zimmetli Personel</dt>
                    <dd>{{ device.personelName || '—' }}</dd>
                  </div>
                  <div>
                    <dt>Seri No</dt>
                    <dd class="font-monospace">{{ device.seriNo }}</dd>
                  </div>
                  <div>
                    <dt>Envanter No</dt>
                    <dd class="font-monospace">{{ device.envanterNo }}</dd>
                  </div>
                  <div>
                    <dt>Oluşturulma</dt>
                    <dd>{{ formatDate(device.createdAt) }}</dd>
                  </div>
                  <div>
                    <dt>Son Güncelleme</dt>
                    <dd>{{ formatDate(device.updatedAt) }}</dd>
                  </div>
                </dl>
              </DetailSection>

              <DetailSection title="Garanti" icon="bi-shield-check">
                <section class="warranty-panel mb-0" :class="`warranty-${device.warrantyStatus}`">
                  <div class="d-flex align-items-start justify-content-between gap-3 mb-3">
                    <div>
                      <p class="small text-secondary mb-0">{{ device.warrantyMessage }}</p>
                    </div>
                    <span class="badge rounded-pill" :class="getWarrantyBadgeClass(device.warrantyStatus)">
                      {{ device.warrantyStatusName }}
                    </span>
                  </div>
                  <dl class="warranty-grid mb-0">
                    <div><dt>Başlangıç</dt><dd>{{ device.warrantyStartDate ? formatDate(device.warrantyStartDate) : '—' }}</dd></div>
                    <div><dt>Bitiş</dt><dd>{{ device.warrantyEndDate ? formatDate(device.warrantyEndDate) : '—' }}</dd></div>
                    <div><dt>Firma</dt><dd>{{ device.warrantyProvider || '—' }}</dd></div>
                    <div class="warranty-note"><dt>Not</dt><dd>{{ device.warrantyNote || '—' }}</dd></div>
                  </dl>
                </section>
              </DetailSection>

              <DetailSection title="QR / Etiket" icon="bi-qr-code">
                <div class="d-flex flex-wrap gap-2">
                  <button type="button" class="btn btn-outline-primary" @click="openQrModal">
                    <i class="bi bi-qr-code me-2"></i>QR Kod Göster
                  </button>
                  <button type="button" class="btn btn-outline-secondary" @click="showLabelModal = true">
                    <i class="bi bi-tag me-2"></i>Etiket Önizle / Yazdır
                  </button>
                </div>
              </DetailSection>

              <DetailSection title="Zimmet Bilgisi" icon="bi-person-check">
                <div class="assignment-action-panel mb-0">
                  <div v-if="unassignError" class="app-alert danger mb-3">
                    <div class="app-alert__content">
                      <i class="bi bi-exclamation-circle me-2"></i>{{ unassignError }}
                    </div>
                  </div>

                  <div v-if="device.status === 1" class="d-flex align-items-center gap-3 flex-wrap">
                    <div class="flex-grow-1">
                      <div class="fw-semibold text-success">
                        <i class="bi bi-box-seam me-2"></i>Stokta — Zimmetsiz
                      </div>
                      <p class="text-secondary small mb-0">Bu cihaz şu anda herhangi bir personele zimmetli değildir.</p>
                    </div>
                    <button v-if="authStore.canEdit.value" type="button" class="btn btn-primary" @click="showAssignModal = true">
                      <i class="bi bi-person-check me-2"></i>Personele Zimmetle
                    </button>
                  </div>

                  <div v-else-if="device.status === 2" class="d-flex align-items-center gap-3 flex-wrap">
                    <div class="flex-grow-1">
                      <div class="fw-semibold">
                        <i class="bi bi-person-fill me-2 text-primary"></i>
                        Zimmetli — {{ device.personelName }}
                      </div>
                      <p class="text-secondary small mb-0">Cihazı geri almak için iade işlemi başlatın.</p>
                    </div>
                    <div v-if="authStore.canEdit.value && !showUnassignConfirm">
                      <button type="button" class="btn btn-outline-danger" @click="showUnassignConfirm = true">
                        <i class="bi bi-arrow-return-left me-2"></i>Zimmeti İade Al
                      </button>
                    </div>
                    <div v-else-if="authStore.canEdit.value" class="d-flex gap-2">
                      <button class="btn btn-light border btn-sm" :disabled="unassigning" @click="showUnassignConfirm = false">
                        İptal
                      </button>
                      <button class="btn btn-danger btn-sm" :disabled="unassigning" @click="confirmUnassign">
                        <span v-if="unassigning" class="spinner-border spinner-border-sm me-1"></span>
                        {{ unassigning ? 'İşleniyor...' : 'Onayla, İade Al' }}
                      </button>
                    </div>
                  </div>

                  <div v-else class="text-secondary small">
                    <i class="bi bi-info-circle me-2"></i>
                    Bu durumdaki cihazlar zimmetlenemez.
                  </div>
                </div>
              </DetailSection>

              <DetailSection title="Zimmet Geçmişi" icon="bi-clock-history">
                <div v-if="historyLoading" class="state-panel py-4" style="min-height: auto">
                  <div class="spinner-border spinner-border-sm text-primary mb-2"></div>
                  <strong>Geçmiş yükleniyor</strong>
                </div>

                <div v-else-if="historyError" class="app-alert warning mb-0">
                  <div class="app-alert__content">{{ historyError }}</div>
                </div>

                <div v-else-if="history.length === 0" class="empty-state py-4">
                  <div class="empty-state__title">Zimmet geçmişi yok</div>
                  <div class="empty-state__text">Bu cihaz henüz kimseye zimmetlenmemiş.</div>
                </div>

                <div v-else class="table-responsive">
                  <table class="table data-table data-table--history align-middle mb-0">
                    <thead>
                      <tr>
                        <th>Personel</th>
                        <th>Sicil No</th>
                        <th>Zimmet Tarihi</th>
                        <th>İade Tarihi</th>
                        <th>Durum</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="record in history" :key="record.id">
                        <td class="cell-primary">{{ record.personnelName }}</td>
                        <td><span class="registration-number">{{ record.personnelSicilNo }}</span></td>
                        <td>{{ formatDate(record.assignedAt) }}</td>
                        <td>{{ formatDate(record.returnedAt) }}</td>
                        <td>
                          <span
                            class="status-badge"
                            :class="record.isActive ? 'active' : 'passive'"
                          >
                            {{ record.isActive ? 'Aktif' : 'İade Edildi' }}
                          </span>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </DetailSection>
            </template>
          </div>

          <div class="modal-footer px-4 py-3">
            <RouterLink v-if="device && authStore.canEdit.value" :to="{ name: 'device-edit', params: { id: device.id } }" class="btn btn-warning me-auto">
              <i class="bi bi-pencil-square me-2"></i>Düzenle
            </RouterLink>
            <button type="button" class="btn btn-light border" @click="emit('close')">Kapat</button>
          </div>
        </div>
      </div>
    </div>
    <div v-if="show" class="modal-backdrop fade show"></div>
  </Teleport>

  <AssignDeviceModal
    v-if="device"
    :show="showAssignModal"
    :device-id="device.id"
    :device-name="device.cihazAdi"
    @close="showAssignModal = false"
    @assigned="onAssigned"
  />

  <DeviceLabelModal
    v-if="device"
    :show="showLabelModal"
    :devices="[device]"
    @close="showLabelModal = false"
  />

  <Teleport to="body">
    <div v-if="showQrModal" ref="qrModalRoot" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true" aria-labelledby="deviceQrTitle">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content app-modal-content">
          <div class="modal-header"><h2 id="deviceQrTitle" class="modal-title h5">QR Kod</h2><button type="button" class="btn-close" aria-label="Kapat" @click="showQrModal = false"></button></div>
          <div class="modal-body text-center p-4">
            <div v-if="qrLoading" class="py-5"><div class="spinner-border text-primary"></div></div>
            <div v-else-if="qrError" class="alert alert-danger text-start mb-0">{{ qrError }}</div>
            <template v-else-if="qrCode">
              <img class="qr-preview" :src="`data:image/png;base64,${qrCode.pngBase64}`" :alt="`${device?.cihazAdi} QR kodu`">
              <p class="small text-secondary mt-3 mb-0 text-break">{{ qrCode.deviceUrl }}</p>
            </template>
          </div>
        </div>
      </div>
    </div>
    <div v-if="showQrModal" class="modal-backdrop fade show"></div>
  </Teleport>
</template>

<style scoped>
.modal-header,
.modal-footer {
  border-color: var(--border-color);
}

.profile-avatar {
  font-weight: var(--font-weight-bold);
  text-transform: uppercase;
}

.qr-preview {
  width: min(100%, 320px);
  aspect-ratio: 1;
  image-rendering: pixelated;
}

.table th {
  font-size: var(--font-size-table-header);
  font-weight: var(--font-weight-bold);
  letter-spacing: 0.03em;
  text-transform: uppercase;
  padding: 0.6rem 0.75rem;
  background: var(--table-header-bg);
}

.table td {
  padding: 0.65rem 0.75rem;
  font-size: var(--font-size-secondary);
}
</style>
