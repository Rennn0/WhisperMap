import { backendUrl, apiKey } from "./variables.js";

export const config = {
    runtime: "edge",
};

export default async function handler(req) {
    const url = new URL(req.url);
    const productId = url.searchParams.get("productId");
    const fileName = url.searchParams.get("fileName");

    const headers = new Headers(req.headers);
    headers.set("x-api-key", apiKey);
    const res = await fetch(
        `${backendUrl}/product/${productId}/storage?fileName=${encodeURIComponent(fileName)}`,
        {
            method: "GET",
            headers
        }
    );

    const data = await res.json();
    return new Response(JSON.stringify(data), {
        status: res.status,
        headers: { "content-type": "application/json" },
    });
}
