import { backendUrl, apiKey } from "../variables.js";

export const config = {
    runtime: "edge",
};

export default async function handler(req) {
    const url = new URL(req.url);
    const productId = url.pathname.split("/").pop();

    const headers = new Headers(req.headers);
    headers.set("x-api-key", apiKey);
    const apiResponse = await fetch(`${backendUrl}/product/${productId}/storage`, {
        method: "POST",
        headers,
        body: req.body,
        duplex: "half"
    });

    return new Response(apiResponse.body, {
        status: apiResponse.status,
        headers: apiResponse.headers
    });
}
