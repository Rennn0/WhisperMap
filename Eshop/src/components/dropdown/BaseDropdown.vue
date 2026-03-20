<script setup lang="ts">
import {
    computed,
    nextTick,
    onMounted,
    onUnmounted,
    ref,
    watch,
    type CSSProperties,
} from 'vue';

type Align = 'left' | 'right';

const props = withDefaults(defineProps<{
    align?: Align;
    offset?: number;
    widthClass?: string;
}>(), {
    align: 'right',
    offset: 8,
    widthClass: 'w-56',
});

const open = ref(false);

const triggerRef = ref<HTMLElement | null>(null);
const panelRef = ref<HTMLElement | null>(null);

const top = ref(0);
const left = ref(0);
const minWidth = ref(0);

const panelStyle = computed<CSSProperties>(() => ({
    position: 'fixed',
    top: `${top.value}px`,
    left: `${left.value}px`,
    minWidth: `${minWidth.value}px`,
}));

function updatePosition() {
    const trigger = triggerRef.value;
    const panel = panelRef.value;

    if (!trigger) return;

    const rect = trigger.getBoundingClientRect();
    const panelWidth = panel?.offsetWidth ?? 224;

    top.value = rect.bottom + props.offset;
    minWidth.value = rect.width;

    left.value = props.align != 'left'
        ? rect.left
        : rect.right - panelWidth;

    const padding = 8;
    if (left.value < padding) left.value = padding;

    const maxLeft = window.innerWidth - panelWidth - padding;
    if (left.value > maxLeft) left.value = Math.max(padding, maxLeft);
}

async function openMenu() {
    open.value = true;
    await nextTick();
    updatePosition();
}

function closeMenu() {
    open.value = false;
}

function toggle(force?: boolean) {
    const next = force ?? !open.value;
    if (next) void openMenu();
    else closeMenu();
}

function onPointerDownDocument(e: PointerEvent) {
    if (!open.value) return;

    const target = e.target as Node | null;
    if (!target) return;

    const clickedTrigger = triggerRef.value?.contains(target);
    const clickedPanel = panelRef.value?.contains(target);

    if (!clickedTrigger && !clickedPanel) {
        closeMenu();
    }
}

function onKeyDown(e: KeyboardEvent) {
    if (e.key === 'Escape') {
        closeMenu();
        triggerRef.value?.focus();
    }
}

function onWindowChange() {
    if (open.value) updatePosition();
}

watch(open, async (value) => {
    if (value) {
        await nextTick();
        updatePosition();
    }
});

onMounted(() => {
    document.addEventListener('pointerdown', onPointerDownDocument, true);
    document.addEventListener('keydown', onKeyDown);
    window.addEventListener('resize', onWindowChange);
    window.addEventListener('scroll', onWindowChange, true);
});

onUnmounted(() => {
    document.removeEventListener('pointerdown', onPointerDownDocument, true);
    document.removeEventListener('keydown', onKeyDown);
    window.removeEventListener('resize', onWindowChange);
    window.removeEventListener('scroll', onWindowChange, true);
});

defineExpose({
    toggle,
    close: closeMenu,
    open: openMenu,
});
</script>

<template>
    <div class="relative inline-flex">
        <slot name="trigger" :open="open" :toggle="toggle" :close="closeMenu"
            :triggerRef="(el: HTMLElement | null) => (triggerRef = el)" />

        <Teleport to="body">
            <transition enter-active-class="transition duration-150 ease-out"
                enter-from-class="opacity-0 scale-95 -translate-y-1"
                enter-to-class="opacity-100 scale-100 translate-y-0"
                leave-active-class="transition duration-100 ease-in"
                leave-from-class="opacity-100 scale-100 translate-y-0"
                leave-to-class="opacity-0 scale-95 -translate-y-1">
                <div v-if="open" ref="panelRef" :style="panelStyle" :class="[
                    widthClass,
                    'z-[1000] overflow-hidden rounded-xl bg-surface text-text border border-black/10 dark:border-white/10 shadow-lg'
                ]" role="menu" aria-orientation="vertical" @click.stop>
                    <div class="p-1">
                        <slot name="content" :close="closeMenu" />
                    </div>
                </div>
            </transition>
        </Teleport>
    </div>
</template>