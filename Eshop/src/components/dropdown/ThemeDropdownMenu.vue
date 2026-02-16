<script setup lang="ts">
import { computed } from 'vue';
import BaseDropdown from './BaseDropdown.vue';
import type { ThemeDropdown } from '../../types';

const _ = defineProps<{
    themes: ThemeDropdown[];
    modelValue: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
}>();

const currentTheme = computed(() =>
    _.themes.find(t => t.name === _.modelValue)
);
</script>

<template>
    <BaseDropdown :align="'right'">
        <!-- Trigger -->
        <template #trigger="{ toggle }">
            <button @click="toggle()"
                class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-subtle transition-colors duration-200"
                aria-label="Select theme">
                <component :is="currentTheme?.icon" class="w-6 h-6" />
            </button>
        </template>

        <!-- Content -->
        <template #content="{ close }">
            <div>
                <button v-for="theme in themes" :key="theme.name"
                    @click="() => { emit('update:modelValue', theme.name); close(); }"
                    class="w-full flex items-center gap-3 px-3 py-2 text-sm transition-colors text-left" :class="[
                        'hover:bg-subtle',
                        modelValue === theme.name ? 'bg-subtle font-medium' : ''
                    ]">
                    <component :is="theme.icon" class="w-5 h-5" />
                    {{ theme.label }}
                </button>
            </div>
        </template>
    </BaseDropdown>
</template>
