<script setup lang="ts">
import { computed, ref, toRef } from 'vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'

const props = withDefaults(
  defineProps<{
    show: boolean
    title: string
    message: string
    detail: string
    confirming: boolean
    errorMessage: string
    confirmLabel?: string
    confirmingLabel?: string
  }>(),
  {
    confirmLabel: 'Onayla',
    confirmingLabel: 'İşleniyor...',
  },
)

const emit = defineEmits<{
  cancel: []
  confirm: []
}>()

const modalRoot = ref<HTMLElement | null>(null)
useModalAccessibility(toRef(props, 'show'), modalRoot, () => {
  if (!props.confirming) emit('cancel')
})
const tone = computed(() => {
  const value = `${props.title} ${props.confirmLabel}`.toLocaleLowerCase('tr-TR')
  if (/(sil|hurda|imha|kaldır)/.test(value)) return 'danger'
  if (/(iade|geri al|pasif)/.test(value)) return 'warning'
  if (/(zimmet|oluştur|ata)/.test(value)) return 'success'
  return 'primary'
})
const toneIcon = computed(() => tone.value === 'danger' ? 'bi-trash3-fill' : tone.value === 'warning' ? 'bi-exclamation-triangle-fill' : tone.value === 'success' ? 'bi-check2-circle' : 'bi-question-circle-fill')
</script>

<template>
  <Teleport to="body">
    <div
      v-if="show"
      ref="modalRoot"
      class="modal fade show d-block"
      tabindex="-1"
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="confirmationTitle"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content app-modal-content">
          <div class="confirm-modal__body" :class="`is-${tone}`">
            <span class="confirm-icon d-inline-flex align-items-center justify-content-center mb-3">
              <i :class="['bi', toneIcon]" aria-hidden="true"></i>
            </span>
            <h2 id="confirmationTitle" class="h5 fw-semibold mb-2">{{ title }}</h2>
            <p class="confirm-modal__message">{{ message }}</p>
            <div v-if="detail" class="confirm-modal__detail">{{ detail }}</div>
            <div class="confirm-modal__warning">
              Bu kritik işlemi onaylamadan önce seçiminizi kontrol edin.
            </div>
            <div v-if="errorMessage" class="alert alert-danger mt-3 mb-0 text-start" role="alert">
              {{ errorMessage }}
            </div>
          </div>
          <div class="modal-footer justify-content-end px-4 py-3 gap-2">
            <button
              type="button"
              class="btn btn-light border"
              :disabled="confirming"
              @click="emit('cancel')"
            >
              Vazgeç
            </button>
            <button
              type="button"
              :class="['btn', `btn-${tone}`]"
              :disabled="confirming"
              @click="emit('confirm')"
            >
              <span
                v-if="confirming"
                class="spinner-border spinner-border-sm me-2"
                aria-hidden="true"
              ></span>
              {{ confirming ? confirmingLabel : confirmLabel }}
            </button>
          </div>
        </div>
      </div>
    </div>
    <div v-if="show" class="modal-backdrop fade show"></div>
  </Teleport>
</template>

<style scoped>
.modal-footer {
  border-color: var(--border-color);
}

.confirm-icon {
  --confirm-color: var(--primary);
  width: 58px;
  height: 58px;
  border-radius: var(--radius-xl);
  color: var(--confirm-color);
  background: color-mix(in srgb, var(--confirm-color) 11%, var(--surface));
}
.confirm-modal__body.is-danger .confirm-icon{--confirm-color:var(--danger)}
.confirm-modal__body.is-warning .confirm-icon{--confirm-color:var(--warning)}
.confirm-modal__body.is-success .confirm-icon{--confirm-color:var(--success)}
.confirm-icon i {
  font-size: 1.35rem;
}
</style>
