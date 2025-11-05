export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    try {
        const backendUrl = process.env.BACKEND_URL || 'localhost:5158';
        const apiKey = process.env.CLIENT_API_KEY || 'dev';

        const apiResponse = await fetch(`${backendUrl}/storage?folder=photo`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey
            }
        });
        const data = await apiResponse.json();

        return res.status(apiResponse.status).json({ data });
    } catch (error) {
        return res.status(500).json({ error });
    }
}
