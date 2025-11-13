import { backendUrl, apiKey } from "./variables.js"
export const config = {
    runtime: 'edge',
};

export default async function handler(req) {
    const headers = new Headers(req.headers);
    headers.set("x-api-key", apiKey);

    const apiUrl = `${backendUrl}/session/me`;
    const apiResponse = await fetch(apiUrl, {
        method: 'GET',
        headers,
        credentials: "include"
    });

    return new Response(apiResponse.body, {
        status: apiResponse.status,
        headers: apiResponse.headers,
    });
}
