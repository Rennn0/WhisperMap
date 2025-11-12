import type { Product } from "../types";
import type { UploadableProduct } from "../types";

const headers = new Headers();
headers.set("content-type", "application/json");

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


export const uploadProduct = async (product: UploadableProduct): Promise<{ product_id: number } | null> => {
    const response = await fetch(`/api/addProduct`, {
        method: "POST",
        headers,
        credentials: "include",
        body: JSON.stringify(product)
    });

    const data = await response.json();
    return data || null;
};

export const uploadProductFile = async (productId: number, file: File): Promise<{ url: string } | null> => {
    const signedUrlResponse = await fetch(
        `/api/getUrl?productId=${productId}&fileName=${encodeURIComponent(file.name)}`,
        {
            method: 'GET',
            credentials: 'include',
            headers
        }
    );

    const { url } = await signedUrlResponse.json();
    alert(`got url ${url}`)

    const putResponse = await fetch(url, {
        method: 'PUT',
        body: new Blob([file]),
    });
    alert("fetched")

    const publicUrl = url.split('?')[0];

    return { url: publicUrl };
};
