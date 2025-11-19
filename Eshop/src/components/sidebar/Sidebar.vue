<script setup lang="ts">
import { CurrentViewSelection, type SidebarOptions } from '../../types';
import { onActivated, onUpdated, onMounted, onUnmounted } from 'vue';
import { useI18n } from 'vue-i18n';
import GiftIcon from '../freestyle/TablerGiftIcon.vue';
import ShoppingBagIcon from '../freestyle/TablerShoppingBagIcon.vue';
import TablerSettingsIcon from '../freestyle/TablerSettingsIcon.vue';
import IconParkCloseIcon from '../freestyle/IconParkCloseIcon.vue';
import LanguageComponent from './LanguageComponent.vue';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ (e: 'close'): void; (e: 'select', option: CurrentViewSelection): void }>();
const { locale } = useI18n();

const options: SidebarOptions[] = [
    { title: "sidebar.product", key: CurrentViewSelection.Product, icon: GiftIcon },
    { title: "sidebar.cart", key: CurrentViewSelection.Cart, icon: ShoppingBagIcon },
    { title: "sidebar.settings", key: CurrentViewSelection.Setting, icon: TablerSettingsIcon }
];

const selectOption = (key: CurrentViewSelection) => emit('select', key);
//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => {
    const savedLang = localStorage.getItem('lang');
    if (savedLang) locale.value = savedLang;
});
onUnmounted(() => { })
//#endregion
</script>

<template>
    <div>
        <!-- Overlay -->
        <transition name="fade">
            <div v-if="props.open" class="fixed inset-0 bg-black/40 z-40" @click="$emit('close')"></div>
        </transition>

        <!-- Sidebar -->
        <transition enter-active-class="transform transition duration-300 ease-out" enter-from-class="-translate-x-full"
            enter-to-class="translate-x-0" leave-active-class="transform transition duration-200 ease-in"
            leave-from-class="translate-x-0" leave-to-class="-translate-x-full">
            <aside v-if="props.open" class="fixed inset-y-0 left-0 bg-surface border-r border-subtle z-50 p-4
               w-72 sm:w-80 md:w-96">
                <!-- Header -->
                <div class="flex items-center justify-between mb-6">
                    <h2 class="font-semibold text-sm sm:text-xl">{{ $t('app.title') }}</h2>
                    <button @click="$emit('close')"
                        class="w-8 h-8 rounded-md flex items-center justify-center text-text hover:text-primary transition-colors">
                        <IconParkCloseIcon class="w-6 h-6" />
                    </button>
                </div>

                <!-- Navigation -->
                <nav class="space-y-3">
                    <button v-for="opt in options" :key="opt.key" @click="selectOption(opt.key)"
                        class="w-full text-left flex items-center sm:justify-start gap-3 px-3 py-3 rounded-md hover:bg-subtle cursor-pointer transition-colors">
                        <component :is="opt.icon" class="w-6 h-6" />
                        <span class="text-base sm:text-sm font-medium">{{ $t(opt.title) }}</span>
                    </button>
                </nav>

                <!-- Language chooser -->
                <LanguageComponent />
            </aside>
        </transition>
    </div>
</template>
