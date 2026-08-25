<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import AppAlert from '../components/AppAlert.vue'
import DataState from '../components/DataState.vue'
import PageHeader from '../components/PageHeader.vue'
import { useModalAccessibility } from '../composables/useModalAccessibility'
import { Roles, type Role } from '../services/authService'
import { createUser, getUsers, resetUserPassword, updateUser, type UserItem } from '../services/userService'

const users = ref<UserItem[]>([])
const loading = ref(true)
const saving = ref(false)
const error = ref('')
const formError = ref('')
const passwordError = ref('')
const success = ref('')
const showForm = ref(false)
const editing = ref<UserItem | null>(null)
const resetTarget = ref<UserItem | null>(null)
const newPassword = ref('')
const resettingPassword = ref(false)
const userModalRoot = ref<HTMLElement | null>(null)
const passwordModalRoot = ref<HTMLElement | null>(null)
const form = reactive({
  userName: '',
  fullName: '',
  password: '',
  role: Roles.Viewer as Role,
  isActive: true,
})
const roleOptions = [Roles.Admin, Roles.Editor, Roles.Viewer]

useModalAccessibility(showForm, userModalRoot, () => {
  if (!saving.value) showForm.value = false
})
useModalAccessibility(resetTarget, passwordModalRoot, () => {
  if (!resettingPassword.value) resetTarget.value = null
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    users.value = await getUsers()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Kullanıcılar yüklenemedi.'
  } finally {
    loading.value = false
  }
}

function openCreate() {
  formError.value = ''
  editing.value = null
  Object.assign(form, { userName: '', fullName: '', password: '', role: Roles.Viewer, isActive: true })
  showForm.value = true
}

function openEdit(user: UserItem) {
  formError.value = ''
  editing.value = user
  Object.assign(form, {
    userName: user.userName,
    fullName: user.fullName,
    password: '',
    role: user.role,
    isActive: user.isActive,
  })
  showForm.value = true
}

async function save() {
  saving.value = true
  formError.value = ''
  try {
    if (editing.value) await updateUser(editing.value.id, form)
    else await createUser(form)
    showForm.value = false
    success.value = 'Kullanıcı kaydedildi.'
    await load()
  } catch (e) {
    formError.value = e instanceof Error ? e.message : 'Kullanıcı kaydedilemedi.'
  } finally {
    saving.value = false
  }
}

function openPasswordReset(user: UserItem) {
  resetTarget.value = user
  newPassword.value = ''
  passwordError.value = ''
}

async function saveNewPassword() {
  if (!resetTarget.value || newPassword.value.length < 10) return
  resettingPassword.value = true
  passwordError.value = ''
  try {
    await resetUserPassword(resetTarget.value.id, newPassword.value)
    success.value = 'Şifre güvenli şekilde yenilendi.'
    resetTarget.value = null
    newPassword.value = ''
  } catch (e) {
    passwordError.value = e instanceof Error ? e.message : 'Şifre yenilenemedi.'
  } finally {
    resettingPassword.value = false
  }
}

function fmt(value: string | null) {
  return value
    ? new Intl.DateTimeFormat('tr-TR', {
        timeZone: 'Europe/Istanbul',
        dateStyle: 'short',
        timeStyle: 'short',
      }).format(new Date(value))
    : 'Henüz giriş yok'
}

onMounted(load)
</script>

