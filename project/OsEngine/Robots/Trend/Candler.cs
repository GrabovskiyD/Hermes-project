using System;
using System.Collections.Generic;
using System.IO;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.Robots.Trend
{
    public class Candler : BotPanel
    {
        public Candler(string name, StartProgram startProgram)
            : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            _sma = new MovingAverage(name + "Sma", false);
            _sma = (MovingAverage)_tab.CreateCandleIndicator(_sma, "Prime");
            _sma.Save();

            _tab.CandleFinishedEvent += Strateg_CandleFinishedEvent;

            Slipage = 0;
            VolumeFix = 1;

            Load();


            DeleteEvent += Strategy_DeleteEvent;
        }

        public override string GetNameStrategyType()
        {
            return "Candler";
        }

        public override void ShowIndividualSettingsDialog()
        {
            //CandlerUi ui = new CandlerUi(this);
            //ui.ShowDialog();
        }

        private BotTabSimple _tab;
        private MovingAverage _sma;
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

        private decimal _lastOpen;
        private decimal _lastClose;

        private decimal _lastSma;

        private void Strateg_CandleFinishedEvent(List<Candle> candles)
        {
            if (Regime == BotTradeRegime.Off)
            {
                return;
            }

            if (_sma.Values ==  null)
            {
                return;
            }

            _lastClose = candles[candles.Count - 1].Close;
            _lastOpen = candles[candles.Count - 1].Open;
            _lastSma = _sma.Values[_sma.Values.Count - 1];

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
            if (_lastOpen > _lastSma && Regime != BotTradeRegime.OnlyShort)
            {
                _tab.BuyAtMarket(VolumeFix);
            }

            if (_lastOpen < _lastSma && Regime != BotTradeRegime.OnlyLong)
            {
                _tab.SellAtMarket(VolumeFix);
            }
        }

        private void LogicClosePosition(List<Candle> candles, Position position)
        {
            if (position.Direction == Side.Buy)
            {
                if (_lastOpen < _lastSma)
                {
                    _tab.CloseAtMarket(position, VolumeFix);
                }
            }

            if (position.Direction == Side.Sell)
            {
                if (_lastOpen > _lastSma)
                {
                    _tab.CloseAtMarket(position, VolumeFix);
                }
            }
        }

    }

}
