using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Websocket.Client;

namespace TriangularArbitrage
{
    public abstract class BaseTriangularArbitrageFinder
    {
        public abstract string WsUrl { get; }
        public abstract string SubscriptionMessage { get; }
        public string StartCurrency { get; set; }
        public decimal MinProfitRatio { get; set; }

        private WebsocketClient _client;

        public abstract IEnumerable<TradeSymbol> GetPairs(string message);

        internal Task Start(Action<TradeSymbol, TradeSymbol, TradeSymbol, decimal> foundArbitrageAction)
        {
            _client = new WebsocketClient(new Uri(WsUrl));
            _client.ReconnectTimeout = TimeSpan.FromSeconds(30);

            var minProfit = 1 + MinProfitRatio;

            _client.MessageReceived.Subscribe(msg =>
            {
                var pairs = GetPairs(msg.Text);
                var endPairs = pairs.Where(p => p.QuoteSymbol == StartCurrency || p.BaseSymbol == StartCurrency);

                foreach (var pair1 in endPairs)
                {
                    foreach (var pair2 in pairs.Where(p =>
                        p.BaseSymbol != StartCurrency
                        && p.QuoteSymbol != StartCurrency
                        &&
                        (
                            p.BaseSymbol == pair1.BaseSymbol
                            || p.BaseSymbol == pair1.QuoteSymbol
                            || p.QuoteSymbol == pair1.BaseSymbol
                            || p.QuoteSymbol == pair1.QuoteSymbol
                        )))
                    {
                        foreach (var pair3 in endPairs.Where(p =>
                            p.Pair != pair1.Pair
                            && ((p.BaseSymbol == StartCurrency
                                && (p.QuoteSymbol == pair2.QuoteSymbol || p.BaseSymbol == pair2.QuoteSymbol))
                            || (pair1.QuoteSymbol == StartCurrency
                                && (p.BaseSymbol == pair2.QuoteSymbol || p.BaseSymbol == pair2.QuoteSymbol))))
                        )
                        {
                            var price1 = (1 - pair1.CommissionRatio) * (pair1.QuoteSymbol == StartCurrency ? 1 / pair1.Ask : pair1.Bid);
                            var price2 = (1 - pair2.CommissionRatio) * (pair1.BaseSymbol == pair2.BaseSymbol ? pair2.Bid : pair2.Ask);
                            var price3 = (1 - pair3.CommissionRatio) * (pair3.BaseSymbol == StartCurrency ? 1 / pair3.Ask : pair3.Bid);

                            var endProfitRatio = price1 * price2 * price3;

                            if (endProfitRatio > minProfit)
                            {
                                foundArbitrageAction(pair1, pair2, pair3, endProfitRatio);
                            }
                        }
                    }
                }
            });

            if (!string.IsNullOrEmpty(SubscriptionMessage))
            {
                Task.Run(() => _client.Send(SubscriptionMessage)).Wait();
            }

            _client.Start().Wait();

            return Task.Delay(-1);
        }

        internal void Stop()
        {
            _client.Stop(WebSocketCloseStatus.NormalClosure, "close");
            _client.Dispose();
        }
    }
}
