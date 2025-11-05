export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    try {
        // const backendUrl = process.env.BACKEND_URL;
        // const apiKey = process.env.CLIENT_API_KEY;

        const backendUrl = 'https://whispermap-flkg.onrender.com'
        const apiKey = 'xui'

        const apiResponse = await fetch(`${backendUrl}/storage?folder=photo`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-Api-Key': apiKey
            }
        });
        const data = await apiResponse.json();

        return res.status(apiResponse.status).json(data);
    } catch (error) {
        return res.status(500).json({ error: error.message });
    }
}
