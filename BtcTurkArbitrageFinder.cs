using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TriangularArbitrage
{
    internal class BtcTurkArbitrageFinder : BaseTriangularArbitrageFinder
    {
        class BtcTurkPair
        {
            public string PS { get; set; }
            public string DS { get; set; }
            public string NS { get; set; }
            public decimal A { get; set; }
            public decimal B { get; set; }
        }

        class BtcTurkPairs
        {
            public IEnumerable<BtcTurkPair> Items { get; set; }
        }

        private decimal _fiatCommissionRatio => 0.001m;
        private decimal _cryptoCommissionRatio => 0.0005m;
        private List<string> _fiatList = new List<string> { "TRY", "USD", "EUR" };

        public override string WsUrl => "wss://ws-feed-pro.btcturk.com";

        public override string SubscriptionMessage => @"[151,{""type"":151,""channel"":""ticker"",event:""all"",""join"":true}]";

        public BtcTurkArbitrageFinder(string startCurrency, decimal minProfit)
        {
            StartCurrency = startCurrency;
            MinProfitRatio = minProfit;
        }

        public override IEnumerable<TradeSymbol> GetPairs(string message)
        {
            if (JArray.Parse(message) is var ja && ja.ElementAt(0).Value<int>() == 401)
            {
                return ja.ElementAt(1)
                    .ToObject<BtcTurkPairs>()
                    .Items
                    .Select(btp => new TradeSymbol
                    {
                        Pair = btp.PS,
                        BaseSymbol = btp.NS,
                        QuoteSymbol = btp.DS,
                        Ask = btp.A,
                        Bid = btp.B,
                        CommissionRatio = _fiatList.Any(fc => fc == btp.NS || fc == btp.DS) ? _fiatCommissionRatio : _cryptoCommissionRatio
                    });
            }

            return Enumerable.Empty<TradeSymbol>();
        }
    }
}
