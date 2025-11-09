export const config = {
    runtime: 'edge',
};

export default async function handler(req) {
    const backendUrl = process.env.BACKEND_URL;
    const apiKey = process.env.CLIENT_API_KEY;
    // const backendUrl = 'http://localhost:5158';
    // const apiKey = 'dev';
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
    headers.set('Cache-Control', 'public, s-maxage=300, stale-while-revalidate=120');

    return new Response(bodyText, {
        status: apiResponse.status,
        headers,
    });
}
