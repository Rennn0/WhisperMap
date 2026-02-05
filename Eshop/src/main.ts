import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import {createRouter, createWebHashHistory, createWebHistory} from 'vue-router'
import ProductDetail from './components/product/ProductDetailedView.vue'
import ProductsView from './components/product/ProductsView.vue'
import CartView from './components/orders/CartView.vue'
import SettingsView from './components/settings/SettingsView.vue'
import { ensureProductDetailAccess } from './guards/productDetail.guard'
import Root from './components/Root.vue'
import UploadView from './components/upload/UploadView.vue'
import { createI18n } from 'vue-i18n'

import eng from "./lang/eng.json"
import ka from "./lang/ka.json"
import rus from "./lang/rus.json"

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: "/",
            component: Root,
            name: "root",
            children: [
                {
                    path: "",
                    name: "products",
                    component: ProductsView
                },
                {
                    path: ":id",
                    name: "product",
                    component: ProductDetail,
                    props: true,
                    beforeEnter: ensureProductDetailAccess
                },
                {
                    path: "/cart",
                    component: CartView,
                    name: "cart"
                },
                {
                    path: "/settings",
                    component: SettingsView,
                    name: "settings"
                },
                {
                    path: "/upload",
                    name: "upload",
                    component: UploadView,
                }
            ]
        },

    ],
})

const i18n = createI18n({
    legacy: false,
    locale: 'ka',
    fallbackLocale: 'eng',
    messages: { eng, ka, rus }
});

createApp(App).use(router).use(i18n).mount('#app');
