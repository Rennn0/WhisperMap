import { backendUrl, apiKey } from "./variables.js"
export const config = {
    runtime: 'edge',
};

export default async function handler(req) {
    const url = new URL(req.url);
    const query = url.searchParams.get("q") || "";

    const headers = new Headers(req.headers);
    headers.set("x-api-key", apiKey);

    const apiUrl = `${backendUrl}/product?query=${encodeURIComponent(query)}`;
    const apiResponse = await fetch(apiUrl, {
        method: 'GET',
        headers
    });

    if (apiResponse.ok)
        apiResponse.headers.set('Cache-Control', 'public, s-maxage=120, stale-while-revalidate=20');

    return new Response(apiResponse.body, {
        status: apiResponse.status,
        headers: apiResponse.headers,
    });
}
