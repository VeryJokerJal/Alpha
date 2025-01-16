using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using UIShell.Commands;

namespace UIShell.Controls
{
    public class TimerPopup : Control
    {
        static TimerPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimerPopup), new FrameworkPropertyMetadata(typeof(TimerPopup)));
        }

        public static readonly DependencyProperty TargetTimeProperty =
            DependencyProperty.Register("TargetTime", typeof(DateTime), typeof(TimerPopup), new PropertyMetadata(DateTime.Now, OnTargetTimeChanged));

        public DateTime TargetTime
        {
            get => (DateTime)GetValue(TargetTimeProperty);
            set => SetValue(TargetTimeProperty, value);
        }

        public static readonly DependencyProperty WarningTimeProperty =
            DependencyProperty.Register("WarningTime", typeof(TimeSpan), typeof(TimerPopup), new PropertyMetadata(TimeSpan.Zero));

        public TimeSpan WarningTime
        {
            get => (TimeSpan)GetValue(WarningTimeProperty);
            set => SetValue(WarningTimeProperty, value);
        }

        public static readonly DependencyProperty CountdownTextProperty =
            DependencyProperty.Register("CountdownText", typeof(string), typeof(TimerPopup), new PropertyMetadata("00 : 00 : 00"));

        public string CountdownText
        {
            get => (string)GetValue(CountdownTextProperty);
            private set => SetValue(CountdownTextProperty, value);
        }

        public static readonly DependencyProperty StartCommandProperty =
            DependencyProperty.Register("StartCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand StartCommand
        {
            get => (ICommand)GetValue(StartCommandProperty);
            set => SetValue(StartCommandProperty, value);
        }

        public static readonly DependencyProperty PauseCommandProperty =
            DependencyProperty.Register("PauseCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand PauseCommand
        {
            get => (ICommand)GetValue(PauseCommandProperty);
            set => SetValue(PauseCommandProperty, value);
        }

        public static readonly DependencyProperty ResetCommandProperty =
            DependencyProperty.Register("ResetCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand ResetCommand
        {
            get => (ICommand)GetValue(ResetCommandProperty);
            set => SetValue(ResetCommandProperty, value);
        }

        public static readonly DependencyProperty FinishMessageProperty =
            DependencyProperty.Register("FinishMessage", typeof(string), typeof(TimerPopup), new PropertyMetadata("时间到！"));

        public string FinishMessage
        {
            get => (string)GetValue(FinishMessageProperty);
            set => SetValue(FinishMessageProperty, value);
        }

        public static readonly DependencyProperty TimeFormatProperty =
            DependencyProperty.Register("TimeFormat", typeof(string), typeof(TimerPopup), new PropertyMetadata(@"dd \: hh \: mm \: ss"));

        public string TimeFormat
        {
            get => (string)GetValue(TimeFormatProperty);
            set => SetValue(TimeFormatProperty, value);
        }

        public static readonly DependencyProperty AutoActionProperty =
            DependencyProperty.Register("AutoAction", typeof(Action), typeof(TimerPopup), new PropertyMetadata(null));

        public Action AutoAction
        {
            get => (Action)GetValue(AutoActionProperty);
            set => SetValue(AutoActionProperty, value);
        }

        public static readonly RoutedEvent CountdownFinishedEvent =
            EventManager.RegisterRoutedEvent("CountdownFinished", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimerPopup));

        public event RoutedEventHandler CountdownFinished
        {
            add => AddHandler(CountdownFinishedEvent, value);
            remove => RemoveHandler(CountdownFinishedEvent, value);
        }

        public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

        private readonly DispatcherTimer _timer;
        private DateTime _targetTime;
        private DateTime _startTime;
        private bool _isRunning;

        public TimerPopup()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            StartCommand = new RelayCommand(StartTimer, (s) => !_isRunning);
            PauseCommand = new RelayCommand(PauseTimer, (s) => _isRunning);
            ResetCommand = new RelayCommand(ResetTimer);

            YearUpCommand = new RelayCommand((s) => ChangeTime(1, 0, 0, 0, 0, 0));
            YearDownCommand = new RelayCommand((s) => ChangeTime(-1, 0, 0, 0, 0, 0));
            MonthUpCommand = new RelayCommand((s) => ChangeTime(0, 1, 0, 0, 0, 0));
            MonthDownCommand = new RelayCommand((s) => ChangeTime(0, -1, 0, 0, 0, 0));
            DayUpCommand = new RelayCommand((s) => ChangeTime(0, 0, 1, 0, 0, 0));
            DayDownCommand = new RelayCommand((s) => ChangeTime(0, 0, -1, 0, 0, 0));
            HourUpCommand = new RelayCommand((s) => ChangeTime(0, 0, 0, 1, 0, 0));
            HourDownCommand = new RelayCommand((s) => ChangeTime(0, 0, 0, -1, 0, 0));
            MinuteUpCommand = new RelayCommand((s) => ChangeTime(0, 0, 0, 0, 1, 0));
            MinuteDownCommand = new RelayCommand((s) => ChangeTime(0, 0, 0, 0, -1, 0));
            SecondUpCommand = new RelayCommand((s) => ChangeTime(0, 0, 0, 0, 0, 1));
            SecondDownCommand = new RelayCommand((s) => ChangeTime(0, 0, 0, 0, 0, -1));
        }

        public static readonly DependencyProperty YearUpCommandProperty =
            DependencyProperty.Register("YearUpCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand YearUpCommand
        {
            get => (ICommand)GetValue(YearUpCommandProperty);
            set => SetValue(YearUpCommandProperty, value);
        }

        public static readonly DependencyProperty YearDownCommandProperty =
            DependencyProperty.Register("YearDownCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand YearDownCommand
        {
            get => (ICommand)GetValue(YearDownCommandProperty);
            set => SetValue(YearDownCommandProperty, value);
        }

        public static readonly DependencyProperty MonthUpCommandProperty =
            DependencyProperty.Register("MonthUpCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand MonthUpCommand
        {
            get => (ICommand)GetValue(MonthUpCommandProperty);
            set => SetValue(MonthUpCommandProperty, value);
        }

        public static readonly DependencyProperty MonthDownCommandProperty =
            DependencyProperty.Register("MonthDownCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand MonthDownCommand
        {
            get => (ICommand)GetValue(MonthDownCommandProperty);
            set => SetValue(MonthDownCommandProperty, value);
        }

        public static readonly DependencyProperty DayUpCommandProperty =
            DependencyProperty.Register("DayUpCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand DayUpCommand
        {
            get => (ICommand)GetValue(DayUpCommandProperty);
            set => SetValue(DayUpCommandProperty, value);
        }

        public static readonly DependencyProperty DayDownCommandProperty =
            DependencyProperty.Register("DayDownCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand DayDownCommand
        {
            get => (ICommand)GetValue(DayDownCommandProperty);
            set => SetValue(DayDownCommandProperty, value);
        }

        public static readonly DependencyProperty HourUpCommandProperty =
            DependencyProperty.Register("HourUpCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand HourUpCommand
        {
            get => (ICommand)GetValue(HourUpCommandProperty);
            set => SetValue(HourUpCommandProperty, value);
        }

        public static readonly DependencyProperty HourDownCommandProperty =
            DependencyProperty.Register("HourDownCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand HourDownCommand
        {
            get => (ICommand)GetValue(HourDownCommandProperty);
            set => SetValue(HourDownCommandProperty, value);
        }

        public static readonly DependencyProperty MinuteUpCommandProperty =
            DependencyProperty.Register("MinuteUpCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand MinuteUpCommand
        {
            get => (ICommand)GetValue(MinuteUpCommandProperty);
            set => SetValue(MinuteUpCommandProperty, value);
        }

        public static readonly DependencyProperty MinuteDownCommandProperty =
            DependencyProperty.Register("MinuteDownCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand MinuteDownCommand
        {
            get => (ICommand)GetValue(MinuteDownCommandProperty);
            set => SetValue(MinuteDownCommandProperty, value);
        }

        public static readonly DependencyProperty SecondUpCommandProperty =
            DependencyProperty.Register("SecondUpCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand SecondUpCommand
        {
            get => (ICommand)GetValue(SecondUpCommandProperty);
            set => SetValue(SecondUpCommandProperty, value);
        }

        public static readonly DependencyProperty SecondDownCommandProperty =
            DependencyProperty.Register("SecondDownCommand", typeof(ICommand), typeof(TimerPopup), new PropertyMetadata(null));

        public ICommand SecondDownCommand
        {
            get => (ICommand)GetValue(SecondDownCommandProperty);
            set => SetValue(SecondDownCommandProperty, value);
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(TimerPopup), new PropertyMetadata(false));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Popup") is Popup popup)
            {
                popup.Closed += Popup_Closed;
            }
        }

        private void Popup_Closed(object? sender, EventArgs e)
        {
            IsOpen = false;
        }

        private static void OnTargetTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimerPopup control)
            {
                control.ResetTimer(null);
            }
        }

        private void StartTimer(object? sender)
        {
            _targetTime = TargetTime;
            _startTime = DateTime.Now;
            _timer.Start();
            _isRunning = true;
            _ = VisualStateManager.GoToState(this, "Open", true);
        }

        private void PauseTimer(object? sender)
        {
            _timer.Stop();
            _isRunning = false;
        }

        private void ResetTimer(object? sender)
        {
            _timer.Stop();
            _isRunning = false;
            CountdownText = "00 : 00 : 00";
            if (Template != null && Template.FindName("progressBar", this) is ProgressBar progressBar)
            {
                progressBar.Value = 0;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeSpan remainingTime = _targetTime - DateTime.Now;
            TimeSpan totalTime = _targetTime - _startTime;

            if (remainingTime <= TimeSpan.Zero)
            {
                _timer.Stop();
                OnCountdownFinished();
                remainingTime = TimeSpan.Zero;
            }

            CountdownText = $"{remainingTime.Days:D2} : {remainingTime.Hours:D2} : {remainingTime.Minutes:D2} : {remainingTime.Seconds:D2}";

            if (Template.FindName("progressBar", this) is ProgressBar progressBar)
            {
                double progress = 100 - (remainingTime.TotalSeconds / totalTime.TotalSeconds * 100);
                progressBar.Value = progress;
            }

            if (remainingTime <= WarningTime)
            {
                _ = VisualStateManager.GoToState(this, "Warning", true);
            }
        }

        protected virtual void OnCountdownFinished()
        {
            PlaySound();
            CountdownText = FinishMessage;
            History.Add($"{DateTime.Now}: {FinishMessage}");
            AutoAction?.Invoke();
            RaiseEvent(new RoutedEventArgs(CountdownFinishedEvent));
        }

        private void PlaySound()
        {
            System.Media.SystemSounds.Beep.Play(); // 简单的提示音
        }

        public void LoadConfig(string filePath)
        {
            XElement config = XElement.Load(filePath);
            TargetTime = DateTime.Parse(config.Element("TargetTime")?.Value ?? DateTime.Now.ToString());
            WarningTime = TimeSpan.Parse(config.Element("WarningTime")?.Value ?? TimeSpan.Zero.ToString());
            FinishMessage = config.Element("FinishMessage")?.Value ?? "时间到！";
            TimeFormat = config.Element("TimeFormat")?.Value ?? @"dd \: hh \: mm \: ss";
        }

        public void SaveConfig(string filePath)
        {
            XElement config = new("TimerControlConfig",
                new XElement("TargetTime", TargetTime.ToString("o")),
                new XElement("WarningTime", WarningTime.ToString()),
                new XElement("FinishMessage", FinishMessage),
                new XElement("TimeFormat", TimeFormat)
            );
            config.Save(filePath);
        }

        private void ChangeTime(int years, int months, int days, int hours, int minutes, int seconds)
        {
            TargetTime = TargetTime.AddYears(years)
                                   .AddMonths(months)
                                   .AddDays(days)
                                   .AddHours(hours)
                                   .AddMinutes(minutes)
                                   .AddSeconds(seconds);
        }
    }
}