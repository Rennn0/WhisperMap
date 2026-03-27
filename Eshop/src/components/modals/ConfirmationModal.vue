<script setup lang="ts">
import { watch, onUnmounted } from 'vue';
import IconParkCloseIcon from '../freestyle/IconParkCloseIcon.vue';

const props = defineProps<{
    isOpen: boolean;
    title: string;
    description?: string;
    cancelText: string;
    confirmText: string;
}>();

const emit = defineEmits<{
    (e: 'confirmed'): void;
    (e: 'cancelled'): void;
}>();

const lockBodyScroll = () => {
    document.body.style.overflow = 'hidden';
};

const unlockBodyScroll = () => {
    document.body.style.overflow = '';
};

watch(
    () => props.isOpen,
    isOpen => {
        if (isOpen) lockBodyScroll();
        else unlockBodyScroll();
    },
    { immediate: true }
);

onUnmounted(() => {
    unlockBodyScroll();
});
</script>

<template>
    <transition enter-active-class="transition duration-200 ease-out" enter-from-class="opacity-0"
        enter-to-class="opacity-100" leave-active-class="transition duration-150 ease-in" leave-from-class="opacity-100"
        leave-to-class="opacity-0">
        <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-black/45 px-4"
            @click.self="emit('cancelled')">
            <div class="relative w-full max-w-sm rounded-2xl border border-subtle bg-surface p-5 shadow-2xl sm:p-6">
                <button type="button"
                    class="absolute right-3 top-3 inline-flex h-8 w-8 items-center justify-center rounded-full text-text/70 transition-colors hover:bg-hover hover:text-text"
                    @click="emit('cancelled')">
                    <IconParkCloseIcon class="h-4 w-4" />
                </button>

                <div class="pr-8">
                    <h2 class="text-base font-semibold text-text sm:text-lg">
                        {{ title }}
                    </h2>

                    <p v-if="description" class="mt-2 text-sm leading-6 text-text/80">
                        {{ description }}
                    </p>
                </div>

                <div class="mt-6 flex justify-end gap-2">
                    <button type="button"
                        class="rounded-xl border border-subtle bg-surface px-4 py-2 text-sm font-medium text-text transition-colors hover:bg-hover"
                        @click="emit('cancelled')">
                        {{ cancelText }}
                    </button>

                    <button type="button"
                        class="rounded-xl bg-primary px-4 py-2 text-sm font-medium text-white transition-opacity hover:opacity-90"
                        @click="emit('confirmed')">
                        {{ confirmText }}
                    </button>
                </div>
            </div>
        </div>
    </transition>
</template>