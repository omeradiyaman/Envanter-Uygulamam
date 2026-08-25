<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import PageHeader from '../components/PageHeader.vue'
import ConfirmationModal from '../components/ConfirmationModal.vue'
import {
  actionLabels,
  entityLabels,
  getAuditLogs,
  undoAuditLog,
  type AuditLog,
} from '../services/auditLogService'

const logs = ref<AuditLog[]>([])
const loading = ref(false)
const undoingId = ref('')
const errorMessage = ref('')
const successMessage = ref('')
const undoCandidate = ref<AuditLog | null>(null)
const undoError = ref('')
const knownUsers = ref<{ id: string; name: string }[]>([])
const filters = reactive({ actionType: '', user: '', entityType: '', from: '', to: '' })

const actionOptions = computed(() => Object.entries(actionLabels))
const entityOptions = computed(() => Object.entries(entityLabels))

const hasActiveFilters = computed(
  () =>
    Boolean(filters.actionType) ||
    Boolean(filters.user) ||
    Boolean(filters.entityType) ||
    Boolean(filters.from) ||
    Boolean(filters.to),
)

function toUtc(value: string, endOfDay = false) {
  if (!value) return undefined
  return new Date(`${value}T${endOfDay ? '23:59:59.999' : '00:00:00'}`).toISOString()
}

async function loadLogs() {
  loading.value = true
  errorMessage.value = ''
  try {
    logs.value = await getAuditLogs({
      actionType: filters.actionType || undefined,
      entityType: filters.entityType || undefined,
      systemUserOnly: filters.user === 'system',
      userId: filters.user && filters.user !== 'system' ? filters.user : undefined,
      from: toUtc(filters.from),
      to: toUtc(filters.to, true),
    })
    for (const log of logs.value) {
      if (log.userId && !knownUsers.value.some((user) => user.id === log.userId)) {
        knownUsers.value.push({ id: log.userId, name: log.userDisplayName })
      }
    }
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Son hareketler yüklenemedi.'
  } finally {
    loading.value = false
  }
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('tr-TR', {
    timeZone: 'Europe/Istanbul',
    dateStyle: 'short',
    timeStyle: 'medium',
  }).format(new Date(value))
}

function requestUndo(log: AuditLog) {
  undoError.value = ''
  undoCandidate.value = log
}

async function confirmUndo() {
  const log = undoCandidate.value
  if (!log) return
  undoingId.value = log.id
  errorMessage.value = ''
  successMessage.value = ''
  try {
    await undoAuditLog(log.id)
    successMessage.value = 'İşlem güvenli şekilde geri alındı.'
    undoCandidate.value = null
    await loadLogs()
  } catch (error) {
    undoError.value = error instanceof Error ? error.message : 'İşlem geri alınamadı.'
  } finally {
    undoingId.value = ''
  }
}

function clearFilters() {
  Object.assign(filters, { actionType: '', user: '', entityType: '', from: '', to: '' })
  void loadLogs()
}

onMounted(loadLogs)
</script>

