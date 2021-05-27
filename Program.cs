using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace TriangularArbitrage
{
    class Program
    {
        public class Options
        {
            [Option('u', "url", Required = true, HelpText = "Websocker url")]
            public string WsUrl { get; set; }

            [Option('s', "start-currency", Required = true, HelpText = "Start currency")]
            public string StartCurrency { get; set; }

            [Option('m', "min-profit-ratio", Required = true, HelpText = "Minimum profit ratio")]
            public decimal MinProfitRatio { get; set; }
        }

        static void Main(string[] args)
        {
            var o = Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(e =>
                {
                    e.Output();
                    Environment.Exit(1);
                })
                .Value;

            var af = new BtcTurkArbitrageFinder(o.StartCurrency, o.MinProfitRatio);

            af.Start((pair1, pair2, pair3, endProfitRatio) =>
                    Console.WriteLine($"{pair1.Pair}(A: {pair1.Ask}, B: {pair1.Bid}) > {pair2.Pair}(A: {pair2.Ask}, B: {pair2.Bid}) > {pair3.Pair}(A: {pair3.Ask}, B: {pair3.Bid}): {endProfitRatio}")
            ).Wait();
        }
    }
}
