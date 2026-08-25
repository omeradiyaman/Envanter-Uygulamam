<script setup lang="ts">
import { ref, toRef, watch } from 'vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import type { PersonnelDetail } from '../services/personnelService'
import DetailSection from './DetailSection.vue'
import PersonnelDocumentsSection from './PersonnelDocumentsSection.vue'
import { getPersonnelAssignmentHistory, type AssignmentHistory } from '../services/assignmentService'
import StatusBadge from './StatusBadge.vue'
import { authStore } from '../stores/authStore'

const props = defineProps<{
  show: boolean
  personnel: PersonnelDetail | null
  loading: boolean
  errorMessage: string
}>()

const emit = defineEmits<{
  close: []
}>()

const modalRoot = ref<HTMLElement | null>(null)
useModalAccessibility(toRef(props, 'show'), modalRoot, () => emit('close'))
const assignmentHistory = ref<AssignmentHistory[]>([])
const historyLoading = ref(false)
const historyError = ref('')

async function loadHistory() {
  if (!props.personnel) return
  historyLoading.value = true
  historyError.value = ''
  try {
    assignmentHistory.value = await getPersonnelAssignmentHistory(props.personnel.id)
  } catch (error: unknown) {
    historyError.value = error instanceof Error ? error.message : 'Zimmet geçmişi yüklenemedi.'
  } finally {
    historyLoading.value = false
  }
}

watch([() => props.show, () => props.personnel?.id], ([show]) => {
  if (show && props.personnel) void loadHistory()
})

