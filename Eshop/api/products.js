import { backendUrl, apiKey } from "./variables.js"
export const config = {
    runtime: 'edge',
};

export default async function handler(req) {
    const cookies = req.headers.get("cookie") || "";
    const apiResponse = await fetch(`${backendUrl}/product`, {
        method: 'GET',
        headers: {
            'content-type': 'application/json',
            'x-api-key': apiKey,
            ...(cookies ? { 'cookie': cookies } : {})
        },
        credentials: "include"
    });
    const bodyText = await apiResponse.text();
    const headers = new Headers(apiResponse.headers);
    // if (apiResponse.ok) {
    //     headers.set('Cache-Control', 'public, s-maxage=300, stale-while-revalidate=120');
    // } else {
    //     headers.set('Cache-Control', 'no-store');
    // }

    return new Response(bodyText, {
        status: apiResponse.status,
        headers,
    });
}
