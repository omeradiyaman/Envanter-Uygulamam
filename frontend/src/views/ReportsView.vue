<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import PageHeader from '../components/PageHeader.vue'
import StatCard from '../components/StatCard.vue'
import {
  exportInventoryReport,
  getInventoryReport,
  type CountRow,
  type InventoryReport,
  type ReportDevice,
} from '../services/reportService'

const report = ref<InventoryReport | null>(null)
const loading = ref(true)
const exporting = ref(false)
const error = ref('')
const detailKey = ref<keyof typeof totalLabels | null>(null)
const filters = reactive({ from: '', to: '', categoryId: '', department: '', status: '' })

const statusOptions = [
  ['1', 'Stokta'],
  ['2', 'Zimmetli'],
  ['3', 'Serviste'],
  ['4', 'Hurda / İmha'],
  ['5', 'Kayıp'],
  ['6', 'Pasif'],
] as const

const totalLabels: Record<string, string> = {
  stock: 'Stok cihazlar',
  scrap: 'Hurda / İmha',
  expiring: 'Yaklaşan garanti',
  expired: 'Biten garanti',
}

function iso(value: string, end = false) {
  return value
    ? new Date(`${value}T${end ? '23:59:59.999' : '00:00:00'}`).toISOString()
    : undefined
}

function payload() {
  return {
    from: iso(filters.from),
    to: iso(filters.to, true),
    categoryId: filters.categoryId || undefined,
    department: filters.department || undefined,
    status: filters.status || undefined,
  }
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    report.value = await getInventoryReport(payload())
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Rapor yüklenemedi.'
  } finally {
    loading.value = false
  }
}

async function download() {
  exporting.value = true
  try {
    await exportInventoryReport(payload())
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Excel oluşturulamadı.'
  } finally {
    exporting.value = false
  }
}

function max(rows: CountRow[]) {
  return Math.max(1, ...rows.map((row) => row.count))
}

