<script setup lang="ts">
import { computed, ref } from 'vue';
import type { TCartItem } from '../../types';
import BaseDropdown from '../dropdown/BaseDropdown.vue';
import TablerThreeDotIcon from '../freestyle/TablerThreeDotIcon.vue';
import ConfirmationModal from '../modals/ConfirmationModal.vue';
import TablerLoaderBlockWave from '../freestyle/TablerLoaderBlockWave.vue';
import ExpandableText from '../shared/ExpandableText.vue';

const props = defineProps<TCartItem & {
    preview_img?: string;
}>();

const emit = defineEmits<{
    remove: [id: number];
    visit: [id: number];
}>();

const showRemoveConfirmation = ref(false);
const showLoading = ref(false);

const formattedPrice = computed(() => `$${(props.price ?? 0).toFixed(2)}`);

const handleVisit = (close: () => void) => {
    close();
    emit('visit', props.id);
};

const handleRemoveClick = (close: () => void) => {
    close();
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
    <div v-if="!showLoading"
        class="overflow-hidden rounded-2xl border border-subtle bg-surface transition-shadow duration-200 hover:shadow-sm">
        <div class="flex gap-4 p-4">
            <div class="h-20 w-20 shrink-0 overflow-hidden rounded-xl bg-subtle">
                <img v-if="props.preview_img" :src="props.preview_img" :alt="props.title"
                    class="h-full w-full object-cover" />

                <div v-else class="flex h-full w-full items-center justify-center bg-subtle" />
            </div>

            <div class="min-w-0 flex-1">
                <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                        <div class="text-text font-medium">
                            <ExpandableText :text="props.title" />
                        </div>

                        <div class="mt-2">
                            <span
                                class="inline-flex rounded-full bg-subtle px-2.5 py-1 text-sm font-medium text-primary">
                                {{ formattedPrice }}
                            </span>
                        </div>
                    </div>

                    <BaseDropdown align="right" widthClass="w-40">
                        <template #trigger="{ toggle, open, triggerRef }">
                            <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                                aria-label="Open menu"
                                class="flex h-9 w-9 items-center justify-center rounded-xl text-text transition-colors duration-200 hover:bg-hover focus:outline-none"
                                @click="toggle()">
                                <TablerThreeDotIcon class="h-5 w-5" />
                            </button>
                        </template>

                        <template #content="{ close }">
                            <div class="flex flex-col gap-1">
                                <button type="button" role="menuitem"
                                    class="w-full rounded-md px-3 py-2 text-left text-sm text-text transition-colors hover:bg-hover"
                                    @click="handleVisit(close)">
                                    Visit
                                </button>

                                <button type="button" role="menuitem"
                                    class="w-full rounded-md px-3 py-2 text-left text-sm text-danger-text transition-colors hover:bg-danger-bg"
                                    @click="handleRemoveClick(close)">
                                    Remove
                                </button>
                            </div>
                        </template>
                    </BaseDropdown>
                </div>
            </div>
        </div>
    </div>

    <TablerLoaderBlockWave v-else class="h-20 w-full" />

    <ConfirmationModal :isOpen="showRemoveConfirmation" :title="$t('product.modals.delete.desc')"
        :cancel-text="$t('product.modals.delete.cancel')" :confirm-text="$t('product.modals.delete.confirm')"
        @confirmed="handleRemoveConfirmed" @cancelled="handleRemoveCancelled" />
</template>