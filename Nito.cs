namespace ATAS.Indicators.Technical
{
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Intrinsics.X86;
    using System.Windows.Media;
    using ATAS.Indicators;
    using OFT.Rendering.Settings;
    using Utils.Common;
    using Utils.Common.Logging;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using ATAS.Indicators.Technical.Properties;
    using OFT.Attributes;
    using Utils.Common.Localization;

    public class Nito : Indicator
    {
        private ValueDataSeries MiddleBand = new ValueDataSeries("Middleband")
        {
            VisualType = VisualMode.Line,
            LineDashStyle = LineDashStyle.Solid,
            Color = Colors.CadetBlue
        };

        private ValueDataSeries UpperBand = new ValueDataSeries("Upperband")
        {
            VisualType = VisualMode.Line,
            LineDashStyle = LineDashStyle.Solid,
            Color = Colors.Green
        };

        private ValueDataSeries LowerBand = new ValueDataSeries("Lowerband")
        {
            VisualType = VisualMode.Line,
            LineDashStyle = LineDashStyle.Solid,
            Color = Colors.Red
        };

        private readonly WMA Wma = new WMA();

        // Using the premade ATR class like this is what I want to do but using Atr.Calculate() errors
        // with: ATAS.Indicators.Technical.Nito OnCalculate error Specified method is not supported.
        //private readonly ATR Atr = new ATR();

        // so instead I store the raw atr data here and do the logic inline, that doesn't error
        // why? Probably because a new calculation requires access to results of previous calculations, stored here in AtrSeries,
        // and it freaks out when that data doesn't exist when you call .Calculate ???
        // Maybe, that's what I assume is happening. Pity the class way doesn't work, would be so much cleaner/better.
        private ValueDataSeries AtrSeries = new ValueDataSeries("ATR");
        private ValueDataSeries MultipledAtrSeries = new ValueDataSeries("MultipliedATR");

        public int WmaPeriod
        {
            get
            {
                return this.Wma.Period;
            }
            set
            {
                this.Wma.Period = value;
                RecalculateValues();
            }
        }

        public int AtrPeriod
        {
            get
            {
                return this.AtrPeriod;
            }
            set
            {
                this.AtrPeriod = value;
                RecalculateValues();
            }
        }

        public decimal AtrMultiplier
        {
            get
            {
                return this.AtrMultiplier;
            }
            set
            {
                this.AtrMultiplier = value;
                RecalculateValues();
            }
        }

        public Nito()
        {
            // location to display this indicator, on the panel where the bars are (the chart) or a new panel
            Panel = IndicatorDataProvider.CandlesPanel;

            this.WmaPeriod = 20;
            this.AtrPeriod = 20;
            this.AtrMultiplier = 5m;

            // add upper and lower bands
            base.DataSeries.Add(MiddleBand);
            base.DataSeries.Add(UpperBand);
            base.DataSeries.Add(LowerBand);
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            decimal wma = this.Wma.Calculate(bar, value);

            // this is how I want to do it, but it causes errors that have an unknown reason since copying code works fine
            //decimal atr = this.Atr.Calculate(bar, value);

            if (bar == 0 && ChartInfo != null)
            ((ValueDataSeries)DataSeries[0]).StringFormat = ChartInfo.StringFormat;

            var candle = GetCandle(bar);
            var high0 = candle.High;
            var low0 = candle.Low;

            if (bar == 0)
                this.AtrSeries[bar] = high0 - low0;
            else
            {
                var close1 = GetCandle(bar - 1).Close;
                var trueRange = Math.Max(Math.Abs(low0 - close1), Math.Max(high0 - low0, Math.Abs(high0 - close1)));
                this.AtrSeries[bar] = ((Math.Min(CurrentBar + 1, this.AtrPeriod) - 1) * this.AtrSeries[bar - 1] + trueRange) / Math.Min(CurrentBar + 1, this.AtrPeriod);
                this.MultipledAtrSeries[bar] = this.AtrMultiplier * this.AtrSeries[bar];
            }

            this.MiddleBand[bar] = wma;
            this.UpperBand[bar] = wma + this.MultipledAtrSeries[bar];
            this.LowerBand[bar] = wma - this.MultipledAtrSeries[bar];
        }

        protected override void OnInitialize()
        {
            this.LogInfo("BANDS PROBEREN 56");
        }
    }
}
