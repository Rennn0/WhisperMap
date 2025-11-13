import { backendUrl, apiKey } from "./variables.js";

export const config = {
    runtime: "edge",
};

export default async function handler(req) {
    const ip =
        req.headers.get("x-real-ip") ||
        req.headers.get("x-forwarded-for")?.split(",")[0].trim() ||
        "";

    const headers = new Headers({
        "x-public-ip": ip,
        "x-api-key": apiKey,
    });

    const apiUrl = `${backendUrl}/session`;
    const apiResponse = await fetch(apiUrl, {
        method: "GET",
        headers,
    });

    return new Response(apiResponse.body, {
        status: apiResponse.status,
        headers: apiResponse.headers,
    });
}