function formatDate(value: string | null): string {
  if (!value) {
    return '—'
  }

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
      aria-labelledby="personnelDetailTitle"
    >
      <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content app-modal-content">
          <div class="modal-header px-4 py-3">
            <h2 id="personnelDetailTitle" class="modal-title h5 fw-semibold mb-0">
              Personel Detayı
            </h2>
            <button
              type="button"
              class="btn-close"
              aria-label="Kapat"
              @click="emit('close')"
            ></button>
          </div>

          <div class="modal-body p-4">
            <div v-if="loading" class="state-panel py-5">
              <div class="spinner-border text-primary mb-3" role="status">
                <span class="visually-hidden">Yükleniyor</span>
              </div>
              <strong>Personel bilgileri yükleniyor</strong>
            </div>

            <div v-else-if="errorMessage" class="app-alert danger mb-0" role="alert">
              <div class="app-alert__content">{{ errorMessage }}</div>
            </div>

            <template v-else-if="personnel">
              <div class="detail-profile mb-4">
                <span class="profile-avatar d-inline-flex align-items-center justify-content-center">
                  {{ personnel.ad.charAt(0) }}{{ personnel.soyad.charAt(0) }}
                </span>
                <div>
                  <h3 class="h5 fw-semibold mb-1">
                    {{ personnel.ad }} {{ personnel.soyad }}
                  </h3>
                  <div class="detail-profile__meta">
                    <span class="text-secondary">{{ personnel.pozisyon }}</span>
                    <span class="registration-number">{{ personnel.sicilNo }}</span>
                    <span
                      class="status-badge"
                      :class="personnel.aktifMi ? 'active' : 'passive'"
                    >
                      <span class="status-dot"></span>
                      {{ personnel.aktifMi ? 'Aktif' : 'Pasif' }}
                    </span>
                  </div>
                </div>
              </div>

              <DetailSection title="Genel Bilgiler" icon="bi-person-vcard" class="is-first">
                <dl class="detail-grid mb-0">
                  <div>
                    <dt>Sicil No</dt>
                    <dd>{{ personnel.sicilNo }}</dd>
                  </div>
                  <div>
                    <dt>Zimmet No</dt>
                    <dd>{{ personnel.zimmetNo || '—' }}</dd>
                  </div>
                  <div>
                    <dt>Departman</dt>
                    <dd>{{ personnel.departman }}</dd>
                  </div>
                  <div>
                    <dt>Pozisyon</dt>
                    <dd>{{ personnel.pozisyon }}</dd>
                  </div>
                  <div>
                    <dt>Oluşturulma</dt>
                    <dd>{{ formatDate(personnel.createdAt) }}</dd>
                  </div>
                  <div>
                    <dt>Son güncelleme</dt>
                    <dd>{{ formatDate(personnel.updatedAt) }}</dd>
                  </div>
                </dl>
              </DetailSection>

              <DetailSection
                :title="`Aktif Cihazlar (${personnel.devices?.length ?? 0})`"
                icon="bi-laptop"
                description="Personele zimmetli cihazlar"
              >
                <div v-if="personnel.devices && personnel.devices.length > 0" class="table-responsive">
                  <table class="table data-table data-table--history align-middle mb-0">
                    <thead>
                      <tr>
                        <th>Cihaz</th>
                        <th>Marka / Model</th>
                        <th>Seri No</th>
                        <th class="text-end">İşlem</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="device in personnel.devices" :key="device.id">
                        <td>
                          <div class="cell-primary">{{ device.cihazAdi }}</div>
                          <span class="cell-secondary">{{ device.categoryName }}</span>
                        </td>
                        <td>
                          <div>{{ device.marka }}</div>
                          <span class="cell-secondary">{{ device.model }}</span>
                        </td>
                        <td>
                          <span class="registration-number">{{ device.seriNo }}</span>
                        </td>
                        <td class="text-end">
                          <RouterLink :to="`/devices/${device.id}`" class="btn btn-sm btn-light border" @click="emit('close')">
                            <i class="bi bi-eye me-1"></i>Detay
                          </RouterLink>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
                <div v-else class="empty-state py-4">
                  <div class="empty-state__title">Zimmetli cihaz yok</div>
                  <div class="empty-state__text">Personele atanmış aktif cihaz bulunmuyor.</div>
                </div>
              </DetailSection>

              <DetailSection title="Zimmet Geçmişi" icon="bi-clock-history" description="Aktif ve iade edilmiş tüm cihaz hareketleri">
                <div v-if="historyLoading" class="state-panel py-4" style="min-height: auto">
                  <div class="spinner-border spinner-border-sm text-primary mb-2"></div><strong>Geçmiş yükleniyor</strong>
                </div>
                <div v-else-if="historyError" class="app-alert warning mb-0"><div class="app-alert__content">{{ historyError }}</div></div>
                <div v-else-if="assignmentHistory.length === 0" class="empty-state py-4"><div class="empty-state__title">Zimmet geçmişi yok</div><div class="empty-state__text">Bu personele ait geçmiş hareket bulunmuyor.</div></div>
                <div v-else class="table-responsive">
                  <table class="table data-table data-table--history align-middle mb-0">
                    <thead><tr><th>Cihaz</th><th>Seri / Envanter</th><th>Zimmet Tarihi</th><th>İade Tarihi</th><th>Durum</th></tr></thead>
                    <tbody><tr v-for="record in assignmentHistory" :key="record.id"><td><strong>{{ record.deviceName }}</strong><small class="d-block text-secondary">{{ record.deviceCategoryName }}</small></td><td><code>{{ record.deviceSeriNo }}</code><small class="d-block text-secondary">{{ record.deviceEnvanterNo }}</small></td><td>{{ formatDate(record.assignedAt) }}</td><td>{{ formatDate(record.returnedAt) }}</td><td><StatusBadge :label="record.isActive ? 'Aktif' : 'İade Edildi'" :variant="record.isActive ? 'success' : 'neutral'" dot /></td></tr></tbody>
                  </table>
                </div>
              </DetailSection>

              <DetailSection title="Belgeler" icon="bi-file-earmark-text">
                <PersonnelDocumentsSection :personnel="personnel" :active="show" />
              </DetailSection>
            </template>
          </div>

          <div class="modal-footer px-4 py-3">
            <RouterLink v-if="personnel && authStore.canEdit.value" :to="{ name: 'personnel-edit', params: { id: personnel.id } }" class="btn btn-warning me-auto">
              <i class="bi bi-pencil-square me-2"></i>Düzenle
            </RouterLink>
            <button type="button" class="btn btn-light border" @click="emit('close')">
              Kapat
            </button>
          </div>
        </div>
      </div>
    </div>
    <div v-if="show" class="modal-backdrop fade show"></div>
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
</style>
