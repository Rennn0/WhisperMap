<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import NavBar from './navbar/NavbarView.vue';
import Sidebar from './sidebar/Sidebar.vue';
import { CurrentViewSelection, type Product } from '../types';
import { useRouter } from 'vue-router';

const router = useRouter();
const sidebarOpen = ref(false);
const navbarOpen = ref(true);


var lastScrollTop = 0;
const handleScroll = () => {
    var st = window.pageYOffset || document.documentElement.scrollTop;
    if (st > lastScrollTop) {
        navbarOpen.value = false;
    } else if (st < lastScrollTop) {
        navbarOpen.value = true;
    }
    lastScrollTop = st <= 0 ? 0 : st;
};


const onMenuToggle = () => sidebarOpen.value = true

const onProfileClick = (): void => { }

const onProductChosen = (product: Product) => router.push({ name: "product", params: { id: product.id } })

const onOptionSelect = (key: CurrentViewSelection) => {
    sidebarOpen.value = false;
    switch (key) {
        case CurrentViewSelection.Product:
            router.push({ name: "products" })
            break;

        case CurrentViewSelection.Cart:
            router.push({ name: "cart" })
            break;

        case CurrentViewSelection.Setting:
            router.push({ name: "settings" })
            break;
    }
}

const onUpload = () => router.push("upload");

onMounted(() => { window.addEventListener("scroll", handleScroll, false) });
onUnmounted(() => { window.removeEventListener("scroll", handleScroll, false) })
//#endregion
</script>

<template>
    <div class="min-h-screen bg-surface text-text transition-colors duration-300">
        <title>{{ $t('app.title') }}</title>

        <NavBar :class="navbarOpen ? 'translate-y-0 opacity-100' : '-translate-y-full opacity-0'"
            @menu-toggle="onMenuToggle" @profile-click="onProfileClick" @product-chosen="onProductChosen"
            @upload="onUpload" />
        <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" @select="onOptionSelect" />

        <main class="max-w-6xl mx-auto p-4">
            <router-view />
        </main>
    </div>
</template>