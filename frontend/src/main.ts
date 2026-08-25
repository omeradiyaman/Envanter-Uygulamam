import { createApp } from 'vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
import { initTheme } from './composables/useTheme'
import './style.css'
import App from './App.vue'
import router from './router'
import { authStore } from './stores/authStore'

initTheme()

createApp(App).use(router).mount('#app')

window.addEventListener('auth:unauthorized', () => {
  authStore.clear()
  if (router.currentRoute.value.name !== 'login') {
    void router.replace({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
  }
})
