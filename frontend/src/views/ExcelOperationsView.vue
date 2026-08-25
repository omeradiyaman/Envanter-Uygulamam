<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import PageHeader from '../components/PageHeader.vue'
import StatusBadge from '../components/StatusBadge.vue'
import WorkflowSteps from '../components/WorkflowSteps.vue'
import { authStore } from '../stores/authStore'
import {
  executeExcelImport,
  exportDevicesExcel,
  exportInventoryExcel,
  exportPersonnelExcel,
  previewExcelImport,
  type ImportExecutionResult,
  type ImportPreviewRow,
  type ImportPreviewSummary,
  type ImportTarget,
} from '../services/excelService'

const target = ref<ImportTarget>('devices')
const file = ref<File | null>(null)
const preview = ref<ImportPreviewSummary | null>(null)
const result = ref<ImportExecutionResult | null>(null)
const analyzing = ref(false)
const importing = ref(false)
const exporting = ref<string | null>(null)
const errorMessage = ref('')
const dragActive = ref(false)
const route = useRoute()
const showExport = computed(() => route.name !== 'excel-import')
const showImport = computed(() => route.name !== 'excel-export')

const currentStep = computed(() => result.value ? 4 : preview.value ? 3 : file.value ? 2 : 1)
const visibleRows = computed(() => result.value?.rows ?? preview.value?.rows ?? [])
const canImport = computed(() => Boolean(preview.value && file.value && preview.value.errorCount === 0))

function resetImport() {
  file.value = null
  preview.value = null
  result.value = null
  errorMessage.value = ''
}

function selectFile(selected: File | null) {
  resetImport()
  if (!selected) return
  const extension = selected.name.split('.').pop()?.toLocaleLowerCase('tr-TR')
  if (!['xlsx', 'xls'].includes(extension ?? '')) {
    errorMessage.value = 'Yalnızca .xlsx veya .xls dosyaları desteklenir.'
    return
  }
  if (selected.size > 6 * 1024 * 1024) {
    errorMessage.value = 'Dosya boyutu 6 MB sınırını aşamaz.'
    return
  }
  file.value = selected
}

function onFileInput(event: Event) {
  selectFile((event.target as HTMLInputElement).files?.[0] ?? null)
}

function onDrop(event: DragEvent) {
  dragActive.value = false
  selectFile(event.dataTransfer?.files?.[0] ?? null)
}

async function analyze() {
  if (!file.value) return
  analyzing.value = true
  errorMessage.value = ''
  result.value = null
  try {
    preview.value = await previewExcelImport(target.value, file.value)
  } catch (error: unknown) {
    errorMessage.value = error instanceof Error ? error.message : 'Excel dosyası analiz edilemedi.'
  } finally {
    analyzing.value = false
  }
}

async function executeImport() {
  if (!file.value || !preview.value) return
  importing.value = true
  errorMessage.value = ''
  try {
    result.value = await executeExcelImport(target.value, file.value)
  } catch (error: unknown) {
    errorMessage.value = error instanceof Error ? error.message : 'Excel import işlemi tamamlanamadı.'
  } finally {
    importing.value = false
  }
}

async function runExport(key: string, action: () => Promise<void>) {
  exporting.value = key
  errorMessage.value = ''
  try { await action() }
  catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Excel dosyası indirilemedi.' }
  finally { exporting.value = null }
}

function actionVariant(action: string): 'success' | 'warning' | 'danger' | 'neutral' {
  const value = action.toLocaleLowerCase('tr-TR')
  if (value.includes('error') || value.includes('hata')) return 'danger'
  if (value.includes('new') || value.includes('yeni') || value.includes('add')) return 'success'
  if (value.includes('update') || value.includes('güncel')) return 'warning'
  return 'neutral'
}

function actionLabel(row: ImportPreviewRow) {
  const labels: Record<string, string> = { New: 'Yeni', Update: 'Güncellenecek', Unchanged: 'Değişmeyecek', Error: 'Hatalı' }
  return labels[row.action] ?? row.action
}
</script>

