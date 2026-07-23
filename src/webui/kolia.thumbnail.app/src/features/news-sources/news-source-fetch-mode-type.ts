// =============================================================================
// CSourceFetchMode — mirror BE enum
// =============================================================================

export const CSourceFetchMode = {
  /// <summary>Fetch RSS/Atom feed trực tiếp từ URL nguồn</summary>
  RssDirect: 1,
  /// <summary>RSS trực tiếp trước, nếu thất bại thì tự động thử Google News RSS</summary>
  GoogleNewsFallback: 2,
  /// <summary>Bỏ qua RSS, sử dụng Google News RSS site-restricted, fallback Sitemap</summary>
  GoogleNewsSiteRestricted: 3,
  /// <summary>Sử dụng sitemap.xml (Tier 3)</summary>
  SitemapFallback: 4,
  /// <summary>Sitemap hoặc Google News</summary>
  SitemapOrGoogleNews: 5,
  /// <summary>Custom scraper riêng</summary>
  Custom: 6,
  /// <summary>REST API — gọi HTTP GET đến ApiEndpoint, parse JSON response</summary>
  RestApi: 7,
} as const

export type CSourceFetchMode = (typeof CSourceFetchMode)[keyof typeof CSourceFetchMode]

export const SOURCE_FETCH_MODE_OPTIONS: { id: CSourceFetchMode; label: string }[] = [
  { id: CSourceFetchMode.RssDirect, label: 'RSS trực tiếp' },
  { id: CSourceFetchMode.GoogleNewsFallback, label: 'RSS + Google News fallback' },
  { id: CSourceFetchMode.GoogleNewsSiteRestricted, label: 'Google News site-restricted' },
  { id: CSourceFetchMode.SitemapFallback, label: 'Sitemap' },
  { id: CSourceFetchMode.SitemapOrGoogleNews, label: 'Sitemap hoặc Google News' },
  { id: CSourceFetchMode.Custom, label: 'Custom scraper' },
  { id: CSourceFetchMode.RestApi, label: 'REST API' },
]

export function getSourceFetchModeLabel(v: number): string | undefined {
  return SOURCE_FETCH_MODE_OPTIONS.find((o) => o.id === v)?.label
}
