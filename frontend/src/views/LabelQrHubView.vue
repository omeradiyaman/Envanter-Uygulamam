<script setup lang="ts">
const options = [
  { to: '/toplu-etiket', icon: 'bi-tags-fill', eyebrow: 'CİHAZ ETİKETİ', title: 'Toplu Etiket Bas', description: 'Envanterden cihazları seçin; cihaz adı, envanter no, seri no ve QR içeren yazıcı dostu etiketler hazırlayın.', tone: 'label' },
  { to: '/toplu-qr', icon: 'bi-qr-code', eyebrow: 'QR MERKEZİ', title: 'Toplu QR Bas', description: 'Kategori ve arama ile cihaz seçin; cihaz detay sayfalarına yönlendiren QR kodlarını toplu önizleyip yazdırın.', tone: 'qr' },
]
</script>

<template>
  <section class="label-hub-page">
    <header class="inventory-page-hero">
      <span class="inventory-page-hero__visual"><i class="bi bi-upc-scan"></i></span>
      <div class="inventory-page-hero__main"><span class="inventory-page-hero__eyebrow">BASKI VE ETİKETLEME</span><h1 class="inventory-page-hero__title">Etiket / QR İşlemleri</h1><p class="inventory-page-hero__description">İhtiyacınıza göre etiket veya QR akışını seçin.</p></div>
    </header>
    <div class="label-hub-grid">
      <RouterLink v-for="option in options" :key="option.to" :to="option.to" class="label-hub-card" :class="`is-${option.tone}`">
        <span class="label-hub-card__icon"><i :class="['bi', option.icon]"></i></span>
        <span class="label-hub-card__eyebrow">{{ option.eyebrow }}</span>
        <h2>{{ option.title }}</h2><p>{{ option.description }}</p>
        <span class="label-hub-card__action">İşlemi başlat <i class="bi bi-arrow-right"></i></span>
      </RouterLink>
    </div>
  </section>
</template>
