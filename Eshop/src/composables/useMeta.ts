/**
 * Composable for managing meta tags dynamically
 * Helps with per-page SEO optimization
 */

export interface MetaTagConfig {
  title?: string;
  description?: string;
  keywords?: string;
  ogTitle?: string;
  ogDescription?: string;
  ogImage?: string;
  ogType?: string;
  canonical?: string;
  twitterTitle?: string;
  twitterDescription?: string;
  twitterImage?: string;
}

/**
 * Update meta tags for the current page
 * @param config - Meta tag configuration
 */
export function useMeta(config: MetaTagConfig): void {
  // Update title
  if (config.title) {
    document.title = config.title;
  }

  // Update or create meta tags
  updateMetaTag('name', 'description', config.description);
  updateMetaTag('name', 'keywords', config.keywords);
  updateMetaTag('property', 'og:title', config.ogTitle || config.title);
  updateMetaTag('property', 'og:description', config.ogDescription || config.description);
  updateMetaTag('property', 'og:image', config.ogImage || 'https://xati.org/appicon.svg');
  updateMetaTag('property', 'og:type', config.ogType || 'website');
  updateMetaTag('name', 'twitter:title', config.twitterTitle || config.title);
  updateMetaTag('name', 'twitter:description', config.twitterDescription || config.description);
  updateMetaTag('name', 'twitter:image', config.twitterImage || config.ogImage || 'https://xati.org/appicon.svg');

  // Update canonical URL
  if (config.canonical) {
    const canonical = document.querySelector('link[rel="canonical"]') as HTMLLinkElement;
    if (canonical) {
      canonical.href = config.canonical;
    } else {
      const link = document.createElement('link');
      link.rel = 'canonical';
      link.href = config.canonical;
      document.head.appendChild(link);
    }
  }
}

/**
 * Update or create a meta tag
 */
function updateMetaTag(attribute: string, name: string, content?: string): void {
  if (!content) return;

  const selector = `meta[${attribute}="${name}"]`;
  let meta = document.querySelector(selector) as HTMLMetaElement;

  if (!meta) {
    meta = document.createElement('meta');
    meta.setAttribute(attribute, name);
    document.head.appendChild(meta);
  }

  meta.content = content;
}

/**
 * Reset meta tags to defaults
 */
export function resetMeta(): void {
  useMeta({
    title: 'Xati - Premium Online Marketplace | Shop Quality Products',
    description: 'Discover premium quality products on Xati marketplace. Browse our curated collection, enjoy secure shopping, and fast delivery.',
    keywords: 'xati, online shopping, marketplace, products, quality items, e-commerce',
    ogType: 'website',
  });
}
