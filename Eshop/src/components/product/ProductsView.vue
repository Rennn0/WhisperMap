<script setup lang="ts">
import { computed, inject, onMounted, ref, watch, type Ref } from 'vue';
import type { Product, UserInfo } from '../../types';
import { useRouter } from 'vue-router';
import ProductItem from './ProductItem.vue';
import SkeletonProductItem from '../skeletons/SkeletonProductItem.vue';
import { deleteProduct, getProducts, includeProduct } from '../../services/http';
import BaseDropdown from '../dropdown/BaseDropdown.vue';
import { userInfoInjectionKey } from '../../injectionKeys';
import ConfirmationModal from '../modals/ConfirmationModal.vue';

enum OrderBy {
    NewestFirst = 0,
    OldestFirst = 1,
    PriceIncreasing = 2,
    PriceDecreasing = 3
}

const STORAGE_KEYS = {
    order: 'products.selectedOrder',
    batch: 'products.selectedBatch'
} as const;

type ContextActionContext = {
    selectedIds: number[];
    selectedCount: number;
    selectionMode: boolean;
    userInfo?: UserInfo;
};

type ContextActionConfirmation = {
    titleKey: string;
    descriptionKey?: string;
    cancelKey: string;
    confirmKey: string;
};

type ContextAction = {
    key: string;
    labelKey: string;
    canDisplay: (context: ContextActionContext) => boolean;
    action: (context: ContextActionContext) => void;
    confirmation?: ContextActionConfirmation;
};

const router = useRouter();
const emit = defineEmits<{
    (e: 'select', product: Product): void;
    (e: 'selected-ids-change', ids: number[]): void;
    (e: 'delete-selected', ids: number[]): void;
    (e: 'add-selected', ids: number[]): void;
}>();

const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);

const loading = ref(false);
const loadingMore = ref(false);
const initialized = ref(false);

const productsRef = ref<Product[]>([]);
const continuationToken = ref<string | null>(null);

const selectedOrder = ref<OrderBy>(OrderBy.NewestFirst);
const selectedBatch = ref<number>(10);

const selectionMode = ref(false);
const selectedIds = ref<number[]>([]);

const pendingAction = ref<ContextAction | null>(null);
const showActionConfirmation = ref(false);

const snackbar = ref<{
    visible: boolean;
    messageKey: string;
    tone: 'success' | 'error';
}>({
    visible: false,
    messageKey: '',
    tone: 'success'
});

let snackbarTimeout: ReturnType<typeof setTimeout> | null = null;


const products = computed(() => productsRef.value);
const hasProducts = computed(() => products.value.length > 0);
const hasMore = computed(() => !!continuationToken.value);
const isBusy = computed(() => loading.value || loadingMore.value);
const selectedCount = computed(() => selectedIds.value.length);

const orderOptions = [
    { value: OrderBy.NewestFirst, label: 'product.sort.newestFirst' },
    { value: OrderBy.OldestFirst, label: 'product.sort.oldestFirst' },
    { value: OrderBy.PriceIncreasing, label: 'product.sort.priceIncreasing' },
    { value: OrderBy.PriceDecreasing, label: 'product.sort.priceDecreasing' }
] as const;

const batchOptions = [
    { value: 5, label: '5' },
    { value: 10, label: '10' },
    { value: 20, label: '20' },
    { value: 50, label: '50' }
] as const;

const selectedOrderLabel = computed(() =>
    orderOptions.find(x => x.value === selectedOrder.value)?.label ?? orderOptions[0].label
);

const selectedBatchLabel = computed(() =>
    batchOptions.find(x => x.value === selectedBatch.value)?.label ?? String(batchOptions[0].value)
);

const contextActionContext = computed<ContextActionContext>(() => ({
    selectedIds: [...selectedIds.value],
    selectedCount: selectedCount.value,
    selectionMode: selectionMode.value,
    userInfo: userInfo?.value
}));

const contextActions = computed<ContextAction[]>(() => [
    {
        key: 'delete',
        labelKey: 'product.actions.delete',
        canDisplay: context =>
            !!context.userInfo?.can_delete &&
            context.selectionMode &&
            context.selectedCount > 0,
        confirmation: {
            titleKey: 'product.modals.delete.title',
            descriptionKey: 'product.modals.delete.desc',
            cancelKey: 'product.modals.delete.cancel',
            confirmKey: 'product.modals.delete.confirm'
        },
        action: async context => {
            for (const id of context.selectedIds)
                await deleteProduct(id).request

            router.push({ name: 'root' }).then(() => { window.location.reload(); })
            // emit('delete-selected', [...context.selectedIds]);
        }
    },
    {
        key: 'add',
        labelKey: 'product.actions.add',
        canDisplay: context => context.selectionMode && context.selectedCount > 0,
        action: async context => {
            for (const id of context.selectedIds)
                await includeProduct(id).request;

            showSnackbar(context.selectedCount > 1 ? 'product.snackBar.addOkMany' : 'product.snackBar.addOk', 'success');
            clearSelection();
            // emit('add-selected', [...context.selectedIds]);
        }
    }
]);

