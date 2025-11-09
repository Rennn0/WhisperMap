import type { Product } from "../types";

export const getProducts = async (): Promise<Product[] | null> => {
    const response = await fetch(`/api/products`, { method: "GET", credentials: "include" });
    const data = await response.json();
    return data.products || null;
}


export const getProduct = async (id: string): Promise<Product | null> => {
    const response = await fetch(`/api/product/${id}`, { method: "GET", credentials: "include" });
    const data = await response.json();
    return data || null;
}