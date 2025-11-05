export const config = {
    runtime: 'nodejs',
};

export default async function handler(req, res) {
    try {
        const backendUrl = process.env.BACKEND_URL || 'localhost:5158';

        const apiResponse = await fetch(`${backendUrl}/session`);
        const data = await apiResponse.json();

        return res.status(apiResponse.status).json({ data });
    } catch (error) {
        return res.status(500).json({ error });
    }
}
