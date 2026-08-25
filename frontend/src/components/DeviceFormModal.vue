<script setup lang="ts">
import { computed, reactive, ref, toRef, watch } from 'vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import {
  type Device,
  type DevicePayload,
  type DeviceCategory,
  DeviceStatuses,
} from '../services/deviceService'

const props = defineProps<{
  show: boolean
  device: Device | null
  categories: DeviceCategory[]
  saving: boolean
  errorMessage: string
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: DevicePayload]
}>()

const modalRoot = ref<HTMLElement | null>(null)
useModalAccessibility(toRef(props, 'show'), modalRoot, () => {
  if (!props.saving) emit('close')
})

const isAssigned = computed(() => Boolean(props.device?.personelId))
const editableStatuses = computed(() =>
  isAssigned.value
    ? DeviceStatuses.filter((status) => status.id === 2)
    : DeviceStatuses.filter((status) => status.id !== 2),
)

const form = reactive<DevicePayload>({
  cihazAdi: '',
  seriNo: '',
  envanterNo: '',
  barkodNo: null,
  marka: '',
  model: '',
  categoryId: '',
  status: 1, // Default InStock
  personelId: null,
  warrantyStartDate: null,
  warrantyEndDate: null,
  warrantyProvider: null,
  warrantyNote: null,
})

function resetForm() {
  if (props.device) {
    form.cihazAdi = props.device.cihazAdi
    form.seriNo = props.device.seriNo
    form.envanterNo = props.device.envanterNo
    form.barkodNo = props.device.barkodNo
    form.marka = props.device.marka
    form.model = props.device.model
    form.status = props.device.status
    
    form.categoryId = props.device.categoryId
    form.personelId = props.device.personelId
    form.warrantyStartDate = props.device.warrantyStartDate
    form.warrantyEndDate = props.device.warrantyEndDate
    form.warrantyProvider = props.device.warrantyProvider
    form.warrantyNote = props.device.warrantyNote
  } else {
    form.cihazAdi = ''
    form.seriNo = ''
    form.envanterNo = ''
    form.barkodNo = null
    form.marka = ''
    form.model = ''
    form.categoryId = ''
    form.status = 1
    form.personelId = null
    form.warrantyStartDate = null
    form.warrantyEndDate = null
    form.warrantyProvider = null
    form.warrantyNote = null
  }
}

function submitForm() {
  emit('submit', {
    cihazAdi: form.cihazAdi.trim(),
    seriNo: form.seriNo.trim(),
    envanterNo: form.envanterNo.trim(),
    barkodNo: form.barkodNo?.trim() || null,
    marka: form.marka.trim(),
    model: form.model.trim(),
    categoryId: form.categoryId,
    status: Number(form.status),
    personelId: form.personelId || null,
    warrantyStartDate: form.warrantyStartDate || null,
    warrantyEndDate: form.warrantyEndDate || null,
    warrantyProvider: form.warrantyProvider?.trim() || null,
    warrantyNote: form.warrantyNote?.trim() || null,
  })
}

