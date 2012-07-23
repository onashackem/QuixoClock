using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace QuixoClock
{
    /// <summary>
    /// Interaction logic for QuixoClockWindow.xaml
    /// </summary>
    public partial class QuixoClockWindow : Window
    {
        private Dictionary<int, Player> _players;
        private DispatcherTimer _timer;
        private int _seconds;

        private string SelectedTime
        {
            get { return ((ComboBoxItem)cbxSelectTime.SelectedItem).Content.ToString(); }
        }

        public QuixoClockWindow()
        {
            InitializeComponent();

            // 2 players are default
            rb2Players.IsChecked = true;

            // 2nd item is default
            cbxSelectTime.SelectedIndex = 1;
        }

        private Dictionary<int, Player> CreatePlayers()
        {
            switch (rb2Players.IsChecked)
            {
                case true:
                    return new Dictionary<int,Player>(){
                        {1, new Player(tbxPlayer1of2.Text, lblPlayer1Score, btnPlayer1Won)},
                        {2, new Player(tbxPlayer2of2.Text, lblPlayer2Score, btnPlayer2Won)}
                    };
                default:
                    return new Dictionary<int, Player>(){
                        {1, new Player(tbxPlayer1of2.Text, lblPlayer1Score, btnPlayer1Won)},
                        {2, new Player(tbxPlayer2of4.Text, lblPlayer2Score, btnPlayer2Won)},
                        {3, new Player(tbxPlayer3of4.Text, lblPlayer3Score, btnPlayer3Won)},
                        {4, new Player(tbxPlayer4of4.Text, lblPlayer4Score, btnPlayer4Won)}
                    };
            }
        }

        private void PlayerCountSelected(object sender, RoutedEventArgs e)
        {
            g2PlayersName.IsEnabled = (sender == rb2Players);
            g4PlayersName.IsEnabled = (sender == rb4Players);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            SwitchMode(GameModes.PLAY);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            SwitchMode(GameModes.SETTINGS);
        }

        private void btnPlayerWon_Click(object sender, RoutedEventArgs e)
        {
            _players.Values.First(p => p.Button == sender).HasWon();
        }

        private void btnTimer_Click(object sender, RoutedEventArgs e)
        {
            // Reset timer
            StopTimer();
            StartTimer();
        }

        private void SwitchMode(GameModes mode)
        {
            switch (mode)
            {
                case GameModes.PLAY:
                    StartGame();
                    break;

                case GameModes.SETTINGS:
                    StopGame();
                    break;
            }

            // In every case reset value on timer
            btnTimer.Content = SelectedTime;
        }

        private void StopGame()
        {
            SetSettingsDeckEnabled(true);

            SetGameDeckEnabled(false);
        }

        private void StartGame()
        {
            // Initialize players
            _players = CreatePlayers();

            // Count seconds for 1 turn
            string[] times = SelectedTime.Split(new char[] { ':' });
            _seconds = int.Parse(times[0]) * 60 + int.Parse(times[1]);

            SetSettingsDeckEnabled(false);

            SetGameDeckEnabled(true);
        }

        private void SetGameDeckEnabled(bool isEnabled)
        {
            // Stop timer when stopping game
            if (!isEnabled)
                StopTimer();

            btnTimer.IsEnabled = isEnabled;

            foreach (Player p in _players.Values)
            {
                p.SetButtonEnability(isEnabled);

                // Start game -> InitLabels
                // Stop game -> ClearLabels
                if (isEnabled)
                    p.RefreshScore();
                else
                    p.ClearScoreLabel();
            }
        }

        private void SetSettingsDeckEnabled(bool isEnabled)
        {
            rb2Players.IsEnabled = isEnabled;
            rb4Players.IsEnabled = isEnabled;
            btnStart.IsEnabled = isEnabled;
            btnStop.IsEnabled = !isEnabled;
            cbxSelectTime.IsEnabled = isEnabled;
        }

        private void StartTimer()
        {
            btnTimer.Content = SelectedTime;
            
            // Time per turn
            int secondsLeft = _seconds;
            
                // Init time timer
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);

            _timer.Tick += new EventHandler(
                delegate{
                    secondsLeft -= 1;
                    btnTimer.Content = string.Format("{0}:{1}", secondsLeft / 60, (secondsLeft % 60).ToString().PadLeft(2, '0'));

                    if (secondsLeft == 0)
                    {
                        MessageBox.Show("HRÁÁÁÁÁÁJ", "Time's up!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        _timer.Stop();
                    }
                }
            );

            _timer.Start();
        }

        private void StopTimer()
        {
            // Stop running timer
            if (_timer != null && _timer.IsEnabled)
                _timer.Stop();
        }

        private void cbxSelectTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnTimer.Content = SelectedTime;
        }

        private class Player
        {
            private string _name;
            private Label _label;
            public Button Button { get; private set; }

            public int Score { get; private set; }

            public Player(string name, Label label, Button button)
            {
                _name = name;
                _label = label;
                Button = button;
                Score = 0;
            }

            public void HasWon()
            {
                ++Score;
                RefreshScore();
            }

            public void RefreshScore()
            {
                _label.Content = string.Format("{0}: {1}", _name, Score);
            }

            public void SetButtonEnability(bool isEnabled)
            {
                Button.IsEnabled = isEnabled;
            }

            public void ClearScoreLabel()
            {

                _label.Content = String.Empty;
            }
        }

        private enum GameModes
        {
            PLAY,

            SETTINGS
        }
    }
}
