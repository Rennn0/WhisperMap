<script setup lang="ts">
import { watchEffect } from 'vue';
import IconParkCloseIcon from '../freestyle/IconParkCloseIcon.vue';

defineProps<{
    isOpen: boolean;
    title: string;
    text: string;
}>();

const emit = defineEmits<{
    (e: 'closed'): void;
}>();

const handleClose = () => {
    emit('closed');
};

watchEffect(() => {
    if (document) {
        document.body.style.overflow = document.body.style.overflow === 'hidden' ? '' : (document.body.style.overflow || '');
    }
});
</script>

<template>
    <transition enter-active-class="transition opacity duration-200"
        leave-active-class="transition opacity duration-200" enter-from-class="opacity-0" leave-to-class="opacity-0">
        <div v-if="isOpen" class="fixed inset-0 flex items-center justify-center backdrop-blur-md z-50"
            @click.self="handleClose">
            <div class="bg-surface rounded-lg shadow-lg w-11/12 max-w-sm p-6 relative border border-subtle">
                <button @click="handleClose" class="absolute top-2 right-2 text-text hover:text-primary transition">
                    <IconParkCloseIcon class="w-5 h-5" />
                </button>

                <h2 class="text-lg font-semibold mb-4">{{ title }}</h2>
                <div class="text-sm text-text/80 space-y-3">
                    <p v-for="(line, idx) in text.split('\n')" :key="idx">
                        {{ line }}
                    </p>
                </div>
            </div>
        </div>
    </transition>
</template>
