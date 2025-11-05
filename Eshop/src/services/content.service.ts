export const getPhotos = async (): Promise<string[] | null> => {
    const response = await fetch(`/api/photos`);
    const data = await response.json();
    return data || null;
}