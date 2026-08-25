<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import ConfirmationModal from './ConfirmationModal.vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import {
  deleteAssignmentDocument,
  getAssignmentDocumentContent,
  getAssignmentDocuments,
  getGeneratedAssignmentDocument,
  uploadAssignmentDocument,
  type AssignmentDocument,
  type PersonnelDetail,
} from '../services/personnelService'
import { authStore } from '../stores/authStore'

const props = defineProps<{ personnel: PersonnelDetail; active: boolean }>()
const documents = ref<AssignmentDocument[]>([])
const loading = ref(false)
const errorMessage = ref('')
const uploadFile = ref<File | null>(null)
const uploadDescription = ref('')
const replacingDocument = ref<AssignmentDocument | null>(null)
const uploading = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const generatedUrl = ref('')
const generating = ref(false)
const deletingId = ref('')
const deleteCandidate = ref<AssignmentDocument | null>(null)
const deleteError = ref('')
const generatedModalRoot = ref<HTMLElement | null>(null)
const generatedOpen = computed(() => Boolean(generatedUrl.value))
useModalAccessibility(generatedOpen, generatedModalRoot, closeGenerated)

watch(() => [props.active, props.personnel.id] as const, async ([active]) => {
  if (active) await loadDocuments()
}, { immediate: true })

onBeforeUnmount(() => {
  if (generatedUrl.value) URL.revokeObjectURL(generatedUrl.value)
})

async function loadDocuments() {
  loading.value = true
  errorMessage.value = ''
  try {
    documents.value = await getAssignmentDocuments(props.personnel.id)
  } catch (error: unknown) {
    errorMessage.value = message(error, 'Belgeler yüklenemedi.')
  } finally {
    loading.value = false
  }
}

async function createDocument() {
  generating.value = true
  errorMessage.value = ''
  try {
    const blob = await getGeneratedAssignmentDocument(props.personnel.id)
    if (generatedUrl.value) URL.revokeObjectURL(generatedUrl.value)
    generatedUrl.value = URL.createObjectURL(blob)
  } catch (error: unknown) {
    errorMessage.value = message(error, 'Zimmet belgesi oluşturulamadı.')
  } finally {
    generating.value = false
  }
}

function printGenerated() {
  const frame = document.querySelector<HTMLIFrameElement>('.assignment-pdf-frame')
  frame?.contentWindow?.print()
}

function downloadGenerated() {
  if (!generatedUrl.value) return
  downloadUrl(generatedUrl.value, `zimmet-belgesi-${props.personnel.sicilNo}.pdf`)
}

function closeGenerated() {
  if (generatedUrl.value) URL.revokeObjectURL(generatedUrl.value)
  generatedUrl.value = ''
}

function selectFile(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0] ?? null
  if (!file) return
  const allowed = ['application/pdf', 'image/png', 'image/jpeg']
  if (!allowed.includes(file.type) || file.size > 10 * 1024 * 1024) {
    errorMessage.value = 'Yalnızca 10 MB veya daha küçük PDF, PNG ve JPG/JPEG dosyaları yüklenebilir.'
    input.value = ''
    return
  }
  uploadFile.value = file
}

function startReplacement(document: AssignmentDocument) {
  replacingDocument.value = document
  uploadFile.value = null
  fileInput.value?.click()
}

async function upload() {
  if (!uploadFile.value) return
  uploading.value = true
  errorMessage.value = ''
  try {
    await uploadAssignmentDocument(
      props.personnel.id,
      uploadFile.value,
      uploadDescription.value,
      replacingDocument.value?.id,
    )
    uploadFile.value = null
    uploadDescription.value = ''
    replacingDocument.value = null
    if (fileInput.value) fileInput.value.value = ''
    await loadDocuments()
  } catch (error: unknown) {
    errorMessage.value = message(error, 'İmzalı belge yüklenemedi.')
  } finally {
    uploading.value = false
  }
}

