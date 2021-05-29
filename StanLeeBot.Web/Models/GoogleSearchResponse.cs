using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StanLeeBot.Web.Models
{
    public static class GoogleSearchResponse
    {
        public class SearchResponse
        {
            [JsonProperty("kind", NullValueHandling = NullValueHandling.Include)]
            public string Kind { get; set; }

            [JsonProperty("url", NullValueHandling = NullValueHandling.Include)]
            public Url Url { get; set; }

            [JsonProperty("queries", NullValueHandling = NullValueHandling.Include)]
            public Queries Queries { get; set; }

            [JsonProperty("context", NullValueHandling = NullValueHandling.Include)]
            public Context Context { get; set; }

            [JsonProperty("searchInformation", NullValueHandling = NullValueHandling.Include)]
            public SearchInformation SearchInformation { get; set; }

            [JsonProperty("items", NullValueHandling = NullValueHandling.Include)]
            public List<Item> Items { get; set; }
        }

        public class Context
        {
            [JsonProperty("title", NullValueHandling = NullValueHandling.Include)]
            public string Title { get; set; }
        }

        public class Item
        {
            [JsonProperty("kind", NullValueHandling = NullValueHandling.Include)]
            public string Kind { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Include)]
            public string Title { get; set; }

            [JsonProperty("htmlTitle", NullValueHandling = NullValueHandling.Include)]
            public string HtmlTitle { get; set; }

            [JsonProperty("link", NullValueHandling = NullValueHandling.Include)]
            public string Link { get; set; }

            [JsonProperty("displayLink", NullValueHandling = NullValueHandling.Include)]
            public string DisplayLink { get; set; }

            [JsonProperty("snippet", NullValueHandling = NullValueHandling.Include)]
            public string Snippet { get; set; }

            [JsonProperty("htmlSnippet", NullValueHandling = NullValueHandling.Include)]
            public string HtmlSnippet { get; set; }

            [JsonProperty("cacheId", NullValueHandling = NullValueHandling.Include)]
            public string CacheId { get; set; }

            [JsonProperty("formattedUrl", NullValueHandling = NullValueHandling.Include)]
            public string FormattedUrl { get; set; }

            [JsonProperty("htmlFormattedUrl", NullValueHandling = NullValueHandling.Include)]
            public string HtmlFormattedUrl { get; set; }

            [JsonProperty("pagemap", NullValueHandling = NullValueHandling.Include)]
            public PageMap PageMap { get; set; }
        }

        public class PageMap
        {
            [JsonProperty("cse_thumbnail", NullValueHandling = NullValueHandling.Include)]
            public List<CseThumbnail> CseThumbnail { get; set; }

            [JsonProperty("metatags", NullValueHandling = NullValueHandling.Include)]
            public List<MetaTag> MetaTags { get; set; }

            [JsonProperty("cse_image", NullValueHandling = NullValueHandling.Include)]
            public List<CseImage> CseImage { get; set; }
        }

        public class CseImage
        {
            [JsonProperty("src", NullValueHandling = NullValueHandling.Include)]
            public string Src { get; set; }
        }

        public class CseThumbnail
        {
            [JsonProperty("width", NullValueHandling = NullValueHandling.Include)]
            public string Width { get; set; }

            [JsonProperty("height", NullValueHandling = NullValueHandling.Include)]
            public string Height { get; set; }

            [JsonProperty("src", NullValueHandling = NullValueHandling.Include)]
            public string Src { get; set; }
        }

        public class MetaTag
        {
            [JsonProperty("og:site_name", NullValueHandling = NullValueHandling.Include)]
            public string OgSiteName { get; set; }

            [JsonProperty("twitter:creator", NullValueHandling = NullValueHandling.Include)]
            public string TwitterCreator { get; set; }

            [JsonProperty("twitter:site", NullValueHandling = NullValueHandling.Include)]
            public string TwitterSite { get; set; }

            [JsonProperty("og:url", NullValueHandling = NullValueHandling.Include)]
            public string OgUrl { get; set; }

            [JsonProperty("og:type", NullValueHandling = NullValueHandling.Include)]
            public string OgType { get; set; }

            [JsonProperty("og:description", NullValueHandling = NullValueHandling.Include)]
            public string OgDescription { get; set; }

            [JsonProperty("twitter:description", NullValueHandling = NullValueHandling.Include)]
            public string TwitterDescription { get; set; }

            [JsonProperty("og:image", NullValueHandling = NullValueHandling.Include)]
            public string OgImage { get; set; }

            [JsonProperty("twitter:image", NullValueHandling = NullValueHandling.Include)]
            public string TwitterImage { get; set; }

            [JsonProperty("twitter:image:alt", NullValueHandling = NullValueHandling.Include)]
            public string TwitterImageAlt { get; set; }

            [JsonProperty("mobileoptimized", NullValueHandling = NullValueHandling.Include)]
            public string MobileOptimized { get; set; }

            [JsonProperty("viewport", NullValueHandling = NullValueHandling.Include)]
            public string Viewport { get; set; }

            [JsonProperty("content-type", NullValueHandling = NullValueHandling.Include)]
            public string ContentType { get; set; }

            [JsonProperty("apple-mobile-web-app-capable", NullValueHandling = NullValueHandling.Include)]
            public string AppleMobileWebAppCapable { get; set; }

            [JsonProperty("msapplication-config", NullValueHandling = NullValueHandling.Include)]
            public string MsApplicationConfig { get; set; }

            [JsonProperty("msapplication-tilecolor", NullValueHandling = NullValueHandling.Include)]
            public string MsApplicationTileColor { get; set; }

            [JsonProperty("msapplication-tileimage", NullValueHandling = NullValueHandling.Include)]
            public string MsApplicationTileImage { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Include)]
            public string Title { get; set; }

            [JsonProperty("twitter:card", NullValueHandling = NullValueHandling.Include)]
            public string TwitterCard { get; set; }

            [JsonProperty("og:title", NullValueHandling = NullValueHandling.Include)]
            public string OgTitle { get; set; }

            [JsonProperty("twitter:title", NullValueHandling = NullValueHandling.Include)]
            public string TwitterTitle { get; set; }

            [JsonProperty("twitter:url", NullValueHandling = NullValueHandling.Include)]
            public Uri TwitterUrl { get; set; }

            [JsonProperty("marvel:contentid", NullValueHandling = NullValueHandling.Include)]
            public string MarvelContentId { get; set; }

            [JsonProperty("marvel:contenttype", NullValueHandling = NullValueHandling.Include)]
            public string MarvelContentType { get; set; }

            [JsonProperty("og:image:alt", NullValueHandling = NullValueHandling.Include)]
            public string OgImageAlt { get; set; }

            [JsonProperty("fb:app_id", NullValueHandling = NullValueHandling.Include)]
            public string FbAppId { get; set; }

            [JsonProperty("theme-color", NullValueHandling = NullValueHandling.Include)]
            public string ThemeColor { get; set; }
        }

        public class Queries
        {
            [JsonProperty("request", NullValueHandling = NullValueHandling.Include)]
            public List<NextPage> Request { get; set; }

            [JsonProperty("nextPage", NullValueHandling = NullValueHandling.Include)]
            public List<NextPage> NextPage { get; set; }
        }

        public class NextPage
        {
            [JsonProperty("title", NullValueHandling = NullValueHandling.Include)]
            public string Title { get; set; }

            [JsonProperty("totalResults", NullValueHandling = NullValueHandling.Include)]
            public string TotalResults { get; set; }

            [JsonProperty("searchTerms", NullValueHandling = NullValueHandling.Include)]
            public string SearchTerms { get; set; }

            [JsonProperty("count", NullValueHandling = NullValueHandling.Include)]
            public int Count { get; set; }

            [JsonProperty("startIndex", NullValueHandling = NullValueHandling.Include)]
            public int StartIndex { get; set; }

            [JsonProperty("inputEncoding", NullValueHandling = NullValueHandling.Include)]
            public string InputEncoding { get; set; }

            [JsonProperty("outputEncoding", NullValueHandling = NullValueHandling.Include)]
            public string OutputEncoding { get; set; }

            [JsonProperty("safe", NullValueHandling = NullValueHandling.Include)]
            public string Safe { get; set; }

            [JsonProperty("cx", NullValueHandling = NullValueHandling.Include)]
            public string Cx { get; set; }
        }

        public class SearchInformation
        {
            [JsonProperty("searchTime", NullValueHandling = NullValueHandling.Include)]
            public double? SearchTime { get; set; }

            [JsonProperty("formattedSearchTime", NullValueHandling = NullValueHandling.Include)]
            public string FormattedSearchTime { get; set; }

            [JsonProperty("totalResults", NullValueHandling = NullValueHandling.Include)]
            public string TotalResults { get; set; }

            [JsonProperty("formattedTotalResults", NullValueHandling = NullValueHandling.Include)]
            public string FormattedTotalResults { get; set; }
        }

        public class Url
        {
            [JsonProperty("type", NullValueHandling = NullValueHandling.Include)]
            public string Type { get; set; }

            [JsonProperty("template", NullValueHandling = NullValueHandling.Include)]
            public string Template { get; set; }
        }
    }


}
