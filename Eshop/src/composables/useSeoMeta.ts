interface SeoMetaOptions {
  title?: string
  description?: string
  image?: string
  url?: string
  type?: string
  productPrice?: number
  productCurrency?: string
  canonical?: string
  productSchema?: object
}

export function useSeoMeta() {
  const setSeoMeta = (options: SeoMetaOptions) => {
    const {
      title = 'Xati - ხელნაკეთი საჩუქრების ზარდახშა',
      description = 'Shop premium handcrafted gifts and artisan items. Authentic handmade Easter eggs, decorative chests, and unique handcrafted gifts.',
      image = `${window.location.origin}/appicon.svg`,
      url = window.location.href,
      type = 'website',
      canonical = url,
      productSchema = null,
    } = options

    // Update document title
    document.title = title

    // Update or create meta tags
    updateMetaTag('name', 'description', description)
    updateMetaTag('property', 'og:title', title)
    updateMetaTag('property', 'og:description', description)
    updateMetaTag('property', 'og:image', image)
    updateMetaTag('property', 'og:url', url)
    updateMetaTag('property', 'og:type', type)
    updateMetaTag('name', 'twitter:title', title)
    updateMetaTag('name', 'twitter:description', description)
    updateMetaTag('name', 'twitter:image', image)
    updateMetaTag('name', 'twitter:card', 'summary_large_image')

    // Update canonical link
    updateCanonicalTag(canonical)

    // Update structured data if provided
    if (productSchema) {
      updateStructuredData(productSchema)
    }

    // Add language meta tag
    updateMetaTag('http-equiv', 'content-language', 'en')
  }

  const updateMetaTag = (
    attribute: 'name' | 'property' | 'http-equiv',
    attrValue: string,
    content: string
  ) => {
    let tag = document.querySelector(`meta[${attribute}="${attrValue}"]`)

    if (!tag) {
      tag = document.createElement('meta')
      tag.setAttribute(attribute, attrValue)
      document.head.appendChild(tag)
    }

    tag.setAttribute('content', content)
  }

  const updateCanonicalTag = (url: string) => {
    let canonicalTag = document.querySelector('link[rel="canonical"]') as HTMLLinkElement
    if (!canonicalTag) {
      canonicalTag = document.createElement('link')
      canonicalTag.rel = 'canonical'
      document.head.appendChild(canonicalTag)
    }
    canonicalTag.href = url
  }

  const updateStructuredData = (schema: object) => {
    // Remove old structured data if exists
    const oldScript = document.querySelector('script[type="application/ld+json"]')
    if (oldScript) {
      oldScript.remove()
    }

    // Add new structured data
    const script = document.createElement('script')
    script.type = 'application/ld+json'
    script.textContent = JSON.stringify(schema)
    document.head.appendChild(script)
  }

  const setProductSeoMeta = (product: {
    id: number | string
    title: string
    description: string
    price?: number
    resources?: string[]
  }) => {
    const productUrl = `${window.location.origin}/${product.id}`
    const productImage = product.resources?.[0] || `${window.location.origin}/appicon.svg`

    const productSchema = {
      '@context': 'https://schema.org/',
      '@type': 'Product',
      name: product.title,
      description: product.description,
      image: productImage,
      url: productUrl,
      ...(product.price && {
        offers: {
          '@type': 'Offer',
          price: product.price.toFixed(2),
          priceCurrency: 'USD',
        },
      }),
    }

    setSeoMeta({
      title: `${product.title} | ხათი - საჩუქრები | Xati`,
      description: `${product.description.substring(0, 160)} | ხელნაკეთი საჩუქრები ხათიდან`,
      image: productImage,
      url: productUrl,
      type: 'product',
      canonical: productUrl,
      productSchema,
    })
  }

  const setHomeSeoMeta = (productCount: number = 0) => {
    const collectionSchema = {
      '@context': 'https://schema.org/',
      '@type': 'CollectionPage',
      name: 'Xati - საჩუქრების ზარდახშა',
      description: 'Premium handcrafted gifts and artisan items. ხათი - საჩუქრების ზარდახშა, ხელნაკეთი საჩუქრები. Shop authentic handmade Easter eggs, decorative chests, and unique handcrafted gifts.',
      url: `${window.location.origin}/`,
      ...(productCount > 0 && {
        numberOfItems: productCount,
      }),
    }

    setSeoMeta({
      title: 'Xati - Handcrafted Gifts & Artisan Items | ხათი - საჩუქრები | Premium Handmade Easter Eggs & Gifts',
      description: 'Discover premium handcrafted gifts and artisan items. ხათი - საჩუქრების ზარდახშა, ხელნაკეთი საჩუქრები. Shop authentic handmade Easter eggs, decorative chests, and unique handcrafted gifts from Xati.',
      image: `${window.location.origin}/appicon.svg`,
      url: `${window.location.origin}/`,
      type: 'website',
      canonical: `${window.location.origin}/`,
      productSchema: collectionSchema,
    })
  }

  return {
    setSeoMeta,
    setProductSeoMeta,
    setHomeSeoMeta,
  }
}
