<script setup lang="ts">
import { computed, ref, toRef, watch } from 'vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import { getDeviceQrCode, type DeviceQrCode } from '../services/deviceService'

export interface LabelDevice {
  id: string
  cihazAdi: string
  envanterNo: string
  seriNo: string
}

const props = defineProps<{
  show: boolean
  devices: LabelDevice[]
}>()

const emit = defineEmits<{ close: [] }>()
const modalRoot = ref<HTMLElement | null>(null)
useModalAccessibility(toRef(props, 'show'), modalRoot, () => emit('close'))
const qrCodes = ref<Record<string, DeviceQrCode>>({})
const loading = ref(false)
const errorMessage = ref('')

const labels = computed(() => props.devices.map((device) => ({
  device,
  qr: qrCodes.value[device.id],
})))

watch(() => props.show, async (show) => {
  if (!show) return
  qrCodes.value = {}
  errorMessage.value = ''
  loading.value = true
  try {
    const results = await Promise.all(props.devices.map(async (device) => [
      device.id,
      await getDeviceQrCode(device.id),
    ] as const))
    qrCodes.value = Object.fromEntries(results)
  } catch (error: unknown) {
    errorMessage.value = error instanceof Error ? error.message : 'QR kodları oluşturulamadı.'
  } finally {
    loading.value = false
  }
})

function printLabels() {
  window.print()
}
</script>

<template>
  <Teleport to="body">
    <div v-if="show" class="device-label-print-root">
      <div ref="modalRoot" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true" aria-labelledby="deviceLabelTitle">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content app-modal-content">
            <div class="modal-header px-4 py-3 no-print">
              <div>
                <h2 id="deviceLabelTitle" class="modal-title h5 fw-semibold mb-0">Etiket Önizleme</h2>
                <small class="text-secondary">{{ devices.length }} cihaz için yazdırılabilir etiket</small>
              </div>
              <button type="button" class="btn-close" aria-label="Kapat" @click="emit('close')"></button>
            </div>
            <div class="modal-body p-4">
              <div v-if="loading" class="text-center py-5 text-secondary">
                <div class="spinner-border text-primary mb-3"></div>
                <div>QR kodları hazırlanıyor...</div>
              </div>
              <div v-else-if="errorMessage" class="alert alert-danger">{{ errorMessage }}</div>
              <div v-else class="label-sheet">
                <article v-for="label in labels" :key="label.device.id" class="device-label">
                  <div class="label-details">
                    <div class="label-title">{{ label.device.cihazAdi }}</div>
                    <dl>
                      <div><dt>Envanter No</dt><dd>{{ label.device.envanterNo }}</dd></div>
                      <div><dt>Seri No</dt><dd>{{ label.device.seriNo }}</dd></div>
                    </dl>
                  </div>
                  <img
                    v-if="label.qr"
                    class="label-qr"
                    :src="`data:image/png;base64,${label.qr.pngBase64}`"
                    :alt="`${label.device.cihazAdi} QR kodu`"
                  >
                </article>
              </div>
            </div>
            <div class="modal-footer px-4 py-3 no-print">
              <button type="button" class="btn btn-light border" @click="emit('close')">Kapat</button>
              <button type="button" class="btn btn-primary" :disabled="loading || !!errorMessage" @click="printLabels">
                <i class="bi bi-printer me-2"></i>Yazdır
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-backdrop fade show no-print"></div>
    </div>
  </Teleport>
</template>

<style>
:root { --device-label-width: 90mm; --device-label-height: 45mm; --device-label-gap: 5mm; }
.label-sheet { display: grid; grid-template-columns: repeat(auto-fit, minmax(290px, 1fr)); gap: 1rem; }
.device-label { width: var(--device-label-width); min-height: var(--device-label-height); max-width: 100%; padding: 7mm; border: 1px solid #1f2937; display: flex; align-items: center; justify-content: space-between; gap: 5mm; background: #fff; color: #111827; break-inside: avoid; }
.label-details { min-width: 0; flex: 1; }.label-title { margin-bottom: 4mm; font-size: 14pt; font-weight: 700; line-height: 1.2; }.label-details dl { margin: 0; }.label-details dl > div { margin-top: 2mm; }.label-details dt { font-size: 7pt; text-transform: uppercase; color: #4b5563; }.label-details dd { margin: 0; font-family: ui-monospace, monospace; font-size: 9pt; font-weight: 700; word-break: break-word; }.label-qr { width: 30mm; height: 30mm; object-fit: contain; image-rendering: pixelated; }
@media print { @page { margin: 8mm; } body * { visibility: hidden !important; } .device-label-print-root, .device-label-print-root * { visibility: visible !important; } .device-label-print-root { position: absolute; inset: 0; } .device-label-print-root .modal { position: static; display: block !important; } .device-label-print-root .modal-dialog, .device-label-print-root .modal-content { width: auto; max-width: none; margin: 0; border: 0 !important; box-shadow: none !important; } .device-label-print-root .modal-body { padding: 0 !important; } .device-label-print-root .label-sheet { grid-template-columns: repeat(2, var(--device-label-width)); gap: var(--device-label-gap); } .no-print { display: none !important; } }
</style>
