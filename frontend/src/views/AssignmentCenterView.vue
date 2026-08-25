<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppAlert from '../components/AppAlert.vue'
import ConfirmationModal from '../components/ConfirmationModal.vue'
import DataState from '../components/DataState.vue'
import PageHeader from '../components/PageHeader.vue'
import StatusBadge from '../components/StatusBadge.vue'
import WorkflowSteps from '../components/WorkflowSteps.vue'
import { assignDevice, getPersonnelAssignmentHistory, unassignDevice, type AssignmentHistory } from '../services/assignmentService'
import { getDevices, type DeviceListItem } from '../services/deviceService'
import { getPersonnelList, type PersonnelListItem } from '../services/personnelService'
import { authStore } from '../stores/authStore'

const devices = ref<DeviceListItem[]>([])
const personnel = ref<PersonnelListItem[]>([])
const history = ref<AssignmentHistory[]>([])
const selectedDeviceId = ref('')
const selectedPersonnelId = ref('')
const note = ref('')
const search = ref('')
const loading = ref(true)
const historyLoading = ref(false)
const saving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const showAssignConfirm = ref(false)
const showUnassignConfirm = ref(false)
const unassignDeviceItem = ref<DeviceListItem | null>(null)
const abortController = new AbortController()
const route = useRoute()
const showCreate = computed(() => route.name === 'assignments' || route.name === 'assignment-create')
const showActive = computed(() => route.name === 'assignments' || route.name === 'active-assignments')
const showHistory = computed(() => route.name === 'assignments' || route.name === 'assignment-history')

const stockDevices = computed(() => devices.value.filter(device => device.status === 1))
const assignedDevices = computed(() => devices.value.filter(device => device.status === 2))
const activePersonnel = computed(() => personnel.value.filter(person => person.aktifMi))
const selectedDevice = computed(() => devices.value.find(device => device.id === selectedDeviceId.value) ?? null)
const selectedPerson = computed(() => personnel.value.find(person => person.id === selectedPersonnelId.value) ?? null)
const currentStep = computed(() => showAssignConfirm.value ? 3 : selectedDeviceId.value && selectedPersonnelId.value ? 2 : 1)
const filteredAssignments = computed(() => {
  const query = search.value.toLocaleLowerCase('tr-TR').trim()
  return assignedDevices.value.filter(device => !query || [device.cihazAdi, device.seriNo, device.envanterNo, device.personelName ?? ''].join(' ').toLocaleLowerCase('tr-TR').includes(query))
})

async function loadData() {
  loading.value = true
  errorMessage.value = ''
  try { [devices.value, personnel.value] = await Promise.all([getDevices(abortController.signal), getPersonnelList(abortController.signal)]) }
  catch (error: unknown) { if (!abortController.signal.aborted) errorMessage.value = error instanceof Error ? error.message : 'Zimmet verileri yüklenemedi.' }
  finally { loading.value = false }
}

async function loadHistory(personnelId: string) {
  history.value = []
  if (!personnelId) return
  historyLoading.value = true
  try { history.value = await getPersonnelAssignmentHistory(personnelId) }
  catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Zimmet geçmişi yüklenemedi.' }
  finally { historyLoading.value = false }
}

async function confirmAssign() {
  if (!selectedDevice.value || !selectedPerson.value) return
  saving.value = true
  errorMessage.value = ''
  try {
    await assignDevice({ deviceId: selectedDevice.value.id, personnelId: selectedPerson.value.id, note: note.value.trim() || null })
    successMessage.value = `${selectedDevice.value.cihazAdi}, ${selectedPerson.value.ad} ${selectedPerson.value.soyad} personeline zimmetlendi.`
    showAssignConfirm.value = false
    selectedDeviceId.value = ''
    note.value = ''
    await loadData()
    await loadHistory(selectedPersonnelId.value)
  } catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Zimmet işlemi tamamlanamadı.' }
  finally { saving.value = false }
}

function requestUnassign(device: DeviceListItem) {
  unassignDeviceItem.value = device
  showUnassignConfirm.value = true
}

async function confirmUnassign() {
  if (!unassignDeviceItem.value) return
  saving.value = true
  errorMessage.value = ''
  try {
    await unassignDevice({ deviceId: unassignDeviceItem.value.id })
    successMessage.value = `${unassignDeviceItem.value.cihazAdi} cihazının zimmeti iade alındı.`
    showUnassignConfirm.value = false
    unassignDeviceItem.value = null
    await loadData()
    if (selectedPersonnelId.value) await loadHistory(selectedPersonnelId.value)
  } catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'İade işlemi tamamlanamadı.' }
  finally { saving.value = false }
}

