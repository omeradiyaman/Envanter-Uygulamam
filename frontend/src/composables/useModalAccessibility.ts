import { nextTick, onBeforeUnmount, watch, type Ref } from 'vue'

type ModalEntry = {
  id: symbol
  root: Ref<HTMLElement | null>
  close: () => void
  previousFocus: HTMLElement | null
}

const modalStack: ModalEntry[] = []

const focusableSelector = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled]):not([type="hidden"])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',')

function visibleFocusableElements(root: HTMLElement) {
  return Array.from(root.querySelectorAll<HTMLElement>(focusableSelector))
    .filter((element) => element.offsetParent !== null && element.getAttribute('aria-hidden') !== 'true')
}

function handleKeydown(event: KeyboardEvent) {
  const activeModal = modalStack.at(-1)
  const root = activeModal?.root.value
  if (!activeModal || !root) return

  if (event.key === 'Escape') {
    event.preventDefault()
    activeModal.close()
    return
  }

  if (event.key !== 'Tab') return
  const elements = visibleFocusableElements(root)
  if (elements.length === 0) {
    event.preventDefault()
    root.focus()
    return
  }

  const first = elements[0]
  const last = elements[elements.length - 1]
  if (event.shiftKey && document.activeElement === first) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && document.activeElement === last) {
    event.preventDefault()
    first.focus()
  }
}

function syncGlobalModalState() {
  document.body.classList.toggle('modal-open', modalStack.length > 0)
  document.removeEventListener('keydown', handleKeydown)
  if (modalStack.length > 0) document.addEventListener('keydown', handleKeydown)
}

export function useModalAccessibility(
  show: Ref<unknown>,
  root: Ref<HTMLElement | null>,
  close: () => void,
) {
  const id = Symbol('modal')

  function deactivate() {
    const index = modalStack.findIndex((entry) => entry.id === id)
    if (index === -1) return
    const [entry] = modalStack.splice(index, 1)
    syncGlobalModalState()
    if (entry.previousFocus?.isConnected) entry.previousFocus.focus()
  }

  watch(show, async (isOpen) => {
    if (!isOpen) {
      deactivate()
      return
    }

    modalStack.push({
      id,
      root,
      close,
      previousFocus: document.activeElement instanceof HTMLElement ? document.activeElement : null,
    })
    syncGlobalModalState()
    await nextTick()
    const autofocus = root.value?.querySelector<HTMLElement>('[autofocus]')
    const firstFocusable = root.value ? visibleFocusableElements(root.value)[0] : null
    ;(autofocus ?? firstFocusable ?? root.value)?.focus()
  }, { flush: 'post' })

  onBeforeUnmount(deactivate)
}