const visibleContextActions = computed(() =>
    contextActions.value.filter(action => action.canDisplay(contextActionContext.value))
);

const canShowActions = computed(() => visibleContextActions.value.length > 0);

const confirmationState = computed(() => pendingAction.value?.confirmation ?? null);

const restorePreferences = () => {
    const savedOrder = localStorage.getItem(STORAGE_KEYS.order);
    const savedBatch = localStorage.getItem(STORAGE_KEYS.batch);

    const parsedOrder = Number(savedOrder);
    const parsedBatch = Number(savedBatch);

    if (orderOptions.some(x => x.value === parsedOrder)) {
        selectedOrder.value = parsedOrder as OrderBy;
    }

    if (batchOptions.some(x => x.value === parsedBatch)) {
        selectedBatch.value = parsedBatch;
    }
};

const persistPreferences = () => {
    localStorage.setItem(STORAGE_KEYS.order, String(selectedOrder.value));
    localStorage.setItem(STORAGE_KEYS.batch, String(selectedBatch.value));
};

const emitSelectedIds = () => {
    emit('selected-ids-change', [...selectedIds.value]);
};

const clearSelection = () => {
    selectedIds.value = [];
    selectionMode.value = false;
    emitSelectedIds();
};

const isSelected = (id?: number | null) => {
    if (id == null) return false;
    return selectedIds.value.includes(id);
};

const toggleSelection = (id?: number | null) => {
    if (id == null) return;

    if (isSelected(id)) {
        selectedIds.value = selectedIds.value.filter(x => x !== id);
    } else {
        selectedIds.value = [...selectedIds.value, id];
    }

    emitSelectedIds();
};

const executeContextAction = (action: ContextAction) => {
    action.action(contextActionContext.value);
};

const openActionConfirmation = (action: ContextAction) => {
    pendingAction.value = action;
    showActionConfirmation.value = true;
};

const handleContextActionClick = (action: ContextAction, close: () => void) => {
    close();

    if (action.confirmation) {
        openActionConfirmation(action);
        return;
    }

    executeContextAction(action);
};

const handleActionConfirmed = () => {
    if (pendingAction.value) {
        executeContextAction(pendingAction.value);
    }

    pendingAction.value = null;
    showActionConfirmation.value = false;
};

const handleActionCancelled = () => {
    pendingAction.value = null;
    showActionConfirmation.value = false;
};

const onProductClick = (product: Product) => {
    if (selectionMode.value) {
        toggleSelection(product.id);
        return;
    }

    router.push({ name: 'product', params: { id: product.id } });
    emit('select', product);
};

const loadProducts = async () => {
    if (loading.value) return;

    loading.value = true;
    continuationToken.value = null;

    try {
        const response = await getProducts({
            orderBy: selectedOrder.value,
            continuationToken: null,
            batch: selectedBatch.value
        }).request;

        productsRef.value = response.products ?? [];
        continuationToken.value = response.continuation_token ?? null;

        if (selectionMode.value) {
            const visibleIds = new Set(productsRef.value.map(x => x.id).filter((x): x is number => x != null));
            selectedIds.value = selectedIds.value.filter(id => visibleIds.has(id));
            emitSelectedIds();
        }
    } finally {
        loading.value = false;
    }
};


const loadMore = async () => {
    if (loading.value || loadingMore.value || !continuationToken.value) return;

    loadingMore.value = true;

    try {
        const response = await getProducts({
            orderBy: selectedOrder.value,
            continuationToken: continuationToken.value,
            batch: selectedBatch.value
        }).request;

        const nextProducts = response.products ?? [];
        productsRef.value = [...productsRef.value, ...nextProducts];
        continuationToken.value = response.continuation_token ?? null;
    } finally {
        loadingMore.value = false;
    }
};

const selectOrder = (value: OrderBy, close: () => void) => {
    if (selectedOrder.value !== value) {
        selectedOrder.value = value;
    }

    close();
};

const selectBatch = (value: number, close: () => void) => {
    if (selectedBatch.value !== value) {
        selectedBatch.value = value;
    }

    close();
};


const showSnackbar = (message: string, tone: 'success' | 'error' = 'success') => {
    snackbar.value.visible = true;
    snackbar.value.messageKey = message;
    snackbar.value.tone = tone;

    if (snackbarTimeout) {
        clearTimeout(snackbarTimeout);
    }

    snackbarTimeout = setTimeout(() => {
        snackbar.value.visible = false;
    }, 2500);
};

watch([selectedOrder, selectedBatch], async () => {
    if (!initialized.value) return;

    persistPreferences();
    await loadProducts();
});