async function openStored(document: AssignmentDocument, download: boolean) {
  errorMessage.value = ''
  const viewer = download ? null : window.open('', '_blank')
  try {
    const blob = await getAssignmentDocumentContent(props.personnel.id, document.id)
    const url = URL.createObjectURL(blob)
    if (download) downloadUrl(url, document.originalFileName)
    else if (viewer) viewer.location.href = url
    window.setTimeout(() => URL.revokeObjectURL(url), 60_000)
  } catch (error: unknown) {
    viewer?.close()
    errorMessage.value = message(error, 'Belge açılamadı.')
  }
}

function requestDelete(document: AssignmentDocument) {
  deleteError.value = ''
  deleteCandidate.value = document
}

async function confirmDelete() {
  const document = deleteCandidate.value
  if (!document) return
  deletingId.value = document.id
  errorMessage.value = ''
  try {
    await deleteAssignmentDocument(props.personnel.id, document.id)
    documents.value = documents.value.filter((item) => item.id !== document.id)
    deleteCandidate.value = null
  } catch (error: unknown) {
    deleteError.value = message(error, 'Belge silinemedi.')
  } finally {
    deletingId.value = ''
  }
}

function downloadUrl(url: string, fileName: string) {
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('tr-TR', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
}

function formatSize(value: number) {
  return value < 1024 * 1024 ? `${Math.ceil(value / 1024)} KB` : `${(value / 1024 / 1024).toFixed(1)} MB`
}

function typeName(type: string) {
  return type === 'application/pdf' ? 'PDF' : type === 'image/png' ? 'PNG' : 'JPG'
}

function message(error: unknown, fallback: string) {
  return error instanceof Error ? error.message : fallback
}
</script>

<template>
  <section class="documents-section mt-4 pt-4 border-top">
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-2 mb-3">
      <div><h4 class="h6 fw-semibold mb-1">Zimmet Belgeleri</h4><p class="small text-secondary mb-0">Resmi belge oluşturun ve imzalı kopyaları saklayın.</p></div>
      <button type="button" class="btn btn-outline-primary btn-sm" :disabled="generating" @click="createDocument">
        <span v-if="generating" class="spinner-border spinner-border-sm me-1"></span>
        <i v-else class="bi bi-file-earmark-pdf me-1"></i>Zimmet Belgesi Oluştur
      </button>
    </div>

    <div v-if="errorMessage" class="alert alert-danger py-2 small">{{ errorMessage }}</div>

    <div v-if="authStore.canEdit.value" class="upload-panel mb-3">
      <div v-if="replacingDocument" class="small text-primary mb-2">
        <i class="bi bi-arrow-repeat me-1"></i>{{ replacingDocument.originalFileName }} için yeni sürüm yükleniyor.
        <button type="button" class="btn btn-link btn-sm p-0 ms-1" @click="replacingDocument = null">İptal</button>
      </div>
      <div class="row g-2 align-items-end">
        <div class="col-12 col-md-5"><label :for="`signed-document-${personnel.id}`" class="form-label small fw-semibold">İmzalı Belge</label><input :id="`signed-document-${personnel.id}`" ref="fileInput" class="form-control form-control-sm" type="file" accept=".pdf,.png,.jpg,.jpeg,application/pdf,image/png,image/jpeg" @change="selectFile"></div>
        <div class="col-12 col-md-5"><label :for="`document-description-${personnel.id}`" class="form-label small fw-semibold">Açıklama</label><input :id="`document-description-${personnel.id}`" v-model="uploadDescription" class="form-control form-control-sm" maxlength="500" placeholder="Opsiyonel açıklama"></div>
        <div class="col-12 col-md-2 d-grid"><button type="button" class="btn btn-primary btn-sm" :disabled="!uploadFile || uploading" @click="upload"><span v-if="uploading" class="spinner-border spinner-border-sm me-1"></span><i v-else class="bi bi-upload me-1"></i>Yükle</button></div>
      </div>
      <div class="form-text">PDF, PNG veya JPG/JPEG - en fazla 10 MB.</div>
    </div>

    <div v-if="loading" class="text-center text-secondary small py-3"><span class="spinner-border spinner-border-sm me-2"></span>Belgeler yükleniyor...</div>
    <div v-else-if="documents.length === 0" class="empty-documents small text-secondary">Henüz imzalı belge yüklenmemiş.</div>
    <div v-else class="table-responsive">
      <table class="table table-sm document-table align-middle mb-0">
        <thead><tr><th>Dosya</th><th>Tarih</th><th>Tür</th><th class="text-end">İşlemler</th></tr></thead>
        <tbody><tr v-for="document in documents" :key="document.id">
          <td><div class="fw-medium text-break">{{ document.originalFileName }}</div><small class="text-secondary">{{ formatSize(document.fileSize) }}<template v-if="document.description"> · {{ document.description }}</template></small></td>
          <td class="small text-secondary">{{ formatDate(document.uploadedAt) }}</td>
          <td><span class="badge text-bg-light border">{{ typeName(document.contentType) }}</span></td>
          <td class="text-end"><div class="btn-group btn-group-sm">
            <button type="button" class="btn btn-light border" title="Görüntüle" aria-label="Belgeyi görüntüle" @click="openStored(document, false)"><i class="bi bi-eye" aria-hidden="true"></i></button>
            <button type="button" class="btn btn-light border" title="İndir" aria-label="Belgeyi indir" @click="openStored(document, true)"><i class="bi bi-download" aria-hidden="true"></i></button>
            <button v-if="authStore.canEdit.value" type="button" class="btn btn-light border" title="Değiştir / Yeni sürüm yükle" aria-label="Yeni belge sürümü yükle" @click="startReplacement(document)"><i class="bi bi-arrow-repeat" aria-hidden="true"></i></button>
            <button v-if="authStore.isAdmin.value" type="button" class="btn btn-outline-danger" title="Sil" aria-label="Belgeyi sil" :disabled="deletingId === document.id" @click="requestDelete(document)"><i class="bi bi-trash3" aria-hidden="true"></i></button>
          </div></td>
        </tr></tbody>
      </table>
    </div>
  </section>

  <ConfirmationModal
    :show="Boolean(deleteCandidate)"
    title="İmzalı belge silinsin mi?"
    message="Belge listeden kaldırılacak ve artık görüntülenemeyecektir."
    :detail="deleteCandidate?.originalFileName ?? ''"
    :confirming="Boolean(deletingId)"
    :error-message="deleteError"
    confirm-label="Belgeyi Sil"
    @cancel="deleteCandidate = null; deleteError = ''"
    @confirm="confirmDelete"
  />

  <Teleport to="body">
    <div v-if="generatedUrl" ref="generatedModalRoot" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true" aria-labelledby="assignmentPdfTitle">
      <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content app-modal-content">
        <div class="modal-header"><div><h2 id="assignmentPdfTitle" class="modal-title h5 mb-0">Zimmet Belgesi</h2><small class="text-secondary">Belgeyi görüntüleyin, yazdırın veya indirin.</small></div><button type="button" class="btn-close" aria-label="Kapat" @click="closeGenerated"></button></div>
        <div class="modal-body p-0"><iframe class="assignment-pdf-frame" :src="generatedUrl" title="Zimmet belgesi önizleme"></iframe></div>
        <div class="modal-footer"><button type="button" class="btn btn-light border" @click="closeGenerated">Kapat</button><button type="button" class="btn btn-outline-primary" @click="printGenerated"><i class="bi bi-printer me-1"></i>Yazdır</button><button type="button" class="btn btn-primary" @click="downloadGenerated"><i class="bi bi-download me-1"></i>İndir</button></div>
      </div></div>
    </div><div v-if="generatedUrl" class="modal-backdrop fade show"></div>
  </Teleport>
</template>
