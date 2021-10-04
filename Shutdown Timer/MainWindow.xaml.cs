using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Shutdown_Timer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private DispatcherTimer progressBarTimer;
        private int _totalTime;
        private int _remainingTime;
        //USED FOR LOG OFF
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);
        const int WTS_CURRENT_SESSION = -1;
        static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        public MainWindow()
        {
            InitializeComponent();
            CreateTimers();
        }

        //*****************************BUTTONS*****************************
        private void topBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            int hr = 0;
            int min = 0;
            int sec = 0;

            if (!(String.IsNullOrEmpty(hoursInput.Text)))
            {
                try
                {
                    hr = Int32.Parse(hoursInput.Text);
                }
                catch{}
            }

            if (!(String.IsNullOrEmpty(minutesInput.Text)))
            {
                try
                {
                    min = Int32.Parse(minutesInput.Text);
                }
                catch{}
            }

            if (!(String.IsNullOrEmpty(secondsInput.Text)))
            {
                try
                {
                    sec = Int32.Parse(secondsInput.Text);
                }
                catch{}                
            }

            if (hr > 0 || min > 0 || sec > 0)
            {
                SetEndTime(hr, min, sec);
            }
            CheckValidTime();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_remainingTime > 0)
            {
                if (shutdownRadio.IsChecked == true)
                {
                    timerLabel.Content = "Shutdown Cancelled";
                }
                if (restartRadio.IsChecked == true)
                {
                    timerLabel.Content = "Restart Cancelled";
                }
                if (logOffRadio.IsChecked == true)
                {
                    timerLabel.Content = "Log Off Cancelled";
                }
                timer.Content = "----";
                TimerStop();
                _remainingTime = 0;
                _totalTime = 0;
            }
            else
            {
                timerLabel.Content = "Timer Is Not Running";
            }
        }

        private void shutdownRadio_Click(object sender, RoutedEventArgs e)
        {
            if (_remainingTime > 0)
            {
                SettingsChangedText();
                TimerStop();
            }
        }

        private void restartRadio_Click(object sender, RoutedEventArgs e)
        {
            if (_remainingTime > 0)
            {
                SettingsChangedText();
                TimerStop();
            }
        }

        private void logOffRadio_Click(object sender, RoutedEventArgs e)
        {
            if (_remainingTime > 0)
            {
                SettingsChangedText();
                TimerStop();
            }
        }


        //*****************************TIME DATA*****************************
        private void SetEndTime(int hr, int min, int sec)
        {
            int _hoursInSeconds = hr * 3600;
            int _minutesInSeconds = min * 60;
            int _seconds = sec;

            _totalTime = _hoursInSeconds + _minutesInSeconds + _seconds;
            _remainingTime = _hoursInSeconds + _minutesInSeconds + _seconds;
        }

        private void CheckValidTime()
        {
            if (_remainingTime > 0 && _remainingTime < 86400)
            {
                TimerStart();
                GetRadioStatus();
                ResetTextFields();
            }
            else
            {
                if (_remainingTime <= 0)
                {
                    timerLabel.Content = "No Time Has Been Entered";
                    ResetTextFields();
                }
                else
                {
                    timerLabel.Content = "Entered Time Is Out Of Range";
                    ResetTextFields();
                }
            }
        }

        //*****************************TEXT LABELS*****************************
        private void GetRadioStatus()
        {
            if (shutdownRadio.IsChecked == true)
            {
                timerLabel.Content = "Shutdown Will Occur In";
            }
            if (restartRadio.IsChecked == true)
            {
                timerLabel.Content = "Restart Will Occur In";
            }
            if (logOffRadio.IsChecked == true)
            {
                timerLabel.Content = "Log Off Will Occur In";
            }
        }

        private void ResetTextFields()
        {
            hoursInput.Text = "";
            minutesInput.Text = "";
            secondsInput.Text = "";
        }

        private void SettingsChangedText()
        {
            timerLabel.Content = "Settings Changed. Please Reset Timer";
            timer.Content = "----";
        }

        //*****************************TIMERS*****************************        
        private void CreateTimers()
        {
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Tick += new EventHandler(dispatcherTimer_Tick);
            _timer.Interval = new TimeSpan(0, 0, 1);

            progressBarTimer = new System.Windows.Threading.DispatcherTimer();
            progressBarTimer.Tick += new EventHandler(progressBar_Tick);
            progressBarTimer.Interval = new TimeSpan(0, 0, 1);
            progressBar.Value = 0;
        }        

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_remainingTime > 0)
            {
                TimeSpan _countdown = TimeSpan.FromSeconds(_remainingTime);
                string displayTime = string.Format("{0:D2}:{1:D2}:{2:D2}", _countdown.Hours, _countdown.Minutes, _countdown.Seconds);
                timer.Content = displayTime;
                this._remainingTime--;
            }
            if (_remainingTime == 0)
            {
                TimerOver();
            }
        }

        private void progressBar_Tick(object sender, EventArgs e)
        {
            progressBar.Maximum = _totalTime;
            progressBar.Minimum = 0;
            progressBar.Value++;
        }

        private void TimerStart()
        {
            _timer.Start();
            progressBarTimer.Start();
        }

        private void TimerStop()
        {
            _timer.Stop();
            progressBarTimer.Stop();
            progressBar.Value = 0;
        }

        private void TimerOver()
        {
            TimerStop();
            timer.Content = "----";
            if (shutdownRadio.IsChecked == true)
            {
                timerLabel.Content = "Shutting Down...";
                System.Diagnostics.Process.Start("shutdown", "/s /t 0");
            }
            if (restartRadio.IsChecked == true)
            {
                timerLabel.Content = "Restarting...";
                System.Diagnostics.Process.Start("shutdown", "/r /t 0");
            }
            if (logOffRadio.IsChecked == true)
            {
                timerLabel.Content = "Logging Off...";

                
                if (!WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, false))
                    throw new Win32Exception();
                Environment.Exit(1);
                
            }
        }
    }
}
