export const imgUrlTransformer = (url?: string, mode: "grid" | "thumb" | "main" = "thumb") => {
    if (!url) return;
    const u = new URL(url);
    const base = "/cdn-cgi/image";
    const params = {
        grid: 'width=400,quality=70,fit=cover,format=auto',
        thumb: 'width=140,height=140,fit=cover,quality=70,format=auto',
        main: 'width=1400,quality=85,fit=contain,format=auto'
    }[mode];

    return `${u.origin}${base}/${params}${u.pathname}`
}