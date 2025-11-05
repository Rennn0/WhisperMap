export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {

    const backendUrl = process.env.BACKEND_URL || 'localhost:5158';
    const apiKey = process.env.CLIENT_API_KEY || 'dev';

    const apiResponse = await fetch(`${backendUrl}/storage?folder=photo`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'X-Api-Key': apiKey
        }
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
