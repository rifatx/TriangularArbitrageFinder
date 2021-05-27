using System.Collections.Generic;
using RestSharp;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;

namespace TriangularArbitrage
{
    internal class BinanceArbitrageFinder : BaseTriangularArbitrageFinder
    {
        class RestSymbol
        {
            [JsonProperty("symbol")]
            public string Symbol { get; set; }

            [JsonProperty("baseAsset")]
            public string BaseAsset { get; set; }

            [JsonProperty("quoteAsset")]
            public string QuoteAsset { get; set; }
        }

        class ExchangeInfo
        {
            [JsonProperty("symbols")]
            public IEnumerable<RestSymbol> Symbols { get; set; }
        }

        private const string REST_BASE_URL = "https://api.binance.com";

        private Dictionary<string, (string Base, string Quote)> _symbolDict;

        public override string WsUrl => "wss://stream.binance.com:9443/ws/!ticker@arr";

        public override string SubscriptionMessage => null;

        public BinanceArbitrageFinder(string startCurrency, decimal minProfit)
        {
            StartCurrency = startCurrency;
            MinProfitRatio = minProfit;

            var rc = new RestClient(REST_BASE_URL);
            var res = rc.Execute(new RestRequest("api/v3/exchangeInfo", Method.GET));

            _symbolDict = JsonConvert.DeserializeObject<ExchangeInfo>(res.Content)
                .Symbols
                .ToDictionary(s => s.Symbol, s => (s.BaseAsset, s.QuoteAsset));
        }

        public override IEnumerable<TradeSymbol> GetPairs(string message) =>
            JArray.Parse(message).Select(t =>
            {
                var symbol = t.Value<string>("s");

                return new TradeSymbol
                {
                    Pair = symbol,
                    BaseSymbol = _symbolDict[symbol].Base,
                    QuoteSymbol = _symbolDict[symbol].Quote,
                    Ask = t.Value<decimal>("a"),
                    Bid = t.Value<decimal>("b"),
                    CommissionRatio = 0.0075m
                };
            });
    }
}
