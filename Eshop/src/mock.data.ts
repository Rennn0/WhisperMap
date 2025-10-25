import type { Product } from "./types";

const randomWords = ["Elegant", "Cozy", "Rustic", "Minimal", "Vibrant", "Sleek", "Modern", "Artisan", "Handmade", "Premium"];
const categories = ["Home", "Clothing", "Art", "Electronics", "Toys", "Beauty"];

export const productData: Product[] = Array.from({ length: 112 }, (_, i) => {
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
    