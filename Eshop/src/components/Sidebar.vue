<script setup lang="ts">
import { XCircleIcon, GiftIcon, ShoppingBagIcon, Cog6ToothIcon } from '@heroicons/vue/24/solid';
import { CurrentViewSelection, type SidebarOptions } from '../types';
import { onActivated, onUpdated, onMounted, onUnmounted } from 'vue';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ (e: 'close'): void; (e: 'select', option: CurrentViewSelection): void }>();

const options: SidebarOptions[] = [
    { title: "Products", key: CurrentViewSelection.Product, icon: GiftIcon },
    { title: "Orders", key: CurrentViewSelection.Order, icon: ShoppingBagIcon },
    { title: "Settings", key: CurrentViewSelection.Setting, icon: Cog6ToothIcon }
];

const selectOption = (key: CurrentViewSelection) => emit('select', key);
//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => { });
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
                    <h2 class="font-semibold text-sm sm:text-xl">ხათის საჩუქრების ზარდახშა</h2>
                    <button @click="$emit('close')"
                        class="w-8 h-8 rounded-md flex items-center justify-center text-text hover:text-primary transition-colors">
                        <XCircleIcon class="w-4 h-4" />
                    </button>
                </div>

                <!-- Navigation -->
                <nav class="space-y-3">
                    <button v-for="opt in options" :key="opt.key" @click="selectOption(opt.key)"
                        class="w-full text-left flex items-center sm:justify-start gap-3 px-3 py-3 rounded-md hover:bg-subtle cursor-pointer transition-colors">
                        <component :is="opt.icon" class="w-6 h-6" />
                        <span class="text-base sm:text-sm font-medium">{{ opt.title }}</span>
                    </button>
                </nav>
            </aside>
        </transition>
    </div>
</template>
