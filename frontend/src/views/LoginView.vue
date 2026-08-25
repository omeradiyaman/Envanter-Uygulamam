<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ThemeToggle from '../components/ThemeToggle.vue'
import { authStore } from '../stores/authStore'

const router = useRouter()
const route = useRoute()
const userName = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')
const showPassword = ref(false)

async function submit() {
  loading.value = true
  error.value = ''
  try {
    await authStore.login(userName.value, password.value)
    const redirect =
      typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/')
        ? route.query.redirect
        : '/'
    await router.replace(redirect)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Giriş yapılamadı.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="login-page d-flex flex-column align-items-center justify-content-center p-3">
    <div class="login-bg-grid" aria-hidden="true"></div>
    <div class="login-toolbar">
      <ThemeToggle />
    </div>
    <div class="login-card card">
      <div class="card-body p-4 p-md-5">
        <div class="brand-icon mx-auto mb-3">
          <img src="/favicon.svg" alt="IT Envanter">
        </div>
        <h1 class="h4 text-center fw-bold mb-1">IT Envanter</h1>
        <p class="text-secondary text-center mb-4">Yönetim Sistemi — Güvenli Giriş</p>
        <div v-if="error" class="app-alert danger mb-3" role="alert">
          <div class="app-alert__content">{{ error }}</div>
        </div>
        <form @submit.prevent="submit">
          <div class="mb-3">
            <label class="form-label" for="login-username">Kullanıcı adı</label>
            <div class="login-input-wrap">
              <i class="bi bi-person-fill field-icon" aria-hidden="true"></i>
              <input
                id="login-username"
                v-model.trim="userName"
                class="form-control"
                autocomplete="username"
                placeholder="Kullanıcı adınız"
                required
                autofocus
              />
            </div>
          </div>
          <div class="mb-4">
            <label class="form-label" for="login-password">Şifre</label>
            <div class="login-input-wrap">
              <i class="bi bi-lock-fill field-icon" aria-hidden="true"></i>
              <input
                id="login-password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                class="form-control"
                autocomplete="current-password"
                placeholder="••••••••"
                required
              />
              <button
                type="button"
                class="password-toggle"
                :aria-label="showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'"
                @click="showPassword = !showPassword"
              >
                <i :class="['bi', showPassword ? 'bi-eye-slash-fill' : 'bi-eye-fill']" aria-hidden="true"></i>
              </button>
            </div>
          </div>
          <button class="btn btn-primary w-100 add-button" type="submit" :disabled="loading">
            <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
            Giriş Yap
          </button>
        </form>
        <div class="login-footer text-center mt-4 pt-3">
          <small><i class="bi bi-shield-lock-fill me-1" aria-hidden="true"></i>Yetkili personel erişimi</small>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
.login-toolbar {
  position: fixed;
  z-index: 2;
  top: 1.5rem;
  right: 1.5rem;
}

.login-bg-grid {
  position: fixed;
  inset: 0;
  pointer-events: none;
  opacity: 0.32;
  background-image:
    linear-gradient(var(--border-color-subtle) 1px, transparent 1px),
    linear-gradient(90deg, var(--border-color-subtle) 1px, transparent 1px);
  background-size: 42px 42px;
}

.login-card {
  position: relative;
  z-index: 1;
  width: min(100%, 430px);
  overflow: hidden;
  border-color: color-mix(in srgb, var(--primary) 18%, var(--border-color));
  border-radius: 20px;
  box-shadow: 0 28px 80px rgb(15 23 42 / 16%);
}

.brand-icon{display:grid;width:64px;height:64px;overflow:hidden;place-items:center;border-radius:18px;box-shadow:0 12px 30px var(--primary-glow)}
.brand-icon img{width:100%;height:100%}

.login-input-wrap {
  position: relative;
}

.login-input-wrap .field-icon {
  position: absolute;
  z-index: 2;
  top: 50%;
  left: 0.95rem;
  color: var(--text-muted);
  transform: translateY(-50%);
  transition: color 0.18s ease;
}

.login-input-wrap .form-control {
  padding-left: 2.75rem;
}

.login-input-wrap .form-control:focus ~ .field-icon,
.login-input-wrap:focus-within .field-icon {
  color: var(--primary);
}

.password-toggle {
  position: absolute;
  z-index: 2;
  top: 50%;
  right: 0.55rem;
  width: 34px;
  height: 34px;
  padding: 0;
  border: 0;
  border-radius: 0.5rem;
  color: var(--text-muted);
  background: transparent;
  transform: translateY(-50%);
}

.password-toggle:hover {
  color: var(--primary);
  background: var(--primary-subtle);
}

.login-input-wrap input[type='password'],
.login-input-wrap input[type='text']#login-password {
  padding-right: 3rem;
}

.login-footer {
  border-top: 1px solid var(--border-color-subtle);
  color: var(--text-muted);
}
</style>
