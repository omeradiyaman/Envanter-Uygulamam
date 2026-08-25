const visuals = [
  { terms: ['laptop', 'notebook'], icon: 'bi-laptop-fill', tone: 'indigo' },
  { terms: ['monitör', 'monitor', 'ekran'], icon: 'bi-display-fill', tone: 'sky' },
  { terms: ['masaüstü', 'desktop', 'bilgisayar'], icon: 'bi-pc-display-horizontal', tone: 'violet' },
  { terms: ['yazıcı', 'printer'], icon: 'bi-printer-fill', tone: 'emerald' },
  { terms: ['tablet'], icon: 'bi-tablet-fill', tone: 'amber' },
  { terms: ['telefon', 'phone', 'mobil'], icon: 'bi-phone-fill', tone: 'rose' },
  { terms: ['zbox', 'mini pc'], icon: 'bi-device-hdd-fill', tone: 'sky' },
  { terms: ['switch', 'network', 'ağ'], icon: 'bi-router-fill', tone: 'indigo' },
  { terms: ['sunucu', 'server'], icon: 'bi-server', tone: 'violet' },
] as const
const fallbackIcons = ['bi-box-seam-fill', 'bi-cpu-fill', 'bi-hdd-stack-fill', 'bi-usb-drive-fill', 'bi-plug-fill']
const fallbackTones = ['indigo', 'sky', 'violet', 'emerald', 'amber', 'rose']

export function inventoryVisual(name: string, index = 0) {
  const normalized = name.toLocaleLowerCase('tr-TR')
  const match = visuals.find(item => item.terms.some(term => normalized.includes(term)))
  return match ?? { icon: fallbackIcons[index % fallbackIcons.length]!, tone: fallbackTones[index % fallbackTones.length]! }
}
