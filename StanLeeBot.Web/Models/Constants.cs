namespace StanLeeBot.Web.Models
{
    public struct Constants
    {
        public const string StanLeeSlackStateCookieName = "StanLeeSlackState";

        public struct DialogFlow
        {
            public const string HelloMarvelLookupIntentId = "92fd80e1-0526-45ed-bb3f-e67f3cb929cb";
            public const string HelloDCLookupIntentId = "78407ec9-cc76-4574-b8f1-a954f55150c0";
            public const string HelloShortenUrlIntentId = "9d3458c7-d50d-494e-9d14-f98d33fa2dfa";
        }

        public struct UrlShortenerDomains
        {
            public const string MrvlCo = "mrvlco";
            public const string XMenTo = "xmento";
        }
    }
}