watch(selectionMode, value => {
    if (!value) {
        clearSelection();
    }
});

onMounted(async () => {
    restorePreferences();
    initialized.value = true;
    await loadProducts();
});
</script>

<template>
    <div class="space-y-5">
        <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div class="text-sm text-subtle">
                <span v-if="hasProducts">
                    {{ $t('product.sort.result') }} {{ products.length }}
                </span>
            </div>

            <div class="flex flex-wrap items-center gap-2 sm:justify-end">
                <button v-if="userInfo?.can_delete" type="button"
                    class="inline-flex items-center justify-between gap-2 rounded-xl border border-subtle bg-surface px-2.5 py-2 text-xs text-text transition-colors hover:bg-hover"
                    :class="selectionMode ? 'font-semibold' : ''" @click="selectionMode = !selectionMode">
                    <span class="truncate">
                        {{ $t('product.select') }} {{ selectedCount > 0 ? `(${selectedCount})` : '' }}
                    </span>

                    <span class="inline-flex h-4 w-4 items-center justify-center rounded-full"
                        :class="selectionMode ? 'bg-primary' : 'bg-hover'">
                        <span class="h-1.5 w-1.5 rounded-full bg-surface" />
                    </span>
                </button>

                <BaseDropdown v-if="canShowActions" align="left" widthClass="auto">
                    <template #trigger="{ toggle, open, triggerRef }">
                        <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                            class="inline-flex items-center justify-between gap-2 rounded-xl border border-subtle bg-surface px-2.5 py-2 text-xs text-text transition-colors hover:bg-hover focus:outline-none"
                            @click="toggle()">
                            <span class="truncate font-medium">
                                {{ $t('product.actions.title') }}
                            </span>

                            <svg class="h-3.5 w-3.5 shrink-0 text-subtle transition-transform duration-200"
                                :class="open ? 'rotate-180' : ''" viewBox="0 0 20 20" fill="currentColor"
                                aria-hidden="true">
                                <path fill-rule="evenodd"
                                    d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z"
                                    clip-rule="evenodd" />
                            </svg>
                        </button>
                    </template>

                    <template #content="{ close }">
                        <div class="flex flex-col gap-1">
                            <button v-for="action in visibleContextActions" :key="action.key" type="button"
                                role="menuitem"
                                class="flex w-full items-center gap-2 rounded-lg px-2.5 py-2 text-left text-xs text-text transition-colors hover:bg-hover"
                                @click="handleContextActionClick(action, close)">
                                <span class="truncate">
                                    {{ $t(action.labelKey) }}
                                </span>
                            </button>
                        </div>
                    </template>
                </BaseDropdown>

                <div class="flex items-center gap-2">
                    <span class="text-xs opacity-70 text-text">
                        {{ $t('product.sort.sortDisplay') }}
                    </span>

                    <BaseDropdown align="left" widthClass="auto">
                        <template #trigger="{ toggle, open, triggerRef }">
                            <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                                class="inline-flex items-center justify-between gap-2 rounded-xl border border-subtle bg-surface px-2.5 py-2 text-xs text-text transition-colors hover:bg-hover focus:outline-none"
                                @click="toggle()">
                                <span class="truncate font-medium">
                                    {{ $t(selectedOrderLabel) }}
                                </span>

                                <svg class="h-3.5 w-3.5 shrink-0 text-subtle transition-transform duration-200"
                                    :class="open ? 'rotate-180' : ''" viewBox="0 0 20 20" fill="currentColor"
                                    aria-hidden="true">
                                    <path fill-rule="evenodd"
                                        d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z"
                                        clip-rule="evenodd" />
                                </svg>
                            </button>
                        </template>

                        <template #content="{ close }">
                            <div class="flex flex-col gap-1">
                                <button v-for="option in orderOptions" :key="option.value" type="button" role="menuitem"
                                    class="flex w-full items-center gap-2 rounded-lg px-2.5 py-2 text-left text-xs transition-colors"
                                    :class="selectedOrder === option.value
                                        ? 'bg-hover font-semibold text-text'
                                        : 'text-text hover:bg-hover'" @click="selectOrder(option.value, close)">
                                    <span class="truncate">
                                        {{ $t(option.label) }}
                                    </span>

                                    <span v-if="selectedOrder === option.value"
                                        class="ml-auto inline-block h-2 w-2 rounded-full bg-primary" />
                                </button>
                            </div>
                        </template>
                    </BaseDropdown>
                </div>

                <div class="flex items-center gap-2">
                    <span class="text-xs opacity-70 text-text">
                        {{ $t('product.sort.batchDisplay') }}
                    </span>

                    <BaseDropdown align="left" widthClass="auto">
                        <template #trigger="{ toggle, open, triggerRef }">
                            <button :ref="triggerRef as any" type="button" :aria-expanded="open" aria-haspopup="menu"
                                class="inline-flex items-center justify-between gap-2 rounded-xl border border-subtle bg-surface px-2.5 py-2 text-xs text-text transition-colors hover:bg-hover focus:outline-none"
                                @click="toggle()">
                                <span class="truncate font-medium">
                                    {{ selectedBatchLabel }}
                                </span>

                                <svg class="h-3.5 w-3.5 shrink-0 text-subtle transition-transform duration-200"
                                    :class="open ? 'rotate-180' : ''" viewBox="0 0 20 20" fill="currentColor"
                                    aria-hidden="true">
                                    <path fill-rule="evenodd"
                                        d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.51a.75.75 0 01-1.08 0l-4.25-4.51a.75.75 0 01.02-1.06z"
                                        clip-rule="evenodd" />
                                </svg>
                            </button>
                        </template>

                        <template #content="{ close }">
                            <div class="flex flex-col gap-1">
                                <button v-for="option in batchOptions" :key="option.value" type="button" role="menuitem"
                                    class="flex w-full items-center gap-2 rounded-lg px-2.5 py-2 text-left text-xs transition-colors"
                                    :class="selectedBatch === option.value
                                        ? 'bg-hover font-semibold text-text'
                                        : 'text-text hover:bg-hover'" @click="selectBatch(option.value, close)">
                                    <span class="truncate">
                                        {{ option.label }}
                                    </span>

                                    <span v-if="selectedBatch === option.value"
                                        class="ml-auto inline-block h-2 w-2 rounded-full bg-primary" />
                                </button>
                            </div>
                        </template>
                    </BaseDropdown>
                </div>
            </div>
        </div>

        <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            <template v-if="loading">
                <SkeletonProductItem v-for="(_, i) in Array(selectedBatch)" :key="i" />
            </template>

            <template v-else>
                <div v-for="item in products" :key="item.id" class="relative">
                    <button v-if="selectionMode" type="button"
                        class="absolute left-3 top-3 z-20 inline-flex h-6 w-6 items-center justify-center rounded-full bg-surface/95 text-text transition-colors hover:bg-hover"
                        @click.stop="toggleSelection(item.id)">
                        <span class="h-3 w-3 rounded-full" :class="isSelected(item.id) ? 'bg-primary' : 'bg-hover'" />
                    </button>

                    <div class="rounded-2xl transition-all duration-150"
                        :class="selectionMode && isSelected(item.id) ? 'ring-2 ring-primary/60' : ''">
                        <ProductItem :product="item" @select="onProductClick(item)" />
                    </div>
                </div>
            </template>
        </div>

        <div v-if="!loading && hasProducts" class="flex flex-col items-center justify-center gap-3 pt-2">
            <button v-if="hasMore" type="button"
                class="inline-flex min-w-[160px] items-center justify-center gap-2 rounded-xl bg-surface px-5 py-3 text-sm font-medium text-text transition-colors hover:bg-hover disabled:cursor-not-allowed disabled:opacity-50"
                :disabled="isBusy" @click="loadMore">
                <svg v-if="loadingMore" class="h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none">
                    <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-opacity="0.2" stroke-width="4" />
                    <path d="M22 12a10 10 0 0 1-10 10" stroke="currentColor" stroke-width="4" stroke-linecap="round" />
                </svg>

                <span>
                    {{ loadingMore
                        ? `${$t('product.sort.loading')}`
                        : `${$t('product.sort.more')} ${selectedBatchLabel}` }}
                </span>
            </button>

            <p v-else class="text-sm text-subtle">
                {{ $t('product.sort.noMore') }}
            </p>
        </div>
    </div>

    <ConfirmationModal :isOpen="showActionConfirmation" :title="confirmationState ? $t(confirmationState.titleKey) : ''"
        :description="confirmationState?.descriptionKey ? $t(confirmationState.descriptionKey) : undefined"
        :cancel-text="confirmationState ? $t(confirmationState.cancelKey) : ''"
        :confirm-text="confirmationState ? $t(confirmationState.confirmKey) : ''" @confirmed="handleActionConfirmed"
        @cancelled="handleActionCancelled" />

    <transition enter-active-class="transition duration-200 ease-out" enter-from-class="opacity-0 translate-y-2"
        enter-to-class="opacity-100 translate-y-0" leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0" leave-to-class="opacity-0 translate-y-2">
        <div v-if="snackbar.visible"
            class="fixed bottom-4 right-4 z-[60] max-w-xs rounded-xl px-4 py-3 text-sm shadow-lg" :class="snackbar.tone === 'success'
                ? 'bg-surface border border-subtle text-text'
                : 'bg-danger-bg text-danger-text border border-subtle'">
            {{ $t(snackbar.messageKey) }}
        </div>
    </transition>
</template>