import { backendUrl, apiKey } from "./variables.js";

export const config = {
    runtime: "edge",
};

export default async function handler(req) {
    const headers = new Headers(req.headers);
    headers.set("x-api-key", apiKey);

    const apiUrl=`${backendUrl}/product`;
    const apiResponse = await fetch(apiUrl, {
        method: "POST",
        headers,
        body: req.body,
    });

    return new Response(apiResponse.body, {
        status: apiResponse.status,
        headers: apiResponse.headers
    });
}
