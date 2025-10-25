import type { Product } from "./types";

export const productData = Array.from({ length: 12 }).map((_, i): Product => ({
    id: `${i + 1}`,
    title: `#${i + 1}`,
    description:
        'Short description of the item for sale',
    image: `https://picsum.photos/seed/product-${i + 1}/640/360`,
    price: Math.round((10 + Math.random() * 90) * 100) / 100,
    seller: `Seller ${Math.ceil(Math.random() * 10)}`,
}));