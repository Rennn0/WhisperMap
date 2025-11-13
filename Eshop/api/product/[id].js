import { backendUrl, apiKey } from "../variables.js"
export const config = {
    runtime: 'edge',
};

export default async function handler(req) {
    const url = new URL(req.url);
    const id = url.searchParams.get("id");

    const headers = new Headers(req.headers);
    headers.set("x-api-key", apiKey);

    const apiUrl = `${backendUrl}/product/${id}`;
    const apiResponse = await fetch(apiUrl, {
        method: 'GET',
        headers
    });

    return new Response(apiResponse.body, {
        status: apiResponse.status,
        headers: apiResponse.headers
    });
}
