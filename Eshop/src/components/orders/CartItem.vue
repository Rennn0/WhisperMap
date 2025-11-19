<script setup lang="ts">
import { ref } from 'vue';
import type { TCartItem } from '../../types';
import TablerThreeDotIcon from '../freestyle/TablerThreeDotIcon.vue';
import ConfirmationModal from '../modals/ConfirmationModal.vue';
import TablerLoaderBlockWave from '../freestyle/TablerLoaderBlockWave.vue';

const props = defineProps<TCartItem>();
const showRemoveConfirmation = ref(false);
const showMenu = ref(false);
const showLoading = ref(false);
const toggleMenu = () => (showMenu.value = !showMenu.value);

const emit = defineEmits<{
    remove: [id: number];
    visit: [id: number];
}>();

const handleVisitClick = () => {
    toggleMenu();
    emit("visit", props.id)
}
const handleRemoveClick = () => {
    showRemoveConfirmation.value = true
}
const handleRemoveCancelled = () => {
    showRemoveConfirmation.value = false;
}
const handleRemoveConfirmed = () => {
    showRemoveConfirmation.value = false;
    toggleMenu();
    showLoading.value = true;
    emit('remove', props.id)
}
</script>

<template>
    <div v-if="!showLoading" class="p-3 rounded-md relative"
        style="background: var(--color-surface); border: 1px solid var(--color-subtle);">
        <div class="flex items-center justify-between gap-2">
            <div class="flex flex-col text-[var(--color-text)]">
                <div class="font-medium">{{ props.title }}</div>
            </div>

            <div class="flex items-center gap-3">
                <div class="text-sm px-2 py-1 rounded-full whitespace-nowrap"
                    style="background: var(--color-subtle); color: var(--color-text);">
                    ${{ props.price }}
                </div>

                <button @click="toggleMenu" class="p-1 rounded transition" style="color: var(--color-text);"
                    :style="{ background: showMenu ? 'var(--color-subtle)' : 'transparent' }">
                    <TablerThreeDotIcon class="w-5 h-5" />
                </button>
            </div>
        </div>

        <div v-if="showMenu" class="absolute right-2 top-10 rounded-md border shadow-md z-10"
            style="background: var(--color-surface); border-color: var(--color-subtle);">
            <button @click="handleVisitClick" class="block px-4 py-2 text-left w-full transition"
                style="color: var(--color-text);"
                @mouseenter="($event.target as HTMLElement).style.background = 'var(--color-hover)'"
                @mouseleave="($event.target as HTMLElement).style.background = 'transparent'">
                Visit
            </button>

            <button @click="handleRemoveClick" class="block px-4 py-2 text-left w-full transition"
                style="color: var(--color-danger-text);"
                @mouseenter="($event.target as HTMLElement).style.background = 'var(--color-danger-bg)'"
                @mouseleave="($event.target as HTMLElement).style.background = 'transparent'">
                Remove
            </button>
        </div>
    </div>
    <TablerLoaderBlockWave v-else class="h-12 w-full" />

    <ConfirmationModal :isOpen="showRemoveConfirmation" :title="$t('product.modals.delete.desc')"
        :cancel-text="$t('product.modals.delete.cancel')" :confirm-text="$t('product.modals.delete.confirm')"
        @confirmed="handleRemoveConfirmed" @cancelled="handleRemoveCancelled" />
</template>
