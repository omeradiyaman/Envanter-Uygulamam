<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import {
  getSystemStatus,
  type SystemStatus,
} from '../services/systemService'

const systemStatus = ref<SystemStatus | null>(null)
const errorMessage = ref('')
const loading = ref(true)
const abortController = new AbortController()

onMounted(async () => {
  try {
    systemStatus.value = await getSystemStatus(abortController.signal)
  } catch (error: unknown) {
    if (!abortController.signal.aborted) {
      errorMessage.value =
        error instanceof Error ? error.message : 'API bağlantısı kurulamadı.'
    }
  } finally {
    loading.value = false
  }
})

onBeforeUnmount(() => abortController.abort())
</script>

<template>
  <section>
    <div class="mb-4">
      <h2 class="h4 fw-semibold mb-1">Genel Bakış</h2>
      <p class="text-secondary mb-0">
        Envanter modülleri sonraki geliştirme aşamalarında eklenecektir.
      </p>
    </div>

    <div class="row g-4">
      <div class="col-12 col-xl-7">
        <div class="card content-card h-100">
          <div class="card-body p-4">
            <div class="empty-dashboard d-flex flex-column align-items-center justify-content-center text-center">
              <span class="empty-icon d-inline-flex align-items-center justify-content-center mb-3">
                <i class="bi bi-grid-1x2" aria-hidden="true"></i>
              </span>
              <h3 class="h5 fw-semibold">Dashboard hazır</h3>
              <p class="text-secondary mb-0">
                Özet kartları ve istatistikler modüllerle birlikte burada yer alacak.
              </p>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 col-xl-5">
        <div class="card content-card h-100">
          <div class="card-body p-4">
            <div class="d-flex align-items-center justify-content-between mb-4">
              <div>
                <h3 class="h6 fw-semibold mb-1">API Bağlantısı</h3>
                <p class="small text-secondary mb-0">Backend sistem durumu</p>
              </div>
              <span
                class="status-icon d-inline-flex align-items-center justify-content-center"
                :class="{
                  connected: systemStatus,
                  disconnected: errorMessage,
                }"
              >
                <span v-if="loading" class="spinner-border spinner-border-sm" role="status">
                  <span class="visually-hidden">Kontrol ediliyor</span>
                </span>
                <i
                  v-else
                  :class="['bi', systemStatus ? 'bi-check-lg' : 'bi-x-lg']"
                  aria-hidden="true"
                ></i>
              </span>
            </div>

            <div v-if="loading" class="text-secondary small">
              API bağlantısı kontrol ediliyor...
            </div>

            <div v-else-if="systemStatus" class="small">
              <div class="d-flex justify-content-between py-2 border-bottom">
                <span class="text-secondary">Durum</span>
                <span class="fw-semibold text-success">Bağlı</span>
              </div>
              <div class="d-flex justify-content-between py-2 border-bottom">
                <span class="text-secondary">Servis</span>
                <span class="fw-semibold">{{ systemStatus.service }}</span>
              </div>
              <div class="d-flex justify-content-between pt-2">
                <span class="text-secondary">API sürümü</span>
                <span class="fw-semibold">{{ systemStatus.apiVersion }}</span>
              </div>
            </div>

            <div v-else class="alert alert-light border mb-0 small" role="alert">
              <div class="fw-semibold text-danger mb-1">Bağlantı kurulamadı</div>
              <div class="text-secondary">{{ errorMessage }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.empty-dashboard {
  min-height: 260px;
}

.empty-icon,
.status-icon {
  width: 48px;
  height: 48px;
  border-radius: 0.875rem;
  color: #2364d2;
  background: #e9f0fc;
}

.empty-icon i {
  font-size: 1.25rem;
}

.status-icon.connected {
  color: #16844b;
  background: #e5f6ed;
}

.status-icon.disconnected {
  color: #c73737;
  background: #fceaea;
}
</style>
