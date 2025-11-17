<script setup lang="ts">
import { watchEffect } from 'vue';
import IconParkCloseIcon from '../freestyle/IconParkCloseIcon.vue';

const props = defineProps<{
    isOpen: boolean;
    title: string;
    description?: string;
    cancelText: string,
    confirmText: string,
}>();

const emit = defineEmits<{
    (e: 'confirmed'): void;
    (e: 'cancelled'): void;
}>();


watchEffect(() => {
    if (props.isOpen)
        document.body.style.overflow = 'hidden';
    else
        document.body.style.overflow = '';
})

</script>

<template>
    <transition enter-active-class="transition opacity duration-200"
        leave-active-class="transition opacity duration-200" enter-from-class="opacity-0" leave-to-class="opacity-0">
        <div v-if="isOpen" class="fixed inset-0 flex items-center justify-center backdrop-blur-md z-50"
            @click.self="emit('cancelled')">
            <div class="bg-surface rounded-lg shadow-lg w-11/12 max-w-sm p-6 relative border border-subtle">
                <button @click="emit('cancelled')"
                    class="absolute top-2 right-2 text-text hover:text-primary transition">
                    <IconParkCloseIcon class="w-5 h-5" />
                </button>

                <h2 class="text-lg font-semibold mb-2">{{ title }}</h2>
                <p v-if="description" class="text-sm text-text/80 mb-6">{{ description }}</p>

                <div class="flex gap-3 justify-end">
                    <button class="px-4 py-2 rounded-md border text-text transition" @click="emit('cancelled')">
                        {{ props.cancelText }}
                    </button>
                    <button class="px-4 py-2 rounded-md border text-text transition" @click="emit('confirmed')">
                        {{ props.confirmText }}
                    </button>
                </div>
            </div>
        </div>
    </transition>
</template>
