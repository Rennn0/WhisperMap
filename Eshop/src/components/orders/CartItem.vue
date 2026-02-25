<script setup lang="ts">
import { ref } from 'vue';
import type { TCartItem } from '../../types';
import BaseDropdown from '../dropdown/BaseDropdown.vue';
import TablerThreeDotIcon from '../freestyle/TablerThreeDotIcon.vue';
import ConfirmationModal from '../modals/ConfirmationModal.vue';
import TablerLoaderBlockWave from '../freestyle/TablerLoaderBlockWave.vue';
import ExpandableText from '../shared/ExpandableText.vue';

const props = defineProps<TCartItem>();

const emit = defineEmits<{
    remove: [id: number];
    visit: [id: number];
}>();

const showRemoveConfirmation = ref(false);
const showLoading = ref(false);

const handleVisit = (close: () => void) => {
    close();
    emit('visit', props.id);
};

const handleRemoveClick = () => {
    showRemoveConfirmation.value = true;
};

const handleRemoveCancelled = () => {
    showRemoveConfirmation.value = false;
};

const handleRemoveConfirmed = () => {
    showRemoveConfirmation.value = false;
    showLoading.value = true;
    emit('remove', props.id);
};
</script>

<template>
    <div v-if="!showLoading" class="p-3 rounded-md relative bg-surface border border-subtle">

        <div class="flex items-center justify-between gap-2">
            <div class="flex flex-col text-text">
                <div class="font-medium">
                    <ExpandableText :text="props.title" />
                </div>
            </div>

            <div class="flex items-center gap-3">
                <div class="text-sm px-2 py-1 rounded-full whitespace-nowrap bg-subtle text-text">
                    ${{ props.price }}
                </div>

                <!-- Dropdown -->
                <BaseDropdown :align="'right'">
                    <!-- Trigger -->
                    <template #trigger="{ toggle }">
                        <button @click="toggle()" class="p-1 rounded-md hover:bg-subtle transition-colors duration-200"
                            aria-label="Open menu">
                            <TablerThreeDotIcon class="w-5 h-5" />
                        </button>
                    </template>

                    <!-- Content -->
                    <template #content="{ close }">
                        <div class="py-1">

                            <button @click="handleVisit(close)"
                                class="w-full text-left px-4 py-2 text-sm transition-colors hover:bg-subtle">
                                Visit
                            </button>

                            <button @click="() => { handleRemoveClick(); close(); }"
                                class="w-full text-left px-4 py-2 text-sm hover:bg-danger-bg text-danger-text transition-colors">
                                Remove
                            </button>

                        </div>
                    </template>
                </BaseDropdown>
            </div>
        </div>
    </div>

    <!-- Loading -->
    <TablerLoaderBlockWave v-else class="h-12 w-full" />

    <!-- Confirmation -->
    <ConfirmationModal :isOpen="showRemoveConfirmation" :title="$t('product.modals.delete.desc')"
        :cancel-text="$t('product.modals.delete.cancel')" :confirm-text="$t('product.modals.delete.confirm')"
        @confirmed="handleRemoveConfirmed" @cancelled="handleRemoveCancelled" />
</template>