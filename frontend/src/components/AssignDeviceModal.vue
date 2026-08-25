<script setup lang="ts">
import { ref, toRef, watch } from 'vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import { getPersonnelList, type PersonnelListItem } from '../services/personnelService'
import { assignDevice } from '../services/assignmentService'

const props = defineProps<{
  show: boolean
  deviceId: string
  deviceName: string
}>()

const emit = defineEmits<{
  close: []
  assigned: []
}>()

const modalRoot = ref<HTMLElement | null>(null)
useModalAccessibility(toRef(props, 'show'), modalRoot, () => {
  if (!saving.value) emit('close')
})

const personnelList = ref<PersonnelListItem[]>([])
const loadingPersonnel = ref(false)
const personnelError = ref('')

const selectedPersonnelId = ref<string>('')
const note = ref('')
const saving = ref(false)
const errorMessage = ref('')
const showConfirm = ref(false)

async function loadPersonnel() {
  if (personnelList.value.length > 0) return
  loadingPersonnel.value = true
  personnelError.value = ''
  try {
    const all = await getPersonnelList()
    personnelList.value = all.filter(p => p.aktifMi)
  } catch (e: unknown) {
    personnelError.value = e instanceof Error ? e.message : 'Personel listesi yüklenemedi.'
  } finally {
    loadingPersonnel.value = false
  }
}

function openModal() {
  selectedPersonnelId.value = ''
  note.value = ''
  errorMessage.value = ''
  showConfirm.value = false
  loadPersonnel()
}

function requestConfirm() {
  if (!selectedPersonnelId.value) return
  showConfirm.value = true
}

async function confirmAssign() {
  saving.value = true
  errorMessage.value = ''
  try {
    await assignDevice({
      deviceId: props.deviceId,
      personnelId: selectedPersonnelId.value,
      note: note.value.trim() || null,
    })
    emit('assigned')
    emit('close')
  } catch (e: unknown) {
    errorMessage.value = e instanceof Error ? e.message : 'Zimmetleme işlemi başarısız.'
    showConfirm.value = false
  } finally {
    saving.value = false
  }
}

const selectedPersonnel = () =>
  personnelList.value.find(p => p.id === selectedPersonnelId.value)

watch(() => props.show, (show) => { if (show) openModal() })
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
          <div class="modal-header px-4 py-3">
            <div>
              <h2 class="modal-title h5 fw-semibold mb-0">Cihazı Zimmetle</h2>
              <p class="text-secondary small mb-0 mt-1">
                <span class="fw-medium text-body">{{ deviceName }}</span> — personel seçin
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
            <div v-if="errorMessage" class="alert alert-danger" role="alert">
              <i class="bi bi-exclamation-circle me-2"></i>
              {{ errorMessage }}
            </div>

            <!-- Personnel search/select -->
            <div v-if="!showConfirm">
              <div v-if="loadingPersonnel" class="text-center py-4">
                <div class="spinner-border spinner-border-sm text-primary me-2"></div>
                Personel listesi yükleniyor...
              </div>
              <div v-else-if="personnelError" class="alert alert-warning">{{ personnelError }}</div>
              <div v-else>
                <p class="text-secondary small mb-3">
                  Aşağıdan zimmetlenecek personeli seçin. Yalnızca aktif personeller listelenmektedir.
                </p>
                <div v-if="personnelList.length" class="personnel-select-list" role="radiogroup" aria-label="Zimmetlenecek personel">
                  <button
                    v-for="person in personnelList"
                    :key="person.id"
                    type="button"
                    class="personnel-option"
                    :class="{ selected: selectedPersonnelId === person.id }"
                    role="radio"
                    :aria-checked="selectedPersonnelId === person.id"
                    @click="selectedPersonnelId = person.id"
                  >
                    <span class="person-avatar-sm">
                      {{ person.ad.charAt(0) }}{{ person.soyad.charAt(0) }}
                    </span>
                    <div class="flex-grow-1 min-w-0">
                      <div class="fw-semibold">{{ person.ad }} {{ person.soyad }}</div>
                      <div class="text-secondary small">
                        <span class="me-2">{{ person.sicilNo }}</span>
                        <span class="me-2">·</span>
                        <span class="me-2">{{ person.departman }}</span>
                        <span v-if="person.zimmetNo">
                          <span class="me-2">·</span>
                          <span class="badge text-bg-light border">{{ person.zimmetNo }}</span>
                        </span>
                      </div>
                    </div>
                    <i
                      v-if="selectedPersonnelId === person.id"
                      class="bi bi-check-circle-fill text-primary fs-5"
                    ></i>
                  </button>
                </div>
                <div v-else class="empty-state py-4">
                  <div class="empty-state__title">Aktif personel bulunamadı</div>
                  <div class="empty-state__text">Zimmet işlemi için önce aktif bir personel kaydı oluşturun.</div>
                </div>

                <div class="mt-3">
                  <label for="assignment-note" class="form-label">Not (İsteğe bağlı)</label>
                  <textarea
                    id="assignment-note"
                    v-model="note"
                    class="form-control"
                    rows="2"
                    maxlength="500"
                    placeholder="Zimmet notu..."
                  ></textarea>
                </div>
              </div>
            </div>

            <!-- Confirmation step -->
            <div v-else class="text-center py-3">
              <div class="confirm-icon mb-3">
                <i class="bi bi-person-check"></i>
              </div>
              <h5 class="fw-semibold mb-1">Zimmeti onaylayın</h5>
              <p class="text-secondary mb-3">
                <strong>{{ deviceName }}</strong> cihazı<br>
                <strong>{{ selectedPersonnel()?.ad }} {{ selectedPersonnel()?.soyad }}</strong>
                ({{ selectedPersonnel()?.sicilNo }}) personeline zimmetlenecek.
              </p>
              <p v-if="note" class="text-secondary small">
                Not: {{ note }}
              </p>
            </div>
          </div>

          <div class="modal-footer px-4 py-3">
            <template v-if="!showConfirm">
              <button
                type="button"
                class="btn btn-light border"
                @click="emit('close')"
              >Vazgeç</button>
              <button
                type="button"
                class="btn btn-primary px-4"
                :disabled="!selectedPersonnelId"
                @click="requestConfirm"
              >
                Devam Et
                <i class="bi bi-arrow-right ms-2"></i>
              </button>
            </template>
            <template v-else>
              <button
                type="button"
                class="btn btn-light border"
                :disabled="saving"
                @click="showConfirm = false"
              >Geri Dön</button>
              <button
                type="button"
                class="btn btn-success px-4"
                :disabled="saving"
                @click="confirmAssign"
              >
                <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
                {{ saving ? 'İşleniyor...' : 'Zimmeti Onayla' }}
              </button>
            </template>
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

.personnel-select-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  max-height: 320px;
  overflow-y: auto;
}

.personnel-option {
  width: 100%;
  color: var(--text-body);
  text-align: left;
}
</style>
