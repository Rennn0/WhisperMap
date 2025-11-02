import type { RouteLocationNormalizedGeneric } from "vue-router";

export const ensureProductDetailAccess = (to: RouteLocationNormalizedGeneric, from: RouteLocationNormalizedGeneric) => {
    console.log(to, from);
    // return { name: "settings" };
}