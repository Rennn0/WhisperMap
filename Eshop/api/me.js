import { backendUrl } from "./variables.js"
export const config = {
    runtime: 'edge',
};

export default async function handler(req) {
    const cookies = req.headers.get("cookie") || "";
    const apiResponse = await fetch(`${backendUrl}/session/me`, {
        method: 'GET',
        headers: {
            'content-type': 'application/json',
            ...(cookies ? { 'cookie': cookies } : {})
        },
        credentials: "include"
    });
    const bodyText = await apiResponse.text();
    const headers = new Headers(apiResponse.headers);
    return new Response(bodyText, {
        status: apiResponse.status,
        headers,
    });
}
