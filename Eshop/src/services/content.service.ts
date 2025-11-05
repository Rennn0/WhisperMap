export const getPhotos = async (): Promise<string[] | null> => {
    const response = await fetch(`/api/photos`, { method: "GET", credentials: "include" });
    const data = await response.json();
    return data || null;
}