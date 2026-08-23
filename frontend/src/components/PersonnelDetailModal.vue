<script setup lang="ts">
import type { PersonnelDetail } from '../services/personnelService'

defineProps<{
  show: boolean
  personnel: PersonnelDetail | null
  loading: boolean
  errorMessage: string
}>()

const emit = defineEmits<{
  close: []
}>()

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
      class="modal fade show d-block"
      tabindex="-1"
      role="dialog"
      aria-modal="true"
      aria-labelledby="personnelDetailTitle"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-0 shadow-lg">
          <div class="modal-header px-4 py-3">
            <h2 id="personnelDetailTitle" class="modal-title h5 fw-semibold">
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
            <div v-if="loading" class="detail-loading text-center py-5">
              <div class="spinner-border text-primary mb-3" role="status">
                <span class="visually-hidden">Yükleniyor</span>
              </div>
              <p class="text-secondary mb-0">Personel bilgileri yükleniyor...</p>
            </div>

            <div v-else-if="errorMessage" class="alert alert-danger mb-0" role="alert">
              {{ errorMessage }}
            </div>

            <template v-else-if="personnel">
              <div class="personnel-profile d-flex align-items-center gap-3 mb-4">
                <span class="profile-avatar d-inline-flex align-items-center justify-content-center">
                  {{ personnel.ad.charAt(0) }}{{ personnel.soyad.charAt(0) }}
                </span>
                <div>
                  <h3 class="h5 fw-semibold mb-1">
                    {{ personnel.ad }} {{ personnel.soyad }}
                  </h3>
                  <div class="d-flex align-items-center gap-2">
                    <span class="text-secondary small">{{ personnel.pozisyon }}</span>
                    <span
                      class="badge rounded-pill"
                      :class="personnel.aktifMi ? 'text-bg-success' : 'text-bg-secondary'"
                    >
                      {{ personnel.aktifMi ? 'Aktif' : 'Pasif' }}
                    </span>
                  </div>
                </div>
              </div>

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
            </template>
          </div>

          <div class="modal-footer px-4 py-3">
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
  width: 58px;
  height: 58px;
  flex: 0 0 58px;
  border-radius: 1rem;
  color: #2364d2;
  background: #e9f0fc;
  font-weight: 700;
  text-transform: uppercase;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.85rem;
}

.detail-grid > div {
  padding: 0.85rem 1rem;
  border: 1px solid var(--border-color);
  border-radius: 0.75rem;
  background: #f8fafc;
}

.detail-grid dt {
  margin-bottom: 0.3rem;
  color: #78879a;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.detail-grid dd {
  margin: 0;
  color: #263548;
  font-size: 0.9rem;
  font-weight: 600;
}

@media (max-width: 575.98px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>
