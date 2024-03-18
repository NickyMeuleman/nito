using System;
using System.Windows.Media;
using ATAS.Indicators;
using OFT.Rendering.Settings;

namespace ATAS.Indicators.Technical
{
    public class VWMA : Indicator
    {
        private ValueDataSeries VwmaSeries = new ValueDataSeries("VwmaSeries")
        {
            VisualType = VisualMode.Line,
            LineDashStyle = LineDashStyle.Solid,
            Color = Colors.Coral
        };

        public int VwmaPeriod
        {
            get
            {
                return this.Sma1.Period;
            }
            set
            {
                this.Sma1.Period = value;
                this.Sma2.Period = value;
                RecalculateValues();
            }
        }

        private SMA Sma1 = new SMA();
        private SMA Sma2 = new SMA();

        public VWMA() {
            this.VwmaPeriod = 10;

            DataSeries[0] = VwmaSeries;
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            IndicatorCandle candle = GetCandle(bar);
            decimal top = Sma1.Calculate(bar, value * candle.Volume);
            decimal bottom = Sma2.Calculate(bar, candle.Volume);

            this.VwmaSeries[bar] = top / bottom;
        }
    }
}
