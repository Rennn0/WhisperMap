import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createRouter, createWebHistory } from 'vue-router'
import ProductDetail from './components/product/ProductDetaiedView.vue'
import ProductsView from './components/product/ProductsView.vue'
import OrdersView from './components/orders/OrdersView.vue'
import SettingsView from './components/settings/SettingsView.vue'
import { ensureProductDetailAccess } from './guards/productDetail.guard'

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: "/",
            component: ProductsView,
            name: "products",
            children: []
        },
        {
            path: "/:id", component: ProductDetail,
            props: true,
            name: "product",
            beforeEnter: ensureProductDetailAccess,
        },
        { path: "/orders", component: OrdersView, name: "orders" },
        { path: "/settings", component: SettingsView, name: "settings" }
    ],
})

createApp(App).use(router).mount('#app');
