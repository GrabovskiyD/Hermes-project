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
    public class TestBot : BotPanel
    {
        public TestBot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            _ma = new MovingAverage(name + "MA_fast", false) { Lenght = 50, TypePointsToSearch = PriceTypePoints.Open, TypeCalculationAverage = MovingAverageTypeCalculation.Exponential };
            _ma = (MovingAverage)_tab.CreateCandleIndicator(_ma, "Prime");
            _ma.Save();

            _tab.CandleUpdateEvent += Strateg_CandleUpdateEvent;

            VolumeFix = 1;
            Slipage = 0;

            Load();

            DeleteEvent += Strategy_DeleteEvent;

        }

        public override string GetNameStrategyType()
        {
            return "TestBot";
        }

        public override void ShowIndividualSettingsDialog()
        {
            //TwoMAUi ui = new TwoMAUi(this);
            //ui.ShowDialog();
        }

        private BotTradeRegime Regime;
        private BotTabSimple _tab;
        private MovingAverage _ma;
        public decimal VolumeFix;
        public decimal Slipage;

        public void Save()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt", false)
                    )
                {
                    writer.WriteLine(Regime);
                    writer.WriteLine(VolumeFix);
                    writer.WriteLine(Slipage);

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

                    VolumeFix = Convert.ToDecimal(reader.ReadLine());
                    Slipage = Convert.ToDecimal(reader.ReadLine());
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

        private decimal _OpenCandle;
        //private decimal _CloseCandle;
        private decimal _OpenCandleBack;
        //private decimal _CloseCandleBack;
        private decimal _Ma;
        private decimal _MaBack;

        private void Strateg_CandleUpdateEvent(List<Candle> candles)
        {
            if (Regime == BotTradeRegime.Off)
            {
                return;
            }

            if (_ma.Values == null)
            {
                return;
            }

            _Ma = _ma.Values[_ma.Values.Count - 1];
            _MaBack = _ma.Values[_ma.Values.Count - 2];
            _OpenCandle = candles[candles.Count - 1].Open;
            _OpenCandleBack = candles[candles.Count - 2].Open;

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
            if (_OpenCandle >= _Ma && _MaBack >= _OpenCandleBack && Regime != BotTradeRegime.OnlyShort)
            {
                _tab.BuyAtMarket(VolumeFix);
            }

            if (_OpenCandle <= _Ma && _MaBack <= _OpenCandleBack && Regime != BotTradeRegime.OnlyShort)
            {
                _tab.SellAtMarket(VolumeFix);
            }
        }

        private void LogicClosePosition(List<Candle> candles, Position position)
        {
            if (position.Direction == Side.Buy)
            {
                if (_OpenCandle <= _Ma && _MaBack <= _OpenCandleBack)
                {
                    _tab.CloseAtMarket(position, VolumeFix);
                }
            }

            if (position.Direction == Side.Sell)
            {
                if (_OpenCandle >= _Ma && _MaBack >= _OpenCandleBack)
                {
                    _tab.CloseAtMarket(position, VolumeFix);
                }
            }
        }

    }
}



