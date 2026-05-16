<script setup lang="ts">
import { CurrentViewSelection, type SidebarOptions, type UserInfo } from '../../types';
import { onActivated, onUpdated, onMounted, onUnmounted, inject, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import GiftIcon from '../freestyle/TablerGiftIcon.vue';
import ShoppingBagIcon from '../freestyle/TablerShoppingBagIcon.vue';
import TablerSettingsIcon from '../freestyle/TablerSettingsIcon.vue';
import IconParkCloseIcon from '../freestyle/IconParkCloseIcon.vue';
import LanguageComponent from './LanguageComponent.vue';
import { userInfoInjectionKey } from '../../injectionKeys';

const props = defineProps<{ open: boolean }>();

const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'select', option: CurrentViewSelection): void;
}>();

const { locale } = useI18n();

const options: SidebarOptions[] = [
    { title: 'sidebar.product', key: CurrentViewSelection.Product, icon: GiftIcon },
    { title: 'sidebar.cart', key: CurrentViewSelection.Cart, icon: ShoppingBagIcon },
    { title: 'sidebar.settings', key: CurrentViewSelection.Setting, icon: TablerSettingsIcon },
];

const selectOption = (key: CurrentViewSelection) => emit('select', key);
const userInfo = inject<Readonly<Ref<UserInfo>>>(userInfoInjectionKey);

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });

onMounted(() => {
    const savedLang = localStorage.getItem('lang');
    if (savedLang) locale.value = savedLang;

    if (userInfo?.value.uid) options.push({ title: 'sidebar.timer', key: CurrentViewSelection.Timer, icon: TablerSettingsIcon })
});

onUnmounted(() => { });
//#endregion
</script>

<template>
    <div>
        <transition name="fade">
            <div v-if="props.open" class="fixed inset-0 z-40 bg-black/40 backdrop-blur-[1px]" @click="$emit('close')" />
        </transition>

        <transition enter-active-class="transform transition duration-300 ease-out" enter-from-class="-translate-x-full"
            enter-to-class="translate-x-0" leave-active-class="transform transition duration-200 ease-in"
            leave-from-class="translate-x-0" leave-to-class="-translate-x-full">
            <aside v-if="props.open"
                class="fixed inset-y-0 left-0 z-50 flex w-72 flex-col border-r border-subtle bg-surface shadow-lg sm:w-80">
                <div class="flex items-center justify-between px-4 py-4 sm:px-5">
                    <h2 class="text-base font-semibold text-text sm:text-lg">
                        {{ $t('app.title') }}
                    </h2>

                    <button type="button"
                        class="flex h-10 w-10 items-center justify-center rounded-xl text-text transition-colors hover:bg-hover"
                        @click="$emit('close')">
                        <IconParkCloseIcon class="h-5 w-5" />
                    </button>
                </div>

                <div class="flex-1 overflow-y-auto px-3 pb-4 sm:px-4">
                    <nav class="space-y-2">
                        <button v-for="opt in options" :key="opt.key" type="button"
                            class="flex w-full items-center gap-3 rounded-xl px-3 py-3 text-left text-text transition-colors hover:bg-subtle"
                            @click="selectOption(opt.key)">
                            <div
                                class="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-subtle text-text">
                                <component :is="opt.icon" class="h-5 w-5" />
                            </div>

                            <span class="text-sm font-medium">
                                {{ $t(opt.title) }}
                            </span>
                        </button>
                    </nav>
                </div>

                <div class="px-4 pb-4 sm:px-5">
                    <LanguageComponent />
                </div>
            </aside>
        </transition>
    </div>
</template>