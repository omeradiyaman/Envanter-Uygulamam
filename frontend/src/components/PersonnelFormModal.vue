<script setup lang="ts">
import { reactive, ref, toRef, watch } from 'vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import type {
  PersonnelListItem,
  PersonnelPayload,
} from '../services/personnelService'

const props = defineProps<{
  show: boolean
  personnel: PersonnelListItem | null
  saving: boolean
  errorMessage: string
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: PersonnelPayload]
}>()

const modalRoot = ref<HTMLElement | null>(null)
useModalAccessibility(toRef(props, 'show'), modalRoot, () => {
  if (!props.saving) emit('close')
})

const form = reactive<PersonnelPayload>({
  sicilNo: '',
  ad: '',
  soyad: '',
  departman: '',
  pozisyon: '',
  zimmetNo: null,
  aktifMi: true,
})

function resetForm() {
  form.sicilNo = props.personnel?.sicilNo ?? ''
  form.ad = props.personnel?.ad ?? ''
  form.soyad = props.personnel?.soyad ?? ''
  form.departman = props.personnel?.departman ?? ''
  form.pozisyon = props.personnel?.pozisyon ?? ''
  form.zimmetNo = props.personnel?.zimmetNo ?? null
  form.aktifMi = props.personnel?.aktifMi ?? true
}

function submitForm() {
  emit('submit', {
    sicilNo: form.sicilNo.trim(),
    ad: form.ad.trim(),
    soyad: form.soyad.trim(),
    departman: form.departman.trim(),
    pozisyon: form.pozisyon.trim(),
    zimmetNo: form.zimmetNo?.trim() || null,
    aktifMi: form.aktifMi,
  })
}

watch(
  [() => props.show, () => props.personnel],
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
      aria-labelledby="personnelFormTitle"
    >
      <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
        <div class="modal-content app-modal-content">
          <form @submit.prevent="submitForm">
            <div class="modal-header px-4 py-3">
              <div>
                <span class="modal-eyebrow">PERSONEL FORMU</span>
                <h2 id="personnelFormTitle" class="modal-title h5 fw-semibold">
                  <i :class="['bi', personnel ? 'bi-person-fill-gear' : 'bi-person-plus-fill', 'me-2', 'text-primary']"></i>
                  {{ personnel ? 'Personeli Düzenle' : 'Yeni Personel' }}
                </h2>
                <p class="small text-secondary mb-0 mt-1">
                  Personelin temel kimlik ve görev bilgilerini girin.
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
                <h3 class="form-section__title"><span class="form-section__icon"><i class="bi bi-person-vcard-fill"></i></span>Kimlik Bilgileri</h3>
                <div class="row g-3">
                  <div class="col-12 col-md-6">
                    <label for="sicilNo" class="form-label">Sicil No</label>
                    <input
                      id="sicilNo"
                      v-model="form.sicilNo"
                      type="text"
                      class="form-control"
                      maxlength="50"
                      autocomplete="off"
                      required
                      autofocus
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="zimmetNo" class="form-label">Zimmet No</label>
                    <input
                      id="zimmetNo"
                      v-model="form.zimmetNo"
                      type="text"
                      class="form-control"
                      maxlength="50"
                      autocomplete="off"
                      placeholder="İsteğe bağlı"
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="ad" class="form-label">Ad</label>
                    <input
                      id="ad"
                      v-model="form.ad"
                      type="text"
                      class="form-control"
                      maxlength="100"
                      autocomplete="given-name"
                      required
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="soyad" class="form-label">Soyad</label>
                    <input
                      id="soyad"
                      v-model="form.soyad"
                      type="text"
                      class="form-control"
                      maxlength="100"
                      autocomplete="family-name"
                      required
                    />
                  </div>
                </div>
              </div>

              <div class="form-section">
                <h3 class="form-section__title"><span class="form-section__icon"><i class="bi bi-briefcase-fill"></i></span>Görev Bilgileri</h3>
                <div class="row g-3">
                  <div class="col-12 col-md-6">
                    <label for="departman" class="form-label">Departman</label>
                    <input
                      id="departman"
                      v-model="form.departman"
                      type="text"
                      class="form-control"
                      maxlength="120"
                      autocomplete="organization"
                      required
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <label for="pozisyon" class="form-label">Pozisyon</label>
                    <input
                      id="pozisyon"
                      v-model="form.pozisyon"
                      type="text"
                      class="form-control"
                      maxlength="120"
                      autocomplete="organization-title"
                      required
                    />
                  </div>
                </div>
              </div>

              <div class="form-section">
                <h3 class="form-section__title"><span class="form-section__icon"><i class="bi bi-toggle-on"></i></span>Durum</h3>
                <div class="form-check form-switch form-switch-panel">
                  <input
                    id="aktifMi"
                    v-model="form.aktifMi"
                    class="form-check-input"
                    type="checkbox"
                    role="switch"
                  />
                  <label class="form-check-label" for="aktifMi">
                    Personel aktif
                  </label>
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
