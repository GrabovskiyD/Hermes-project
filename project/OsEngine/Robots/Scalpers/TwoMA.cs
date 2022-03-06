using System;
using System.Collections.Generic;
using System.IO;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Robots.Scalpers
{
    public class TwoMA : BotPanel
    {
        public TwoMA(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            _maFast = new MovingAverage(name + "MA_fast", false) {Lenght = 10, TypePointsToSearch = PriceTypePoints.Open};
            _maFast = (MovingAverage)_tab.CreateCandleIndicator(_maFast, "Prime");
            _maFast.Save();

            _maSlow = new MovingAverage(name + "MA_slow", false) {Lenght = 50, TypePointsToSearch = PriceTypePoints.Open};
            _maSlow = (MovingAverage)_tab.CreateCandleIndicator(_maSlow, "Prime");
            _maSlow.Save();

            _tab.CandleUpdateEvent += Strateg_CandleUpdateEvent;

            Slipage = 0;
            VolumeFix = 1;

            Load();

            DeleteEvent += Strategy_DeleteEvent;
        }

        public override string GetNameStrategyType()
        {
            return "TwoMA";
        }

        public override void ShowIndividualSettingsDialog()
        {
            //TwoMAUi ui = new TwoMAUi(this);
            //ui.ShowDialog();
        }

        private BotTabSimple _tab;
        private MovingAverage _maFast;
        private MovingAverage _maSlow;
        public decimal VolumeFix;
        public decimal Slipage;
        public BotTradeRegime Regime;

        public void Save()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt", false)
                    )
                {
                    writer.WriteLine(Slipage);
                    writer.WriteLine(VolumeFix);
                    writer.WriteLine(Regime);

                    writer.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Load()
        {
            if (!File.Exists(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt"))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt"))
                {
                    Slipage = Convert.ToDecimal(reader.ReadLine());
                    VolumeFix = Convert.ToDecimal(reader.ReadLine());
                    Enum.TryParse(reader.ReadLine(), true, out Regime);

                    reader.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        void Strategy_DeleteEvent()
        {
            if (File.Exists(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt"))
            {
                File.Delete(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt");
            }
        }

        private decimal _MA_fast;
        private decimal _MA_slow;
        private decimal _MA_fast_back;
        private decimal _MA_slow_back;

        private void Strateg_CandleUpdateEvent(List<Candle> candles)
        {
            if (Regime == BotTradeRegime.Off)
            {
                return;
            }

            if (_maFast.Values == null || _maSlow.Values == null)
            {
                return;
            }

            _MA_fast = _maFast.Values[_maFast.Values.Count - 1];
            _MA_slow = _maSlow.Values[_maSlow.Values.Count - 1];
            _MA_fast_back = _maFast.Values[_maFast.Values.Count - 2];
            _MA_slow_back = _maSlow.Values[_maSlow.Values.Count - 2];

            List<Position> openPositions = _tab.PositionsOpenAll;

            if (openPositions != null && openPositions.Count != 0)
            {
                for (int i = 0; i < openPositions.Count; i++)
                {
                    LogicClosePosition(candles, openPositions[i]);

                }
            }

            if (Regime == BotTradeRegime.OnlyClosePosition)
            {
                return;
            }
            if (openPositions == null || openPositions.Count == 0)
            {
                LogicOpenPosition(candles, openPositions);
            }

        }

        private void LogicOpenPosition(List<Candle> candles, List<Position> position)
        {
            if (_MA_fast >= _MA_slow && _MA_slow_back >= _MA_fast_back && Regime != BotTradeRegime.OnlyShort)
            {
                _tab.BuyAtMarket(VolumeFix);
            }

            if (_MA_slow >= _MA_fast && _MA_fast_back >= _MA_slow_back && Regime != BotTradeRegime.OnlyLong)
            {
                _tab.SellAtMarket(VolumeFix);
            }
        }

        private void LogicClosePosition(List<Candle> candles, Position position)
        {
            if (position.Direction == Side.Buy)
            {
                if (_MA_slow >= _MA_fast && _MA_fast_back >= _MA_slow_back)
                {
                    _tab.CloseAtMarket(position, VolumeFix);
                }
            }

            if (position.Direction == Side.Sell)
            {
                if (_MA_fast >= _MA_slow && _MA_slow_back >= _MA_fast_back)
                {
                    _tab.CloseAtMarket(position, VolumeFix);
                }
            }
        }
    }
}