<template>
  <section class="workflow-page excel-page">
    <PageHeader title="Excel İşlemleri" description="Verileri güvenli biçimde dışa aktarın veya önizleme ve onay adımlarıyla içe alın." />

    <nav class="reference-subnav" aria-label="Excel bölümleri">
      <RouterLink to="/excel-islemleri"><i class="bi bi-grid"></i>İşlem Merkezi</RouterLink>
      <RouterLink to="/excel-islemleri/export"><i class="bi bi-download"></i>Excel Export</RouterLink>
      <RouterLink v-if="authStore.canEdit.value" to="/excel-islemleri/import"><i class="bi bi-upload"></i>Excel Import</RouterLink>
    </nav>

    <AppAlert v-if="errorMessage" variant="danger" dismissible @dismiss="errorMessage = ''">{{ errorMessage }}</AppAlert>

    <div class="operation-card-grid mb-4">
      <article v-if="showExport" class="operation-card is-export">
        <div class="operation-card__top"><span><i class="bi bi-file-earmark-arrow-down-fill"></i></span><em>GÜVENLİ İNDİRME</em></div>
        <h2>Excel Export</h2>
        <p>Güncel cihaz, personel veya tüm envanter verisini Excel olarak indirin.</p>
        <div class="operation-card__actions">
          <button class="btn btn-success btn-sm" :disabled="Boolean(exporting)" @click="runExport('inventory', exportInventoryExcel)"><i class="bi bi-grid-3x3-gap-fill me-1"></i>Tüm Envanter</button>
          <button class="btn btn-outline-success btn-sm" :disabled="Boolean(exporting)" @click="runExport('devices', exportDevicesExcel)">Cihazlar</button>
          <button class="btn btn-outline-success btn-sm" :disabled="Boolean(exporting)" @click="runExport('personnel', exportPersonnelExcel)">Personeller</button>
        </div>
      </article>

      <article v-if="showImport" class="operation-card is-import" :class="{ 'is-locked': !authStore.canEdit.value }">
        <div class="operation-card__top"><span><i :class="['bi', authStore.canEdit.value ? 'bi-file-earmark-arrow-up-fill' : 'bi-lock-fill']"></i></span><em>{{ authStore.canEdit.value ? 'KONTROLLÜ GÜNCELLEME' : 'YETKİ GEREKLİ' }}</em></div>
        <h2>Excel Import</h2>
        <p>{{ authStore.canEdit.value ? 'Dosyayı önce analiz edin, değişiklikleri görün ve ardından sisteme uygulayın.' : 'Import yalnızca Editör veya Admin kullanıcıları tarafından yapılabilir.' }}</p>
        <span class="operation-card__hint"><i class="bi bi-shield-check me-1"></i>Önizleme olmadan veri yazılmaz</span>
      </article>
    </div>

    <section v-if="authStore.canEdit.value && showImport" class="workflow-card">
      <WorkflowSteps :steps="['Dosya seç', 'Analiz et', 'Onayla', 'Sonuç']" :current="currentStep" />

      <div class="import-target-tabs" role="tablist" aria-label="Import veri türü">
        <button type="button" :class="{ 'is-active': target === 'devices' }" @click="target = 'devices'; resetImport()"><i class="bi bi-pc-display-horizontal"></i><span><strong>Cihaz Import</strong><small>Seri ve envanter numaralarıyla eşleşir</small></span></button>
        <button type="button" :class="{ 'is-active': target === 'personnel' }" @click="target = 'personnel'; resetImport()"><i class="bi bi-people-fill"></i><span><strong>Personel Import</strong><small>Sicil numarasıyla eşleşir</small></span></button>
      </div>

      <label class="excel-drop-zone" :class="{ 'is-active': dragActive, 'has-file': file }" @dragenter.prevent="dragActive = true" @dragover.prevent @dragleave.prevent="dragActive = false" @drop.prevent="onDrop">
        <input type="file" accept=".xlsx,.xls" @change="onFileInput" />
        <span class="excel-drop-zone__icon"><i class="bi bi-cloud-arrow-up-fill"></i></span>
        <strong>{{ file?.name ?? 'Excel dosyasını buraya bırakın' }}</strong>
        <small>{{ file ? `${(file.size / 1024).toFixed(1)} KB · Değiştirmek için tıklayın` : '.xlsx veya .xls · En fazla 6 MB' }}</small>
      </label>

      <div v-if="file && !preview" class="workflow-action-row">
        <button type="button" class="btn btn-light border" @click="resetImport">Dosyayı Kaldır</button>
        <button type="button" class="btn btn-primary" :disabled="analyzing" @click="analyze"><span v-if="analyzing" class="spinner-border spinner-border-sm me-2"></span><i v-else class="bi bi-search me-2"></i>{{ analyzing ? 'Analiz Ediliyor...' : 'Dosyayı Analiz Et' }}</button>
      </div>

      <template v-if="preview">
        <div class="import-summary-grid">
          <article class="is-new"><span>Yeni</span><strong>{{ result?.addedCount ?? preview.newCount }}</strong></article>
          <article class="is-update"><span>Güncellenecek</span><strong>{{ result?.updatedCount ?? preview.updatedCount }}</strong></article>
          <article class="is-same"><span>Değişmeyecek</span><strong>{{ result?.unchangedCount ?? preview.unchangedCount }}</strong></article>
          <article class="is-error"><span>Hatalı</span><strong>{{ result?.errorCount ?? preview.errorCount }}</strong></article>
        </div>

        <div class="import-preview-card">
          <header><div><h3><i class="bi bi-eye-fill"></i>Değişiklik Önizlemesi</h3><p>{{ visibleRows.length }} satır analiz edildi</p></div><StatusBadge v-if="result" label="Import tamamlandı" variant="success" dot /></header>
          <DataState v-if="visibleRows.length === 0" type="empty" title="İşlenecek satır bulunamadı" />
          <div v-else class="table-responsive">
            <table class="table data-table align-middle mb-0">
              <thead><tr><th>Satır</th><th>İşlem</th><th>Tanımlayıcı</th><th>Özet</th><th>Mesajlar</th></tr></thead>
              <tbody><tr v-for="row in visibleRows" :key="`${row.rowNumber}-${row.identifier}`"><td>{{ row.rowNumber }}</td><td><StatusBadge :label="actionLabel(row)" :variant="actionVariant(row.action)" /></td><td><code>{{ row.identifier }}</code></td><td>{{ row.summary }}</td><td><span v-if="row.messages.length" class="text-danger small">{{ row.messages.join(' · ') }}</span><span v-else class="text-secondary">—</span></td></tr></tbody>
            </table>
          </div>
        </div>

        <div class="workflow-action-row">
          <button type="button" class="btn btn-light border" :disabled="importing" @click="resetImport">Yeni Dosya Seç</button>
          <button v-if="!result" type="button" class="btn btn-success" :disabled="!canImport || importing" @click="executeImport"><span v-if="importing" class="spinner-border spinner-border-sm me-2"></span><i v-else class="bi bi-check2-circle me-2"></i>{{ importing ? 'İçe Aktarılıyor...' : 'Onayla ve İçe Aktar' }}</button>
        </div>
        <p v-if="preview.errorCount > 0 && !result" class="import-block-note"><i class="bi bi-exclamation-triangle-fill"></i>Hatalı satırlar düzeltilmeden import başlatılamaz.</p>
      </template>
    </section>
  </section>
</template>
