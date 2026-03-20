<script setup lang="ts">
import { computed } from 'vue';
import BaseDropdown from './BaseDropdown.vue';
import type { ThemeDropdown } from '../../types';

const props = defineProps<{
    themes: ThemeDropdown[];
    modelValue: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
}>();

const currentTheme = computed(() =>
    props.themes.find(t => t.name === props.modelValue)
);

const selectTheme = (themeName: string, close: () => void) => {
    emit('update:modelValue', themeName);
    close();
};
</script>

<template>
    <BaseDropdown align="right" widthClass="w-56">
        <template #trigger="{ toggle, open, triggerRef }">
            <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                aria-label="Select theme"
                class="flex items-center justify-center w-10 h-10 rounded-md hover:bg-hover transition-colors duration-200 focus:outline-none"
                @click="toggle()">
                <component :is="currentTheme?.icon" v-if="currentTheme?.icon" class="w-6 h-6" />
                <span v-else class="text-sm">T</span>
            </button>
        </template>

        <template #content="{ close }">
            <div class="flex flex-col gap-1">
                <button v-for="theme in themes" :key="theme.name" type="button" role="menuitem"
                    class="w-full flex items-center gap-3 px-3 py-2 rounded-md text-sm text-left transition-colors"
                    :class="modelValue === theme.name ? 'bg-subtle font-medium' : 'hover:bg-hover'"
                    @click="selectTheme(theme.name, close)">
                    <component :is="theme.icon" class="w-5 h-5 shrink-0" />
                    <span class="truncate">{{ theme.label }}</span>

                    <span v-if="modelValue === theme.name"
                        class="ml-auto inline-block w-2 h-2 rounded-full bg-primary" />
                </button>
            </div>
        </template>
    </BaseDropdown>
</template>