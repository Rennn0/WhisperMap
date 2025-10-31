import type { Product } from "./types";

const randomWords = ["Elegant", "Cozy", "Rustic", "Minimal", "Vibrant", "Sleek", "Modern", "Artisan", "Handmade", "Premium"];
const categories = ["Home", "Clothing", "Art", "Electronics", "Toys", "Beauty"];

const productData: Product[] = Array.from({ length: 112 }, (_, i) => {
    const adjective = randomWords[Math.floor(Math.random() * randomWords.length)];
    const category = categories[Math.floor(Math.random() * categories.length)];

    return {
        id: `${i + 1}`,
        title: `${adjective} ${category} Item #${i + 1}`,
        description: `A ${adjective?.toLowerCase()} ${category?.toLowerCase()} product perfect for your needs.`,
        image: `https://picsum.photos/seed/product-${i + 1}/640/360`,
        price: Math.round((10 + Math.random() * 90) * 100) / 100,
        seller: `Seller ${Math.ceil(Math.random() * 10)}`,
    };
});

export const getProducts = () => new Promise<Product[]>(r => {
    const min = 1000;
    const max = 5000;
    setTimeout(() => r(productData), Math.floor(Math.random() * (max - min + 1) + min));
});

export const getProduct = (id: string) => new Promise<Product>((r, j) =>
    getProducts().then(ps => ps.find(p => p.id == id)).then(p => p ? r(p) : j()));
