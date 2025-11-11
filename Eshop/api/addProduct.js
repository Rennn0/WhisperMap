import { backendUrl, apiKey } from "./variables.js";

export const config = {
    runtime: "edge",
};

export default async function handler(req) {
    const cookies = req.headers.get("cookie") || "";
    const url = new URL(req.url);
    const productId = url.pathname.split("/").pop();

    const headers = new Headers(req.headers);
    headers.set("x-api-key", apiKey);
    const apiResponse = await fetch(`${backendUrl}/product`, {
        method: "POST",
        headers,
        body: req.body,
    });

    return new Response(apiResponse.body, {
        status: apiResponse.status,
        headers: apiResponse.headers
    });
}
