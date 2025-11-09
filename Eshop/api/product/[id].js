export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    const backendUrl = process.env.BACKEND_URL;
    const apiKey = process.env.CLIENT_API_KEY;
    // const backendUrl = 'http://localhost:5158';
    // const apiKey = 'dev';
    const cookies = req.headers.cookie;
    const { id } = req.query
    const apiResponse = await fetch(`${backendUrl}/product/${id}`, {
        method: 'GET',
        headers: {
            'content-type': 'application/json',
            'x-api-key': apiKey,
            ...(cookies ? { 'cookie': cookies } : {})
        },
        credentials: "include"
    });
    const bodyText = await apiResponse.text();
    return res.status(apiResponse.status).send(bodyText);
}