function formatDate(value: string | null) {
  return value ? new Intl.DateTimeFormat('tr-TR', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)) : '—'
}

watch(selectedPersonnelId, id => { void loadHistory(id) })
onMounted(loadData)
onBeforeUnmount(() => abortController.abort())
</script>

<template>
  <section class="workflow-page assignment-page">
    <PageHeader title="Zimmet Belgesi Oluştur" description="Cihazları personele zimmetleyin, aktif zimmetleri yönetin ve işlem geçmişini inceleyin." />
    <nav class="reference-subnav" aria-label="Zimmet bölümleri">
      <RouterLink to="/zimmet"><i class="bi bi-grid"></i>Genel Bakış</RouterLink>
      <RouterLink v-if="authStore.canEdit.value" to="/zimmet/yeni"><i class="bi bi-person-plus"></i>Yeni Zimmet</RouterLink>
      <RouterLink to="/zimmet/aktif"><i class="bi bi-list-check"></i>Aktif Zimmetler</RouterLink>
      <RouterLink to="/zimmet/gecmis"><i class="bi bi-clock-history"></i>Zimmet Geçmişi</RouterLink>
    </nav>
    <AppAlert v-if="successMessage" variant="success" dismissible @dismiss="successMessage = ''">{{ successMessage }}</AppAlert>
    <AppAlert v-if="errorMessage" variant="danger" dismissible @dismiss="errorMessage = ''">{{ errorMessage }}</AppAlert>
    <DataState v-if="loading" type="loading" title="Zimmet verileri yükleniyor" />

    <template v-else>
      <div class="assignment-summary-grid">
        <article><span class="is-stock"><i class="bi bi-box-seam-fill"></i></span><div><strong>{{ stockDevices.length }}</strong><small>Zimmete hazır cihaz</small></div></article>
        <article><span class="is-assigned"><i class="bi bi-person-check-fill"></i></span><div><strong>{{ assignedDevices.length }}</strong><small>Aktif zimmet</small></div></article>
        <article><span class="is-person"><i class="bi bi-people-fill"></i></span><div><strong>{{ activePersonnel.length }}</strong><small>Aktif personel</small></div></article>
      </div>

      <section v-if="authStore.canEdit.value && showCreate" class="workflow-card">
        <WorkflowSteps :steps="['Cihaz ve personel seç', 'Bilgileri kontrol et', 'Zimmeti onayla']" :current="currentStep" />
        <div class="qr-section-heading"><span>1</span><div><h2>Yeni zimmet oluştur</h2><p>Stoktaki cihazı ve aktif personeli seçin.</p></div></div>
        <div class="assignment-picker-grid">
          <label><span>Cihaz</span><select v-model="selectedDeviceId" class="form-select"><option value="">Zimmete hazır cihaz seçin</option><option v-for="device in stockDevices" :key="device.id" :value="device.id">{{ device.cihazAdi }} · {{ device.envanterNo }} · {{ device.seriNo }}</option></select></label>
          <label><span>Personel</span><select v-model="selectedPersonnelId" class="form-select"><option value="">Aktif personel seçin</option><option v-for="person in activePersonnel" :key="person.id" :value="person.id">{{ person.ad }} {{ person.soyad }} · {{ person.sicilNo }} · {{ person.departman }}</option></select></label>
          <label class="assignment-picker-grid__note"><span>Zimmet Notu <small>(isteğe bağlı)</small></span><textarea v-model="note" class="form-control" rows="2" maxlength="500" placeholder="Teslim durumu veya açıklama..."></textarea></label>
        </div>
        <div v-if="selectedDevice && selectedPerson" class="assignment-confirm-preview"><span class="assignment-confirm-preview__device"><i class="bi bi-pc-display-horizontal"></i></span><div><small>CİHAZ</small><strong>{{ selectedDevice.cihazAdi }}</strong><span>{{ selectedDevice.envanterNo }}</span></div><i class="bi bi-arrow-right"></i><span class="assignment-confirm-preview__person">{{ selectedPerson.ad.charAt(0) }}{{ selectedPerson.soyad.charAt(0) }}</span><div><small>PERSONEL</small><strong>{{ selectedPerson.ad }} {{ selectedPerson.soyad }}</strong><span>{{ selectedPerson.sicilNo }} · {{ selectedPerson.departman }}</span></div></div>
        <div class="workflow-action-row"><span class="text-secondary small">Cihaz durumu zimmet sonrasında otomatik güncellenir.</span><button class="btn btn-success" :disabled="!selectedDeviceId || !selectedPersonnelId" @click="showAssignConfirm = true"><i class="bi bi-person-check-fill me-2"></i>Zimmetlemeyi Onayla</button></div>
      </section>

      <section v-if="showActive" class="workflow-card">
        <div class="workflow-card__header"><div><h2><i class="bi bi-list-check"></i>Aktif Zimmetler</h2><p>Personele teslim edilmiş cihazlar</p></div><label class="search-box"><i class="bi bi-search"></i><span class="visually-hidden">Aktif zimmet ara</span><input v-model="search" class="form-control" type="search" placeholder="Cihaz, personel veya seri no ara..."></label></div>
        <DataState v-if="filteredAssignments.length === 0" type="empty" title="Aktif zimmet bulunamadı" message="Zimmetlenen cihazlar burada listelenecek." />
        <div v-else class="table-responsive"><table class="table data-table align-middle mb-0"><thead><tr><th>Cihaz</th><th>Envanter / Seri</th><th>Personel</th><th>Durum</th><th class="text-end">İşlem</th></tr></thead><tbody><tr v-for="device in filteredAssignments" :key="device.id"><td><strong>{{ device.cihazAdi }}</strong><small class="d-block text-secondary">{{ device.categoryName }} · {{ device.marka }} {{ device.model }}</small></td><td><code>{{ device.envanterNo }}</code><small class="d-block text-secondary mt-1">{{ device.seriNo }}</small></td><td><span class="inventory-user-chip"><span>{{ device.personelName?.charAt(0) }}</span><strong>{{ device.personelName }}</strong></span></td><td><StatusBadge :label="device.statusName" variant="success" dot /></td><td class="text-end"><button v-if="authStore.canEdit.value" class="btn btn-outline-danger btn-sm" @click="requestUnassign(device)"><i class="bi bi-arrow-return-left me-1"></i>İade Al</button></td></tr></tbody></table></div>
      </section>

      <section v-if="showHistory" class="workflow-card">
        <div class="workflow-card__header"><div><h2><i class="bi bi-clock-history"></i>Personel Zimmet Geçmişi</h2><p>Personel seçerek aktif ve geçmiş cihaz hareketlerini görüntüleyin.</p></div><select v-model="selectedPersonnelId" class="form-select history-person-select"><option value="">Personel seçin</option><option v-for="person in personnel" :key="person.id" :value="person.id">{{ person.ad }} {{ person.soyad }} · {{ person.sicilNo }}</option></select></div>
        <DataState v-if="historyLoading" type="loading" title="Zimmet geçmişi yükleniyor" />
        <DataState v-else-if="!selectedPersonnelId" type="empty" title="Personel seçin" message="Geçmiş kayıtlarını görmek için yukarıdan bir personel seçin." icon="bi-person-check" />
        <DataState v-else-if="history.length === 0" type="empty" title="Zimmet geçmişi yok" />
        <div v-else class="table-responsive"><table class="table data-table data-table--history align-middle mb-0"><thead><tr><th>Cihaz</th><th>Seri / Envanter</th><th>Zimmet Tarihi</th><th>İade Tarihi</th><th>Durum</th></tr></thead><tbody><tr v-for="record in history" :key="record.id"><td><strong>{{ record.deviceName }}</strong><small class="d-block text-secondary">{{ record.deviceCategoryName }}</small></td><td><code>{{ record.deviceSeriNo }}</code><small class="d-block text-secondary">{{ record.deviceEnvanterNo }}</small></td><td>{{ formatDate(record.assignedAt) }}</td><td>{{ formatDate(record.returnedAt) }}</td><td><StatusBadge :label="record.isActive ? 'Aktif' : 'İade Edildi'" :variant="record.isActive ? 'success' : 'neutral'" dot /></td></tr></tbody></table></div>
      </section>
    </template>

    <ConfirmationModal :show="showAssignConfirm" title="Zimmet işlemi onaylansın mı?" message="Seçilen cihaz personele zimmetlenecek ve cihaz durumu güncellenecektir." :detail="selectedDevice && selectedPerson ? `${selectedDevice.cihazAdi} → ${selectedPerson.ad} ${selectedPerson.soyad}` : ''" :confirming="saving" :error-message="errorMessage" confirm-label="Zimmeti Oluştur" @cancel="showAssignConfirm = false" @confirm="confirmAssign" />
    <ConfirmationModal :show="showUnassignConfirm" title="Zimmet iade alınsın mı?" message="Cihaz yeniden stok durumuna alınacak ve iade tarihi geçmişe işlenecektir." :detail="unassignDeviceItem ? `${unassignDeviceItem.cihazAdi} · ${unassignDeviceItem.personelName ?? ''}` : ''" :confirming="saving" :error-message="errorMessage" confirm-label="İade Al" @cancel="showUnassignConfirm = false" @confirm="confirmUnassign" />
  </section>
</template>
