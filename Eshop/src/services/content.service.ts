import type { Product } from "../types";
import type { UploadableProduct } from "../types";

const headers = new Headers();
headers.set("content-type", "application/json");

export const getProducts = async (query?: string | null): Promise<Product[] | null> => {
    var url;
    if (query) {
        url = `/cl/product?q=${encodeURIComponent(query)}`
    } else {
        url = `/cl/product`
    }
    const response = await fetch(url, { method: "GET", credentials: "include", headers });
    const data = await response.json();
    return data.products || null;
}


export const getProduct = async (id: string): Promise<Product | null> => {
    const response = await fetch(`/cl/product/${id}`, { method: "GET", credentials: "include" });
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
    const buffer = await file.arrayBuffer();

    const signedUrlResponse = await fetch(
        `/api/getUrl?productId=${productId}&fileName=${encodeURIComponent(file.name)}`,
        {
            method: 'GET',
            credentials: 'include',
            headers
        }
    );

    const { url } = await signedUrlResponse.json();

    for (let i = 0; i < 5; i++) {
        try {
            const putResponse = await fetch(url, {
                method: 'PUT',
                body: buffer,
            });
            putResponse;
            break;
        } catch (error) {
            console.error(error)
            await new Promise(r => setTimeout(r, 1000));
        }
    }

    const publicUrl = url.split('?')[0];

    return { url: publicUrl };
};