<template>
  <section>
    <PageHeader title="Kullanıcı Yönetimi" description="Hesapları ve rolleri yönetin.">
      <template #actions>
        <button class="btn btn-primary add-button" type="button" @click="openCreate">
          <i class="bi bi-person-plus me-2"></i>Yeni Kullanıcı
        </button>
      </template>
    </PageHeader>

    <AppAlert v-if="error" variant="danger" dismissible @dismiss="error = ''">{{ error }}</AppAlert>
    <AppAlert v-if="success" variant="success" dismissible @dismiss="success = ''">{{ success }}</AppAlert>

    <div class="card content-card list-card">
      <DataState v-if="loading" type="loading" title="Kullanıcılar yükleniyor" />

      <DataState
        v-else-if="!users.length"
        type="empty"
        title="Kullanıcı bulunamadı"
        message="Henüz tanımlı kullanıcı hesabı yok."
        icon="bi-person-gear"
      >
        <template #actions>
          <button type="button" class="btn btn-primary" @click="openCreate">Yeni Kullanıcı Ekle</button>
        </template>
      </DataState>

      <div v-else class="table-responsive">
        <table class="table data-table data-table--users align-middle mb-0">
          <thead>
            <tr>
              <th>Kullanıcı adı</th>
              <th>Ad Soyad</th>
              <th>Rol</th>
              <th>Durum</th>
              <th>Son giriş</th>
              <th class="text-end">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.id">
              <td class="cell-primary">{{ user.userName }}</td>
              <td>{{ user.fullName }}</td>
              <td><span class="badge text-bg-light border">{{ user.role }}</span></td>
              <td>
                <span class="status-badge" :class="user.isActive ? 'active' : 'passive'">
                  {{ user.isActive ? 'Aktif' : 'Pasif' }}
                </span>
              </td>
              <td>{{ fmt(user.lastLoginAt) }}</td>
              <td class="text-end">
                <div class="action-buttons d-inline-flex gap-1">
                  <button type="button" class="btn btn-sm action-button" title="Düzenle" aria-label="Kullanıcıyı düzenle" @click="openEdit(user)">
                    <i class="bi bi-pencil" aria-hidden="true"></i>
                  </button>
                  <button type="button" class="btn btn-sm action-button" title="Şifre yenile" aria-label="Kullanıcı şifresini yenile" @click="openPasswordReset(user)">
                    <i class="bi bi-key" aria-hidden="true"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="showForm" ref="userModalRoot" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true" aria-labelledby="userFormTitle">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content app-modal-content">
            <form @submit.prevent="save">
              <div class="modal-header px-4 py-3">
                <h2 id="userFormTitle" class="modal-title h5 mb-0">{{ editing ? 'Kullanıcıyı Düzenle' : 'Yeni Kullanıcı' }}</h2>
                <button type="button" class="btn-close" aria-label="Kapat" @click="showForm = false"></button>
              </div>
              <div class="modal-body p-4">
                <AppAlert v-if="formError" variant="danger">{{ formError }}</AppAlert>
                <div class="mb-3">
                  <label for="user-name" class="form-label">Kullanıcı adı</label>
                  <input id="user-name" v-model.trim="form.userName" class="form-control" required minlength="3" autocomplete="username" />
                </div>
                <div class="mb-3">
                  <label for="user-full-name" class="form-label">Ad Soyad</label>
                  <input id="user-full-name" v-model.trim="form.fullName" class="form-control" required autocomplete="name" />
                </div>
                <div v-if="!editing" class="mb-3">
                  <label for="user-initial-password" class="form-label">Başlangıç şifresi</label>
                  <input
                    id="user-initial-password"
                    v-model="form.password"
                    type="password"
                    class="form-control"
                    required
                    minlength="10"
                    autocomplete="new-password"
                    aria-describedby="initial-password-hint"
                  />
                  <small id="initial-password-hint" class="form-text">Büyük/küçük harf, rakam ve özel karakter kullanın.</small>
                </div>
                <div class="mb-3">
                  <label for="user-role" class="form-label">Rol</label>
                  <select id="user-role" v-model="form.role" class="form-select">
                    <option v-for="role in roleOptions" :key="role" :value="role">{{ role }}</option>
                  </select>
                </div>
                <div class="form-check form-switch form-switch-panel">
                  <input id="user-active" v-model="form.isActive" class="form-check-input" type="checkbox" />
                  <label for="user-active" class="form-check-label">Aktif</label>
                </div>
              </div>
              <div class="modal-footer px-4 py-3">
                <button type="button" class="btn btn-light border" @click="showForm = false">Vazgeç</button>
                <button class="btn btn-primary" :disabled="saving">
                  {{ saving ? 'Kaydediliyor...' : 'Kaydet' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
      <div v-if="showForm" class="modal-backdrop fade show"></div>

      <div v-if="resetTarget" ref="passwordModalRoot" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true" aria-labelledby="passwordResetTitle">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content app-modal-content">
            <form @submit.prevent="saveNewPassword">
              <div class="modal-header px-4 py-3">
                <div>
                  <h2 id="passwordResetTitle" class="modal-title h5 mb-0">Şifreyi Yenile</h2>
                  <p class="small text-secondary mb-0 mt-1">{{ resetTarget.userName }} için yeni bir başlangıç şifresi belirleyin.</p>
                </div>
                <button type="button" class="btn-close" aria-label="Kapat" :disabled="resettingPassword" @click="resetTarget = null"></button>
              </div>
              <div class="modal-body p-4">
                <AppAlert v-if="passwordError" variant="danger">{{ passwordError }}</AppAlert>
                <label for="reset-password" class="form-label">Yeni şifre</label>
                <input
                  id="reset-password"
                  v-model="newPassword"
                  type="password"
                  class="form-control"
                  minlength="10"
                  autocomplete="new-password"
                  aria-describedby="reset-password-hint"
                  required
                  autofocus
                />
                <div id="reset-password-hint" class="form-text">En az 10 karakter; büyük/küçük harf, rakam ve özel karakter içermelidir.</div>
              </div>
              <div class="modal-footer px-4 py-3">
                <button type="button" class="btn btn-light border" :disabled="resettingPassword" @click="resetTarget = null">Vazgeç</button>
                <button type="submit" class="btn btn-primary" :disabled="resettingPassword || newPassword.length < 10">
                  <span v-if="resettingPassword" class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>
                  {{ resettingPassword ? 'Yenileniyor...' : 'Şifreyi Yenile' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
      <div v-if="resetTarget" class="modal-backdrop fade show"></div>
    </Teleport>
  </section>
</template>

<style scoped>
.modal-header,
.modal-footer {
  border-color: var(--border-color);
}
</style>
