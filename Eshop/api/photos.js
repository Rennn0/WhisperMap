export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    const backendUrl = process.env.BACKEND_URL;
    const apiKey = process.env.CLIENT_API_KEY;
    // const backendUrl = 'http://localhost:5158';
    // const apiKey = 'dev';
    const cookies = req.headers.cookie;
    const apiResponse = await fetch(`${backendUrl}/storage?folder=photo`, {
        method: 'GET',
        headers: {
            'content-type': 'application/json',
            'x-api-key': apiKey,
            ...(cookies ? { 'cookie': cookies } : {})
        },
        credentials: "include"
    });
    const bodyText = await apiResponse.text();
    let data;
    try {
        data = JSON.parse(bodyText);
    } catch (error) {
        data = bodyText;
    }
    return res.status(apiResponse.status).send(data);
}
