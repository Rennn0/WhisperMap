<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

const _ = defineProps<{
    align?: 'left' | 'right';
}>(); _;

const open = ref(false);
const rootRef = ref<HTMLElement | null>(null);

const toggle = (val: boolean | null = null) => {
    open.value = val ?? !open.value;
};

const close = () => open.value = false;

const onClickOutside = (e: MouseEvent) => {
    if (!rootRef.value) return;
    if (!rootRef.value.contains(e.target as Node)) {
        close();
    }
};

const onEscape = (e: KeyboardEvent) => {
    if (e.key === 'Escape') close();
};

onMounted(() => {
    document.addEventListener('click', onClickOutside);
    document.addEventListener('keydown', onEscape);
});

onUnmounted(() => {
    document.removeEventListener('click', onClickOutside);
    document.removeEventListener('keydown', onEscape);
});

defineExpose({ toggle, close });
</script>

<template>
    <div ref="rootRef" class="relative inline-block">
        <slot name="trigger" :toggle="toggle" />

        <transition enter-active-class="transition duration-150 ease-out" enter-from-class="opacity-0 scale-95"
            enter-to-class="opacity-100 scale-100" leave-active-class="transition duration-75 ease-in"
            leave-from-class="opacity-100 scale-100" leave-to-class="opacity-0 scale-95">
            <div v-if="open" :class="[
                'absolute mt-1 w-48 bg-surface border border-gray-300/40 shadow-lg rounded-lg py-1 z-50',
                align === 'left' ? 'left-0' : 'right-0'
            ]">
                <slot name="content" :close="close" />
            </div>
        </transition>
    </div>
</template>
