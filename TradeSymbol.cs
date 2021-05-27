using System.Collections.Generic;

namespace TriangularArbitrage
{
    public class TradeSymbol
    {
        public string Pair { get; set; }
        public string QuoteSymbol { get; set; }
        public string BaseSymbol { get; set; }
        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public decimal CommissionRatio { get; set; }
    }


}
