<script setup lang="ts">
import { computed } from 'vue'
const props = defineProps<{
  title: string
  description?: string
  icon?: string
  eyebrow?: string
}>()
const presets: Record<string, { icon: string; eyebrow: string }> = {
  'Zimmet Belgesi Oluştur': { icon: 'bi-file-earmark-text-fill', eyebrow: 'ZİMMET YÖNETİMİ' },
  'Excel İşlemleri': { icon: 'bi-file-earmark-spreadsheet-fill', eyebrow: 'VERİ AKTARIM MERKEZİ' },
  'Toplu QR Bas': { icon: 'bi-qr-code', eyebrow: 'BASKI VE ETİKETLEME' },
  'Garanti Merkezi': { icon: 'bi-shield-check', eyebrow: 'AKILLI UYARILAR' },
  'Raporlar & Analiz': { icon: 'bi-bar-chart-fill', eyebrow: 'RAPORLAR VE ANALİZ' },
  'Son Hareketler': { icon: 'bi-clock-history', eyebrow: 'İŞLEM GEÇMİŞİ' },
  'Kullanıcı Yönetimi': { icon: 'bi-person-gear', eyebrow: 'ERİŞİM VE YETKİLER' },
}
const preset = computed(() => presets[props.title] ?? { icon: 'bi-grid-1x2-fill', eyebrow: 'ENVANTER YÖNETİMİ' })
</script>

<template>
  <header class="reference-page-header">
    <span class="reference-page-header__icon"><i :class="['bi', icon ?? preset.icon]"></i></span>
    <div class="reference-page-header__main">
      <span class="reference-page-header__eyebrow">{{ eyebrow ?? preset.eyebrow }}</span>
      <h1>{{ title }}</h1>
      <p v-if="description">{{ description }}</p>
    </div>
    <div v-if="$slots.actions" class="reference-page-header__actions">
      <slot name="actions" />
    </div>
  </header>
</template>

<style scoped>
.reference-page-header { position: relative; display: flex; align-items: center; gap: 1.1rem; min-height: 132px; margin-bottom: 1.25rem; padding: 1.25rem 1.4rem; overflow: hidden; border: 1px solid color-mix(in srgb, var(--primary) 20%, var(--border-color)); border-radius: 18px; background: linear-gradient(120deg, color-mix(in srgb, var(--primary) 8%, var(--surface)), var(--surface) 66%); box-shadow: var(--shadow-card); }.reference-page-header::after { position: absolute; right: -75px; bottom: -150px; width: 280px; height: 280px; border: 1px solid color-mix(in srgb, var(--primary) 15%, transparent); border-radius: 50%; content: ''; }.reference-page-header__icon { position: relative; z-index: 1; display: grid; width: 60px; height: 60px; flex: 0 0 60px; place-items: center; border-radius: 16px; color: var(--primary); background: var(--primary-subtle); font-size: 1.35rem; }.reference-page-header__main { position: relative; z-index: 1; min-width: 0; flex: 1; }.reference-page-header__eyebrow { color: var(--primary); font-size: .65rem; font-weight: 800; letter-spacing: .11em; }.reference-page-header h1 { margin: .25rem 0 .2rem; color: var(--text-heading); font-size: 1.55rem; font-weight: 780; letter-spacing: -.035em; }.reference-page-header p { max-width: 70ch; margin: 0; color: var(--text-muted); font-size: .78rem; }.reference-page-header__actions { position: relative; z-index: 1; display: flex; flex-wrap: wrap; gap: .5rem; margin-left: auto; }
@media (max-width: 767.98px) { .reference-page-header { align-items: flex-start; flex-wrap: wrap; padding: 1.1rem; }.reference-page-header__icon { width: 50px; height: 50px; flex-basis: 50px; }.reference-page-header__actions { width: 100%; margin-left: 0; }.reference-page-header__actions :deep(.btn) { flex: 1; } }
</style>
