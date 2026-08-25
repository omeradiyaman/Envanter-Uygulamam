<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import DeviceDetailModal from '../components/DeviceDetailModal.vue'
import DeviceFormModal from '../components/DeviceFormModal.vue'
import { createDevice, getDevice, getDeviceCategories, updateDevice, type Device, type DeviceCategory, type DevicePayload } from '../services/deviceService'

const route = useRoute()
const router = useRouter()
const device = ref<Device | null>(null)
const categories = ref<DeviceCategory[]>([])
const loading = ref(true)
const saving = ref(false)
const errorMessage = ref('')
const isDetail = computed(() => route.name === 'device-detail' || route.name === 'device-history' || route.name === 'device-qr-detail')
const isEdit = computed(() => route.name === 'device-edit')
const id = computed(() => typeof route.params.id === 'string' ? route.params.id : '')

async function load() {
  loading.value = true
  errorMessage.value = ''
  try {
    categories.value = await getDeviceCategories()
    if (id.value) device.value = await getDevice(id.value)
  } catch (error: unknown) {
    errorMessage.value = error instanceof Error ? error.message : 'Cihaz bilgileri yüklenemedi.'
  } finally { loading.value = false }
}

async function save(payload: DevicePayload) {
  saving.value = true
  errorMessage.value = ''
  try {
    if (isEdit.value && id.value) {
      await updateDevice(id.value, payload)
      device.value = await getDevice(id.value)
      await router.replace({ name: 'device-detail', params: { id: id.value } })
    } else {
      const createdId = await createDevice(payload)
      device.value = await getDevice(createdId)
      await router.replace({ name: 'device-detail', params: { id: createdId } })
    }
  } catch (error: unknown) {
    errorMessage.value = error instanceof Error ? error.message : 'Cihaz kaydedilemedi.'
  } finally { saving.value = false }
}

async function refresh() {
  if (!id.value) return
  try { device.value = await getDevice(id.value) }
  catch (error: unknown) { errorMessage.value = error instanceof Error ? error.message : 'Cihaz yenilenemedi.' }
}

function close() { void router.push(isEdit.value && id.value ? { name: 'device-detail', params: { id: id.value } } : { name: 'inventories' }) }
onMounted(load)
</script>

<template>
  <section class="workspace-route-host" aria-live="polite">
    <DeviceDetailModal v-if="isDetail" :show="true" :device="device" :loading="loading" :error-message="errorMessage" @close="close" @refresh="refresh" />
    <DeviceFormModal v-else :show="true" :device="isEdit ? device : null" :categories="categories" :saving="saving || loading" :error-message="errorMessage" @close="close" @submit="save" />
  </section>
</template>