function fmt(value: string) {
  return new Intl.DateTimeFormat('tr-TR', {
    timeZone: 'Europe/Istanbul',
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

const totals = computed(() => ({
  stock: report.value?.stockDevices.length ?? 0,
  scrap: report.value?.scrapDevices.length ?? 0,
  expiring: report.value?.expiringWarranties.length ?? 0,
  expired: report.value?.expiredWarranties.length ?? 0,
}))
const detailRows = computed<ReportDevice[]>(() => {
  if (!report.value || !detailKey.value) return []
  return detailKey.value === 'stock' ? report.value.stockDevices
    : detailKey.value === 'scrap' ? report.value.scrapDevices
      : detailKey.value === 'expiring' ? report.value.expiringWarranties
        : report.value.expiredWarranties
})

onMounted(load)
</script>

<template>
  <section>
    <PageHeader
      title="Raporlar & Analiz"
      description="Envanter, garanti ve zimmet verilerinin güncel görünümü."
    >
      <template #actions>
        <button class="btn btn-success add-button" type="button" :disabled="exporting" @click="download">
          <i class="bi bi-file-earmark-excel me-2"></i>Excel İndir
        </button>
      </template>
    </PageHeader>

    <AppAlert v-if="error" variant="danger" dismissible @dismiss="error = ''">
      {{ error }}
    </AppAlert>

    <div class="card content-card list-card mb-4">
      <div class="card-body">
        <div class="row g-3 align-items-end">
          <div class="col-md-2">
            <label for="report-from" class="form-label">Başlangıç</label>
            <input id="report-from" v-model="filters.from" type="date" class="form-control" />
          </div>
          <div class="col-md-2">
            <label for="report-to" class="form-label">Bitiş</label>
            <input id="report-to" v-model="filters.to" type="date" class="form-control" />
          </div>
          <div class="col-md-3">
            <label for="report-category" class="form-label">Kategori</label>
            <select id="report-category" v-model="filters.categoryId" class="form-select">
              <option value="">Tümü</option>
              <option v-for="item in report?.filterOptions.categories" :key="item.key" :value="item.key">
                {{ item.label }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <label for="report-department" class="form-label">Departman</label>
            <select id="report-department" v-model="filters.department" class="form-select">
              <option value="">Tümü</option>
              <option v-for="item in report?.filterOptions.departments" :key="item" :value="item">
                {{ item }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <label for="report-status" class="form-label">Durum</label>
            <select id="report-status" v-model="filters.status" class="form-select">
              <option value="">Tümü</option>
              <option v-for="item in statusOptions" :key="item[0]" :value="item[0]">{{ item[1] }}</option>
            </select>
          </div>
          <div class="col-md-1">
            <button class="btn btn-primary w-100" type="button" title="Filtrele" aria-label="Raporu filtrele" @click="load">
              <i class="bi bi-funnel" aria-hidden="true"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <DataState v-if="loading" type="loading" title="Rapor yükleniyor" />

    <template v-else-if="report">
      <div class="row g-3 mb-4">
        <div v-for="(value, key) in totals" :key="key" class="col-6 col-lg-3">
          <button type="button" class="report-kpi-button" @click="detailKey = key"><StatCard :label="totalLabels[key]" :value="value" icon="bi-bar-chart" /></button>
        </div>
      </div>

      <div class="row g-4 mb-4">
        <div
          v-for="group in [
            { title: 'Kategoriye göre cihaz', rows: report.byCategory },
            { title: 'Duruma göre cihaz', rows: report.byStatus },
            { title: 'Departmana göre zimmet', rows: report.byDepartment },
          ]"
          :key="group.title"
          class="col-lg-4"
        >
          <div class="card content-card h-100">
            <div class="card-body">
              <h3 class="dashboard-section__title mb-3">{{ group.title }}</h3>
              <div v-if="!group.rows.length" class="text-secondary">Veri yok.</div>
              <div v-for="row in group.rows" :key="row.key" class="mb-3">
                <div class="d-flex justify-content-between small mb-1">
                  <span>{{ row.label }}</span>
                  <strong>{{ row.count }}</strong>
                </div>
                <div class="report-progress">
                  <div class="report-progress__bar" :style="{ width: `${(row.count / max(group.rows)) * 100}%` }"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="card content-card list-card mb-4">
        <div class="table-card-header">
          <h3 class="table-card-header__title mb-0">Zimmet Hareketleri</h3>
        </div>
        <div class="table-responsive">
          <table class="table data-table data-table--report align-middle mb-0">
            <thead>
              <tr>
                <th>Cihaz</th>
                <th>Envanter No</th>
                <th>Personel</th>
                <th>Departman</th>
                <th>Zimmet</th>
                <th>İade</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="!report.assignmentMovements.length">
                <td colspan="6" class="text-center text-secondary py-4">Hareket yok.</td>
              </tr>
              <tr v-for="item in report.assignmentMovements" :key="item.id">
                <td class="cell-primary">{{ item.deviceName }}</td>
                <td>{{ item.inventoryNumber }}</td>
                <td>{{ item.personnelName }}</td>
                <td>{{ item.department }}</td>
                <td>{{ fmt(item.assignedAt) }}</td>
                <td>{{ item.returnedAt ? fmt(item.returnedAt) : 'Aktif' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="row g-4">
        <div
          v-for="group in [
            { title: 'Stok Cihazlar', rows: report.stockDevices },
            { title: 'Hurda / İmha', rows: report.scrapDevices },
            { title: 'Garantisi Yaklaşanlar', rows: report.expiringWarranties },
            { title: 'Garantisi Bitenler', rows: report.expiredWarranties },
          ]"
          :key="group.title"
          class="col-xl-6"
        >
          <div class="card content-card list-card h-100">
            <div class="table-card-header">
              <h3 class="table-card-header__title mb-0">
                {{ group.title }}
                <span class="badge text-bg-light border ms-2">{{ group.rows.length }}</span>
              </h3>
            </div>
            <div class="table-responsive">
              <table class="table data-table data-table--compact align-middle mb-0">
                <thead>
                  <tr>
                    <th>Cihaz</th>
                    <th>Envanter</th>
                    <th>Kategori</th>
                    <th>Durum</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="!group.rows.length">
                    <td colspan="4" class="text-center text-secondary py-3">Kayıt yok.</td>
                  </tr>
                  <tr v-for="item in group.rows" :key="item.id">
                    <td class="cell-primary cell-truncate" :title="item.deviceName">{{ item.deviceName }}</td>
                    <td class="text-nowrap">{{ item.inventoryNumber }}</td>
                    <td class="cell-truncate" :title="item.category">{{ item.category }}</td>
                    <td class="text-nowrap">{{ item.status }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </template>

    <Teleport to="body"><div v-if="detailKey" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true"><div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable"><div class="modal-content app-modal-content"><div class="modal-header"><div><span class="modal-eyebrow">RAPOR DETAYI</span><h2 class="modal-title h5">{{ totalLabels[detailKey] }}</h2></div><button type="button" class="btn-close" aria-label="Kapat" @click="detailKey = null"></button></div><div class="modal-body p-0"><DataState v-if="!detailRows.length" type="empty" title="Kayıt bulunamadı" /><div v-else class="table-responsive"><table class="table data-table align-middle mb-0"><thead><tr><th>Cihaz</th><th>Envanter No</th><th>Seri No</th><th>Kategori</th><th>Durum</th><th class="text-end">İşlem</th></tr></thead><tbody><tr v-for="item in detailRows" :key="item.id"><td class="cell-primary">{{ item.deviceName }}</td><td>{{ item.inventoryNumber }}</td><td><code>{{ item.serialNumber }}</code></td><td>{{ item.category }}</td><td>{{ item.status }}</td><td class="text-end"><RouterLink :to="{ name: 'device-detail', params: { id: item.id } }" class="btn btn-sm btn-light border" @click="detailKey = null"><i class="bi bi-eye me-1"></i>Detay</RouterLink></td></tr></tbody></table></div></div><div class="modal-footer"><button class="btn btn-light border" type="button" @click="detailKey = null">Kapat</button></div></div></div></div><div v-if="detailKey" class="modal-backdrop fade show"></div></Teleport>
  </section>
</template>

<style scoped>
.report-progress {
  height: 0.45rem;
  border-radius: var(--radius-full);
  background: var(--surface-muted);
  overflow: hidden;
}

.report-progress__bar {
  height: 100%;
  background: linear-gradient(90deg, var(--primary), #7c3aed);
  border-radius: var(--radius-full);
  box-shadow: 0 0 12px var(--primary-glow);
}
.report-kpi-button { display: block; width: 100%; padding: 0; border: 0; background: transparent; text-align: left; }.report-kpi-button:hover .stat-card { transform: translateY(-2px); box-shadow: var(--shadow-card-hover); }
</style>
