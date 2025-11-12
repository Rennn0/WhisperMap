<script setup lang="ts">
import { ref, onMounted, provide, readonly, onActivated, onUpdated, onUnmounted, onBeforeMount } from 'vue';
import NavBar from './navbar/NavbarView.vue';
import Sidebar from './sidebar/Sidebar.vue';
import { CurrentViewSelection, type Product } from '../types';
import { userInfoInjectionKey } from '../injectionKeys';
import { useRouter } from 'vue-router';
import { getMe, getSession } from '../services/user.service';
import { type UserInfo } from '../types';
import LoaderBlockWave from './freestyle/TablerLoaderBlockWave.vue';

//#region variables and providers
const router = useRouter();
// const title = ref("საჩუქრების ზარდახშა");
const sidebarOpen = ref(false);
const navbarOpen = ref(true);
const sessionEnabled = ref(false);
const userInfo = ref<UserInfo>();

// provide(titleInjectionKey, { data: readonly(title), update: (t: string) => title.value = t });
provide(userInfoInjectionKey, readonly(userInfo));
//#endregion1

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

const onUpload = () => router.push("upload");

//#endregion

//#region lifecycle hooks
onActivated(() => { });
onBeforeMount(() => {
    getSession()
        .then(() => {
            sessionEnabled.value = true;
            getMe()
                .then(info => {
                    if (info) {
                        userInfo.value = info;
                    }
                });
        });
});
onUpdated(() => { });
onMounted(() => {
    window.addEventListener("scroll", handleScroll, false)
});
onUnmounted(() => {
    window.removeEventListener("scroll", handleScroll, false)
})
//#endregion
</script>

<template>
    <div v-if="sessionEnabled" class="min-h-screen bg-surface text-text transition-colors duration-300">
        <title>{{ $t('app.title') }}</title>

        <NavBar :class="navbarOpen ? 'translate-y-0 opacity-100' : '-translate-y-full opacity-0'"
            @menu-toggle="onMenuToggle" @profile-click="onProfileClick" @product-chosen="onProductChosen"
            @upload="onUpload" />
        <Sidebar :open="sidebarOpen" @close="sidebarOpen = false" @select="onOptionSelect" />

        <main class="max-w-6xl mx-auto p-4">
            <router-view />
        </main>
    </div>
    <div v-else class="flex items-center justify-center min-h-screen">
        <LoaderBlockWave class="w-16 md:w-28 lg:w-44 h-16 md:h-28 lg:h-44" />
    </div>
</template>