<template>
  <section>
    <PageHeader
      title="Son Hareketler"
      description="Önemli işlemleri izleyin ve güvenli olanları kontrollü biçimde geri alın."
    >
      <template #actions>
        <button class="btn btn-outline-primary" type="button" :disabled="loading" @click="loadLogs">
          <i class="bi bi-arrow-clockwise me-2"></i>Yenile
        </button>
      </template>
    </PageHeader>

    <AppAlert v-if="errorMessage" variant="danger" dismissible @dismiss="errorMessage = ''">
      {{ errorMessage }}
    </AppAlert>
    <AppAlert v-if="successMessage" variant="success" dismissible @dismiss="successMessage = ''">
      {{ successMessage }}
    </AppAlert>

    <div class="card content-card list-card mb-4">
      <div class="card-body">
        <div class="row g-3 align-items-end">
          <div class="col-sm-6 col-xl-3">
            <label for="audit-action" class="form-label">İşlem tipi</label>
            <select id="audit-action" v-model="filters.actionType" class="form-select">
              <option value="">Tüm işlemler</option>
              <option v-for="[value, label] in actionOptions" :key="value" :value="value">{{ label }}</option>
            </select>
          </div>
          <div class="col-sm-6 col-xl-2">
            <label for="audit-user" class="form-label">Kullanıcı</label>
            <select id="audit-user" v-model="filters.user" class="form-select">
              <option value="">Tüm kullanıcılar</option>
              <option value="system">Sistem</option>
              <option v-for="user in knownUsers" :key="user.id" :value="user.id">{{ user.name }}</option>
            </select>
          </div>
          <div class="col-sm-6 col-xl-2">
            <label for="audit-entity" class="form-label">Kayıt tipi</label>
            <select id="audit-entity" v-model="filters.entityType" class="form-select">
              <option value="">Tüm kayıtlar</option>
              <option v-for="[value, label] in entityOptions" :key="value" :value="value">{{ label }}</option>
            </select>
          </div>
          <div class="col-sm-6 col-xl-2">
            <label for="audit-from" class="form-label">Başlangıç tarihi</label>
            <input id="audit-from" v-model="filters.from" type="date" class="form-control" />
          </div>
          <div class="col-sm-6 col-xl-2">
            <label for="audit-to" class="form-label">Bitiş tarihi</label>
            <input id="audit-to" v-model="filters.to" type="date" class="form-control" />
          </div>
          <div class="col-sm-6 col-xl-1 d-flex gap-2">
            <button class="btn btn-primary flex-fill" type="button" title="Filtrele" aria-label="Hareketleri filtrele" @click="loadLogs">
              <i class="bi bi-funnel" aria-hidden="true"></i>
            </button>
            <button
              v-if="hasActiveFilters"
              class="btn btn-filter-clear flex-fill"
              type="button"
              title="Temizle"
              aria-label="Hareket filtrelerini temizle"
              @click="clearFilters"
            >
              <i class="bi bi-x-lg" aria-hidden="true"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card content-card list-card">
      <DataState v-if="loading" type="loading" title="Hareketler yükleniyor" />

      <DataState
        v-else-if="logs.length === 0"
        type="empty"
        title="Hareket bulunamadı"
        message="Seçili filtrelere uygun kayıt bulunmuyor."
        icon="bi-clock-history"
      >
        <template #actions>
          <button v-if="hasActiveFilters" type="button" class="btn btn-light border" @click="clearFilters">
            Filtreleri Temizle
          </button>
        </template>
      </DataState>

      <div v-else class="table-responsive">
        <table class="table data-table data-table--audit align-middle mb-0">
          <thead>
            <tr>
              <th>Tarih / Saat</th>
              <th>İşlemi yapan</th>
              <th>İşlem</th>
              <th>Etkilenen kayıt</th>
              <th>Açıklama</th>
              <th class="text-end">Geri Al</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="log in logs" :key="log.id">
              <td class="text-nowrap">{{ formatDate(log.createdAt) }}</td>
              <td>{{ log.userDisplayName }}</td>
              <td><span class="badge text-bg-light border">{{ actionLabels[log.actionType] ?? log.actionType }}</span></td>
              <td>
                <div>{{ entityLabels[log.entityType] ?? log.entityType }}</div>
                <span class="cell-secondary entity-id cell-truncate" :title="log.entityId">{{ log.entityId }}</span>
              </td>
              <td class="description-cell">
                <RouterLink :to="{ name: 'activity-detail', params: { id: log.id } }" class="audit-detail-link cell-truncate" :title="`${log.description} detayını aç`">
                  {{ log.description }} <i class="bi bi-arrow-up-right"></i>
                </RouterLink>
                <div v-if="log.undoneAt" class="cell-secondary text-warning">
                  <i class="bi bi-arrow-counterclockwise me-1"></i>{{ formatDate(log.undoneAt) }} tarihinde geri alındı
                </div>
              </td>
              <td class="text-end">
                <button
                  v-if="log.canUndo"
                  class="btn btn-sm btn-outline-warning text-nowrap"
                  type="button"
                  :disabled="undoingId === log.id"
                  @click="requestUndo(log)"
                >
                  <span v-if="undoingId === log.id" class="spinner-border spinner-border-sm me-1"></span>
                  <i v-else class="bi bi-arrow-counterclockwise me-1"></i>Geri Al
                </button>
                <span v-else class="text-secondary">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <ConfirmationModal
      :show="Boolean(undoCandidate)"
      title="İşlem geri alınsın mı?"
      message="Mevcut kayıt durumu kontrol edilecek ve yalnızca güvenliyse geri alınacaktır."
      :detail="undoCandidate?.description ?? ''"
      :confirming="Boolean(undoingId)"
      :error-message="undoError"
      confirm-label="Geri Al"
      confirming-label="Geri alınıyor..."
      @cancel="undoCandidate = null; undoError = ''"
      @confirm="confirmUndo"
    />
  </section>
</template>

<style scoped>
.description-cell {
  min-width: 260px;
}

.entity-id {
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: var(--font-size-label);
}

.audit-detail-link { display: block; color: var(--text-primary); font-weight: 650; text-decoration: none; }
.audit-detail-link:hover { color: var(--primary); }
</style>
