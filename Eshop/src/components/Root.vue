<script setup lang="ts">
import { ref, onMounted, provide, readonly, onActivated, onUpdated, onUnmounted } from 'vue';
import NavBar from './navbar/NavbarView.vue';
import Sidebar from './sidebar/Sidebar.vue';
import { CurrentViewSelection, type Product } from '../types';
import { titleInjectionKey } from '../injectionKeys';
import { useRouter } from 'vue-router';
import { getPhotos } from '../services/content.service';
import { getSession } from '../services/user.service';

//#region variables and providers
const router = useRouter();
const title = ref("საჩუქრების ზარდახშა");
const sidebarOpen = ref(false);
const navbarOpen = ref(true);


provide(titleInjectionKey, { title: readonly(title), update: (t: string) => title.value = t });
//#endregion

//#region computed / watchEffect

//#endregion

//#region event hooks
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

        case CurrentViewSelection.Order:
            router.push({ name: "orders" })
            break;

        case CurrentViewSelection.Setting:
            router.push({ name: "settings" })
            break;
    }
}

//#endregion

//#region lifecycle hooks
onActivated(() => { });
onUpdated(() => { });
onMounted(() => {
    window.addEventListener("scroll", handleScroll, false)
    Promise.all([getSession(), getPhotos()]).then(([session, photos]) => console.log(session, photos));
});
onUnmounted(() => {
    window.removeEventListener("scroll", handleScroll, false)
})
//#endregion

</script>

<template>
    <div class="min-h-screen bg-surface text-text transition-colors duration-300">
        <title>{{ title }}</title>

        <NavBar :class="navbarOpen ? 'translate-y-0 opacity-100' : '-translate-y-full opacity-0'"
            @menu-toggle="onMenuToggle" @profile-click="onProfileClick" @product-chosen="onProductChosen" />
        <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" @select="onOptionSelect" />

        <main class="max-w-6xl mx-auto p-4">
            <router-view />
        </main>
    </div>
</template>