using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StanLeeBot.Web.Models
{
    public class GoogleSearchResponse
    {
        [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
        public string Kind { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Url Url { get; set; }

        [JsonProperty("queries", NullValueHandling = NullValueHandling.Ignore)]
        public Queries Queries { get; set; }

        [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
        public Context Context { get; set; }

        [JsonProperty("searchInformation", NullValueHandling = NullValueHandling.Ignore)]
        public SearchInformation SearchInformation { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<Item> Items { get; set; }
    }

    public class Context
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
    }

    public class Item
    {
        [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
        public string Kind { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("htmlTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string HtmlTitle { get; set; }

        [JsonProperty("link", NullValueHandling = NullValueHandling.Ignore)]
        public string Link { get; set; }

        [JsonProperty("displayLink", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayLink { get; set; }

        [JsonProperty("snippet", NullValueHandling = NullValueHandling.Ignore)]
        public string Snippet { get; set; }

        [JsonProperty("htmlSnippet", NullValueHandling = NullValueHandling.Ignore)]
        public string HtmlSnippet { get; set; }

        [JsonProperty("cacheId", NullValueHandling = NullValueHandling.Ignore)]
        public string CacheId { get; set; }

        [JsonProperty("formattedUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string FormattedUrl { get; set; }

        [JsonProperty("htmlFormattedUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string HtmlFormattedUrl { get; set; }

        [JsonProperty("pagemap", NullValueHandling = NullValueHandling.Ignore)]
        public PageMap PageMap { get; set; }
    }

    public class PageMap
    {
        [JsonProperty("cse_thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public List<CseThumbnail> CseThumbnail { get; set; }

        [JsonProperty("metatags", NullValueHandling = NullValueHandling.Ignore)]
        public List<MetaTag> MetaTags { get; set; }

        [JsonProperty("cse_image", NullValueHandling = NullValueHandling.Ignore)]
        public List<CseImage> CseImage { get; set; }
    }

    public class CseImage
    {
        [JsonProperty("src", NullValueHandling = NullValueHandling.Ignore)]
        public string Src { get; set; }
    }

    public class CseThumbnail
    {
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public string Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public string Height { get; set; }

        [JsonProperty("src", NullValueHandling = NullValueHandling.Ignore)]
        public string Src { get; set; }
    }

    public class MetaTag
    {
        [JsonProperty("og:site_name", NullValueHandling = NullValueHandling.Ignore)]
        public string OgSiteName { get; set; }

        [JsonProperty("twitter:creator", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterCreator { get; set; }

        [JsonProperty("twitter:site", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterSite { get; set; }

        [JsonProperty("og:url", NullValueHandling = NullValueHandling.Ignore)]
        public string OgUrl { get; set; }

        [JsonProperty("og:type", NullValueHandling = NullValueHandling.Ignore)]
        public string OgType { get; set; }

        [JsonProperty("og:description", NullValueHandling = NullValueHandling.Ignore)]
        public string OgDescription { get; set; }

        [JsonProperty("twitter:description", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterDescription { get; set; }

        [JsonProperty("og:image", NullValueHandling = NullValueHandling.Ignore)]
        public string OgImage { get; set; }

        [JsonProperty("twitter:image", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterImage { get; set; }

        [JsonProperty("twitter:image:alt", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterImageAlt { get; set; }

        [JsonProperty("mobileoptimized", NullValueHandling = NullValueHandling.Ignore)]
        public string MobileOptimized { get; set; }

        [JsonProperty("viewport", NullValueHandling = NullValueHandling.Ignore)]
        public string Viewport { get; set; }

        [JsonProperty("content-type", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty("apple-mobile-web-app-capable", NullValueHandling = NullValueHandling.Ignore)]
        public string AppleMobileWebAppCapable { get; set; }

        [JsonProperty("msapplication-config", NullValueHandling = NullValueHandling.Ignore)]
        public string MsApplicationConfig { get; set; }

        [JsonProperty("msapplication-tilecolor", NullValueHandling = NullValueHandling.Ignore)]
        public string MsApplicationTileColor { get; set; }

        [JsonProperty("msapplication-tileimage", NullValueHandling = NullValueHandling.Ignore)]
        public string MsApplicationTileImage { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("twitter:card", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterCard { get; set; }

        [JsonProperty("og:title", NullValueHandling = NullValueHandling.Ignore)]
        public string OgTitle { get; set; }

        [JsonProperty("twitter:title", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterTitle { get; set; }

        [JsonProperty("twitter:url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri TwitterUrl { get; set; }

        [JsonProperty("marvel:contentid", NullValueHandling = NullValueHandling.Ignore)]
        public string MarvelContentId { get; set; }

        [JsonProperty("marvel:contenttype", NullValueHandling = NullValueHandling.Ignore)]
        public string MarvelContentType { get; set; }

        [JsonProperty("og:image:alt", NullValueHandling = NullValueHandling.Ignore)]
        public string OgImageAlt { get; set; }

        [JsonProperty("fb:app_id", NullValueHandling = NullValueHandling.Ignore)]
        public string FbAppId { get; set; }

        [JsonProperty("theme-color", NullValueHandling = NullValueHandling.Ignore)]
        public string ThemeColor { get; set; }
    }

    public class Queries
    {
        [JsonProperty("request", NullValueHandling = NullValueHandling.Ignore)]
        public List<NextPage> Request { get; set; }

        [JsonProperty("nextPage", NullValueHandling = NullValueHandling.Ignore)]
        public List<NextPage> NextPage { get; set; }
    }

    public class NextPage
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("totalResults", NullValueHandling = NullValueHandling.Ignore)]
        public string TotalResults { get; set; }

        [JsonProperty("searchTerms", NullValueHandling = NullValueHandling.Ignore)]
        public string SearchTerms { get; set; }

        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public int Count { get; set; }

        [JsonProperty("startIndex", NullValueHandling = NullValueHandling.Ignore)]
        public int StartIndex { get; set; }

        [JsonProperty("inputEncoding", NullValueHandling = NullValueHandling.Ignore)]
        public string InputEncoding { get; set; }

        [JsonProperty("outputEncoding", NullValueHandling = NullValueHandling.Ignore)]
        public string OutputEncoding { get; set; }

        [JsonProperty("safe", NullValueHandling = NullValueHandling.Ignore)]
        public string Safe { get; set; }

        [JsonProperty("cx", NullValueHandling = NullValueHandling.Ignore)]
        public string Cx { get; set; }
    }

    public class SearchInformation
    {
        [JsonProperty("searchTime", NullValueHandling = NullValueHandling.Ignore)]
        public double? SearchTime { get; set; }

        [JsonProperty("formattedSearchTime", NullValueHandling = NullValueHandling.Ignore)]
        public string FormattedSearchTime { get; set; }

        [JsonProperty("totalResults", NullValueHandling = NullValueHandling.Ignore)]
        public string TotalResults { get; set; }

        [JsonProperty("formattedTotalResults", NullValueHandling = NullValueHandling.Ignore)]
        public string FormattedTotalResults { get; set; }
    }

    public class Url
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("template", NullValueHandling = NullValueHandling.Ignore)]
        public string Template { get; set; }
    }
}
