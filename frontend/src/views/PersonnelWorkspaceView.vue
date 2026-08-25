<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PersonnelDetailModal from '../components/PersonnelDetailModal.vue'
import PersonnelFormModal from '../components/PersonnelFormModal.vue'
import { createPersonnel, getPersonnelById, updatePersonnel, type PersonnelDetail, type PersonnelPayload } from '../services/personnelService'

const route = useRoute()
const router = useRouter()
const personnel = ref<PersonnelDetail | null>(null)
const loading = ref(true)
const saving = ref(false)
const errorMessage = ref('')
const isDetail = computed(() => ['personnel-detail', 'personnel-documents', 'personnel-history'].includes(String(route.name)))
const isEdit = computed(() => route.name === 'personnel-edit')
const id = computed(() => typeof route.params.id === 'string' ? route.params.id : '')

async function load() {
  loading.value = true
  errorMessage.value = ''
  try { if (id.value) personnel.value = await getPersonnelById(id.value) }
  catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Personel bilgileri yüklenemedi.' }
  finally { loading.value = false }
}

async function save(payload: PersonnelPayload) {
  saving.value = true
  errorMessage.value = ''
  try {
    const result = isEdit.value && id.value ? await updatePersonnel(id.value, payload) : await createPersonnel(payload)
    await router.replace({ name: 'personnel-detail', params: { id: result.id } })
  } catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Personel kaydedilemedi.' }
  finally { saving.value = false }
}

function close() { void router.push(isEdit.value && id.value ? { name: 'personnel-detail', params: { id: id.value } } : { name: 'personnel' }) }
onMounted(load)
</script>

<template>
  <section class="workspace-route-host">
    <PersonnelDetailModal v-if="isDetail" :show="true" :personnel="personnel" :loading="loading" :error-message="errorMessage" @close="close" />
    <PersonnelFormModal v-else :show="true" :personnel="isEdit ? personnel : null" :saving="saving || loading" :error-message="errorMessage" @close="close" @submit="save" />
  </section>
</template>
