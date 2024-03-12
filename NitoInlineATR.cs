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

    public class Nito2 : Indicator
    {
        private int _period = 10;
        private decimal _multiplier = 1;
        private ValueDataSeries _values = new("values");
        private ValueDataSeries atrseries = new("atr");

        private ValueDataSeries UpperBand = new ValueDataSeries("UpperBand")
        {
            VisualType = VisualMode.Line,
            LineDashStyle = LineDashStyle.Solid,
            Color = Colors.Green
        };

        private ValueDataSeries LowerBand = new ValueDataSeries("LowerBand")
        {
            VisualType = VisualMode.Line,
            LineDashStyle = LineDashStyle.Solid,
            Color = Colors.Red
        };

        private readonly WMA Wma = new WMA();

        public int WmaPeriod
        {
            get
            {
                return Wma.Period;
            }
            set
            {
                if (value <= 0)
                {
                    return;
                }
                this.Wma.Period = value;
                RecalculateValues();
            }
        }

        [OFT.Attributes.Parameter]
        [Display(ResourceType = typeof(Resources),
    Name = "Period",
    GroupName = "Common",
    Order = 20)]
        [Range(1, 10000)]
        public int Period
        {
            get => _period;
            set
            {
                _period = value;
                RecalculateValues();
            }
        }

        [Display(ResourceType = typeof(Resources),
            Name = "Multiplier",
            GroupName = "Common",
            Order = 20)]
        [Range(0.0000001, 10000000)]
        public decimal Multiplier
        {
            get => _multiplier;
            set
            {
                _multiplier = value;
                RecalculateValues();
            }
        }

        public Nito2()
        {
            // location to display this indicator, on the panel where the bars are (the chart) or a new panel
            Panel = IndicatorDataProvider.CandlesPanel;
            // the first data series at index 0
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Line;
            ((ValueDataSeries)DataSeries[0]).Color = Colors.CadetBlue;

            this.WmaPeriod = 20;
            this.Period = 20;
            this.Multiplier = 20;

            // add upper and lower bands
            base.DataSeries.Add(UpperBand);
            base.DataSeries.Add(LowerBand);
        }
        protected override void OnCalculate(int bar, decimal value)
        {
            decimal wma = this.Wma.Calculate(bar, value);

            // ATR
            if (bar == 0 && ChartInfo != null)
                ((ValueDataSeries)DataSeries[0]).StringFormat = ChartInfo.StringFormat;

            var candle = GetCandle(bar);
            var high0 = candle.High;
            var low0 = candle.Low;

            if (bar == 0)
                _values[bar] = high0 - low0;
            else
            {
                var close1 = GetCandle(bar - 1).Close;
                var trueRange = Math.Max(Math.Abs(low0 - close1), Math.Max(high0 - low0, Math.Abs(high0 - close1)));
                _values[bar] = ((Math.Min(CurrentBar + 1, Period) - 1) * _values[bar - 1] + trueRange) / Math.Min(CurrentBar + 1, Period);
                atrseries[bar] = Multiplier * _values[bar];
            }
            // end ATR

            // add data to dataseries every time this function is called

            // middle band
            this[bar] = wma;

            // outer bands
            this.UpperBand[bar] = wma + atrseries[bar];
            this.LowerBand[bar] = wma - atrseries[bar];
        }

        protected override void OnInitialize()
        {
            this.LogInfo("BANDS PROBEREN 16");
        }
    }
}
