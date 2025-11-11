import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createRouter, createWebHashHistory } from 'vue-router'
import ProductDetail from './components/product/ProductDetaiedView.vue'
import ProductsView from './components/product/ProductsView.vue'
import OrdersView from './components/orders/OrdersView.vue'
import SettingsView from './components/settings/SettingsView.vue'
import { ensureProductDetailAccess } from './guards/productDetail.guard'
import Root from './components/Root.vue'
import UploadView from './components/upload/UploadView.vue'

const router = createRouter({
    history: createWebHashHistory(),
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
                    path: "/orders",
                    component: OrdersView,
                    name: "orders"
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

createApp(App).use(router).mount('#app');
