<script setup lang="ts">
defineProps<{
  show: boolean
  title: string
  message: string
  detail: string
  confirming: boolean
  errorMessage: string
}>()

const emit = defineEmits<{
  cancel: []
  confirm: []
}>()
</script>

<template>
  <Teleport to="body">
    <div
      v-if="show"
      class="modal fade show d-block"
      tabindex="-1"
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="confirmationTitle"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-0 shadow-lg">
          <div class="modal-body p-4 text-center">
            <span class="danger-icon d-inline-flex align-items-center justify-content-center mb-3">
              <i class="bi bi-trash3" aria-hidden="true"></i>
            </span>
            <h2 id="confirmationTitle" class="h5 fw-semibold">{{ title }}</h2>
            <p class="text-secondary mb-2">{{ message }}</p>
            <div class="confirmation-detail">{{ detail }}</div>
            <div v-if="errorMessage" class="alert alert-danger mt-3 mb-0" role="alert">
              {{ errorMessage }}
            </div>
          </div>
          <div class="modal-footer justify-content-center px-4 py-3">
            <button
              type="button"
              class="btn btn-light border px-4"
              :disabled="confirming"
              @click="emit('cancel')"
            >
              Vazgeç
            </button>
            <button
              type="button"
              class="btn btn-danger px-4"
              :disabled="confirming"
              @click="emit('confirm')"
            >
              <span
                v-if="confirming"
                class="spinner-border spinner-border-sm me-2"
                aria-hidden="true"
              ></span>
              {{ confirming ? 'Siliniyor...' : 'Personeli Sil' }}
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

.danger-icon {
  width: 58px;
  height: 58px;
  border-radius: 1rem;
  color: #c73737;
  background: #fceaea;
}

.danger-icon i {
  font-size: 1.35rem;
}

.confirmation-detail {
  display: inline-block;
  padding: 0.5rem 0.85rem;
  border-radius: 0.6rem;
  color: #34445a;
  background: #f2f5f9;
  font-size: 0.875rem;
  font-weight: 600;
}
</style>
