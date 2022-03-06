/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Windows;
using OsEngine.Language;
using OsEngine.OsTrader.Panels;

namespace OsEngine.Robots.Scalpers
{
    public partial class CandleScalperUi
    {
        private CandleScalper _strategy;
        public CandleScalperUi(CandleScalper strategy)
        {
            InitializeComponent();
            _strategy = strategy;

            ComboBoxRegime.Items.Add(BotTradeRegime.Off);
            ComboBoxRegime.Items.Add(BotTradeRegime.On);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyClosePosition);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyLong);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyShort);
            ComboBoxRegime.SelectedItem = _strategy.Regime;

            LabelRegime.Content = OsLocalization.Trader.Label115;
            ButtonAccept.Content = OsLocalization.Trader.Label17;

            LabelVolume.Content = OsLocalization.Trader.Label130;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Convert.ToDecimal(Volume.Text) <= 0) 
                {
                    throw new Exception("");
                }
            }
            catch (Exception)
            {
                MessageBox.Show(OsLocalization.Trader.Label13);
                return;
            }


            //_strategy;
            Enum.TryParse(ComboBoxRegime.Text, true, out _strategy.Regime);
            _strategy.VolumeFix = Convert.ToDecimal(Volume.Text);

            _strategy.Save();
            Close();
        }
    }
}