watch(
  [() => props.show, () => props.device],
  ([show]) => {
    if (show) {
      resetForm()
    }
  },
)
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
    >
      <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content app-modal-content">
          <form @submit.prevent="submitForm">
            <div class="modal-header px-4 py-3">
              <div>
                <span class="modal-eyebrow">ENVANTER FORMU</span>
                <h2 class="modal-title h5 fw-semibold">
                  <i :class="['bi', device ? 'bi-pencil-square' : 'bi-plus-circle-fill', 'me-2', 'text-primary']"></i>
                  {{ device ? 'Cihazı Düzenle' : 'Yeni Cihaz' }}
                </h2>
                <p class="small text-secondary mb-0 mt-1">
                  Cihaz bilgilerini girin.
                </p>
              </div>
              <button
                type="button"
                class="btn-close"
                aria-label="Kapat"
                :disabled="saving"
                @click="emit('close')"
              ></button>
            </div>

            <div class="modal-body p-4">
              <div v-if="errorMessage" class="app-alert danger" role="alert">
                <div class="app-alert__content">{{ errorMessage }}</div>
              </div>

              <div class="form-section">
                <h3 class="form-section__title"><span class="form-section__icon"><i class="bi bi-pc-display-horizontal"></i></span>Temel Bilgiler</h3>
                <div class="row g-3">
                  <div class="col-12 col-md-6">
                    <label for="device-name" class="form-label">Cihaz Adı</label>
                    <input id="device-name" v-model="form.cihazAdi" type="text" class="form-control" maxlength="150" required autofocus />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="device-category" class="form-label">Kategori</label>
                    <select id="device-category" v-model="form.categoryId" class="form-select" required>
                      <option value="" disabled>Seçiniz</option>
                      <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
                    </select>
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="device-brand" class="form-label">Marka</label>
                    <input id="device-brand" v-model="form.marka" type="text" class="form-control" maxlength="100" required />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="device-model" class="form-label">Model</label>
                    <input id="device-model" v-model="form.model" type="text" class="form-control" maxlength="100" required />
                  </div>
                </div>
              </div>

              <div class="form-section">
                <h3 class="form-section__title"><span class="form-section__icon"><i class="bi bi-upc-scan"></i></span>Envanter Bilgileri</h3>
                <div class="row g-3">
                  <div class="col-12 col-md-6">
                    <label for="device-serial" class="form-label">Seri No</label>
                    <input id="device-serial" v-model="form.seriNo" type="text" class="form-control" maxlength="100" required />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="device-inventory" class="form-label">Envanter No</label>
                    <input id="device-inventory" v-model="form.envanterNo" type="text" class="form-control" maxlength="100" required />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="device-status" class="form-label">Durum</label>
                    <select id="device-status" v-model="form.status" class="form-select" :class="{ 'read-only-control': isAssigned }" :disabled="isAssigned" required>
                      <option v-for="status in editableStatuses" :key="status.id" :value="status.id">{{ status.name }}</option>
                    </select>
                    <div v-if="isAssigned" class="form-text">Durumu değiştirmek için önce zimmeti iade alın.</div>
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="device-assignee" class="form-label">Zimmetli Personel</label>
                    <input
                      id="device-assignee"
                      type="text"
                      class="form-control read-only-control"
                      :value="device?.personelName || 'Zimmetsiz'"
                      disabled
                    />
                    <div class="form-text">Personel ilişkisi zimmet/iade işlemlerinden yönetilir.</div>
                  </div>
                </div>
              </div>

              <div class="form-section">
                <h3 class="form-section__title">
                  <span class="form-section__icon"><i class="bi bi-shield-check"></i></span>
                  Garanti Bilgileri
                  <span class="form-section__hint">(opsiyonel)</span>
                </h3>
                <div class="row g-3">
                  <div class="col-12 col-md-6">
                    <label for="warranty-start" class="form-label">Garanti Başlangıç Tarihi</label>
                    <input id="warranty-start" v-model="form.warrantyStartDate" type="date" class="form-control">
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="warranty-end" class="form-label">Garanti Bitiş Tarihi</label>
                    <input id="warranty-end" v-model="form.warrantyEndDate" type="date" class="form-control" :min="form.warrantyStartDate || undefined">
                  </div>
                  <div class="col-12">
                    <label for="warranty-provider" class="form-label">Garanti Firması</label>
                    <input id="warranty-provider" v-model="form.warrantyProvider" type="text" class="form-control" maxlength="200" placeholder="Üretici, distribütör veya servis firması">
                  </div>
                  <div class="col-12">
                    <label for="warranty-note" class="form-label">Garanti Notu</label>
                    <textarea id="warranty-note" v-model="form.warrantyNote" class="form-control" rows="3" maxlength="1000" placeholder="Opsiyonel garanti açıklaması"></textarea>
                  </div>
                </div>
              </div>
            </div>

            <div class="modal-footer px-4 py-3">
              <button
                type="button"
                class="btn btn-light border"
                :disabled="saving"
                @click="emit('close')"
              >
                Vazgeç
              </button>
              <button type="submit" class="btn btn-primary px-4" :disabled="saving">
                <span
                  v-if="saving"
                  class="spinner-border spinner-border-sm me-2"
                  aria-hidden="true"
                ></span>
                {{ saving ? 'Kaydediliyor...' : 'Kaydet' }}
              </button>
            </div>
          </form>
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
</style>
