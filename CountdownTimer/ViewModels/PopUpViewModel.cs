using System;
using System.Media;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Btl.Builders;
using Btl.Messaging;
using Btl.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Btl.ViewModels
{
    public class PopUpViewModel : ViewModelBase
    {
        #region Members
        private FontFamily _fontFamily = null;
        private double _fontSize = 0d;
        private Color _statusColor = Colors.Transparent;
        readonly ITimerModel _timer = TimerModelBuilder.GetNewTimer();
        readonly ISettingsModel _settings = SettingsModelBuilder.GetNewSettings();

        private TaskbarItemProgressState _ProgressState;
        private double _ProgressValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Use a static constructor as it is the easiest way to handle any
        /// exceptions that might be thrown when creating the view models.
        /// </summary>
        static PopUpViewModel()
        {

        }
        /// <summary>
        /// Construct a new timer view model.
        /// </summary>
        public PopUpViewModel()
        {
            //  add event handlers
            AddEventHandlers();

            //  update all the settings, such as the timer duration and so on.
            UpdateMembersFromSettings();

            //  bind the commands to their respective actions
            BindCommands();

            //  Register against the Messenger singleton to receive any simple
            //  messages.  Specifically the one that says settings have changed.
            Messenger.Default.Register<SimpleMessage>(this, ConsumeMessage);
            Messenger.Default.Register<TaskbarItemMessage>(this, ConsumeTaskbarItemMessage);
        }

        #endregion

        #region Commands

        public ICommand StartTimer { get; private set; }
        public ICommand StopTimer { get; private set; }
        public ICommand Loaded { get; private set; }

        #region Start Command

        /// <summary>
        /// Start the underlying timer
        /// </summary>
        void StartTimerExecute()
        {
            _timer.Start();
        }

        /// <summary>
        /// can we start the underlying timer?
        /// </summary>
        /// <returns></returns>
        bool CanStartTimerExecute()
        {
            return !_timer.Complete && _timer.Status != TimerState.Running;
        }

        #endregion

        #region Stop Command

        /// <summary>
        /// Stop the underlying timer.
        /// </summary>
        void StopTimerExecute()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Can the timer be stopped?
        /// </summary>
        /// <returns></returns>
        bool CanStopTimerExecute()
        {
            return !_timer.Complete && _timer.Status == TimerState.Running;
        }

        #endregion

        #region Loaded Command
        /// <summary>
        /// Stuff that is to execute when the view that displays the current ViewModel is loaded
        /// </summary>
        void LoadedExecute()
        {
            if(CanStartTimerExecute()) StartTimerExecute();
        }

        #endregion

        #endregion

        #region Properties
        string timerValue;

        /// <summary>
        /// The value of the timer as a string.
        /// </summary>
        public string TimerValue
        {
            get
            {
                return timerValue;
            }

            set
            {
                if (timerValue != value)
                {
                    timerValue = value;
                    RaisePropertyChanged("TimerValue");
                }
            }
        }

        private double percentElapsed;
        /// <summary>
        /// The percentage elapsed.
        /// </summary>
        public double PercentElapsed
        {
            get
            {
                return percentElapsed;
            }
            set
            {
                if (value != percentElapsed)
                {
                    percentElapsed = value;
                    RaisePropertyChanged("PercentElapsed");
                }
            }
        }

        /// <summary>
        /// The background colour of the clock
        /// </summary>
        public Color StatusColor
        {
            get
            {
                return _statusColor;
            }
            set
            {
                if (value != _statusColor)
                {
                    _statusColor = value;
                    RaisePropertyChanged("StatusColor");
                    StatusBrush = new SolidColorBrush(_statusColor);
                }
            }
        }

        /// <summary>
        /// The timer duration.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return _timer.Duration;
            }

            set
            {
                if (_timer.Duration == value)
                    return;
                _timer.Duration = value;
                RaisePropertyChanged("Duration");
            }
        }

        /// <summary>
        /// The brush that we paint the clock with.
        /// </summary>
        Brush statusBrush = new SolidColorBrush();
        public Brush StatusBrush
        {
            get
            {
                return statusBrush;
            }

            private set
            {
                if (value != statusBrush)
                {
                    statusBrush = value;
                    RaisePropertyChanged("StatusBrush");
                }
            }
        }

        /// <summary>
        /// The clock font family
        /// </summary>
        public FontFamily FontFamily
        {
            get
            {
                return _fontFamily;
            }
            set
            {
                if (_fontFamily == value)
                    return;
                _fontFamily = value;
                RaisePropertyChanged("FontFamily");
            }
        }

        /// <summary>
        /// The clock font size.
        /// </summary>
        public double FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                if (_fontSize == value)
                    return;
                _fontSize = value;
                RaisePropertyChanged("FonSize");
            }
        }

        /// <summary>
        /// The progress state of the timer (aimed at the taskbar).
        /// </summary>
        public TaskbarItemProgressState ProgressState
        {
            get
            {
                return _ProgressState;
            }
            set
            {
                if (_ProgressState == value)
                    return;
                _ProgressState = value;

                RaisePropertyChanged("ProgressState");
            }
        }

        /// <summary>
        /// The progress value of the timer.
        /// </summary>
        public double ProgressValue
        {
            get
            {
                return _ProgressValue;
            }
            set
            {
                if (_ProgressValue == value)
                    return;
                _ProgressValue = value;

                RaisePropertyChanged("ProgressValue");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update the TaskbarItemInfo values with whatever is specified in the
        /// message.
        /// </summary>
        /// <param name="message"></param>
        void ConsumeTaskbarItemMessage(TaskbarItemMessage message)
        {
            if (message == null)
                return;

            ProgressState = message.State;

            //  if the taskbar item message carried a (percentage) value,
            //  update the taskbar progress value with it.
            if (message.HasValue)
                ProgressValue = message.Value;
        }

        private void BindCommands()
        {
            StartTimer = new RelayCommand(() => StartTimerExecute(), CanStartTimerExecute);
            StopTimer = new RelayCommand(() => StopTimerExecute(), CanStopTimerExecute);
            Loaded = new RelayCommand(() => LoadedExecute());
        }

        /// <summary>
        /// Update the TimerViewModel values from the (user defined) settings.
        /// </summary>
        private void UpdateMembersFromSettings()
        {
            _settings.Reload();

            Duration = _settings.Duration;
            
            FontSize = _settings.FontSize;
            FontFamily = _settings.FontFamily;

            UpdateTimerValues();
        }

        /// <summary>
        /// Consume any messages that are passed between models
        /// </summary>
        /// <param name="message"></param>
        void ConsumeMessage(SimpleMessage message)
        {
            switch (message.Type)
            {
                case SimpleMessage.MessageType.SwitchToTimerView:
                    //  ignored
                    break;
                case SimpleMessage.MessageType.SwitchToSettingsView:
                    //  ignored
                    break;
                case SimpleMessage.MessageType.SettingsChanged:
                    StopTimerExecute();
                    UpdateMembersFromSettings();
                    break;
                case SimpleMessage.MessageType.TimerStop:
                    StopTimerExecute();
                    break;
                case SimpleMessage.MessageType.TimerStart:
                    StartTimerExecute();
                    break;
            }
        }

        /// <summary>
        /// Update the timer view model properties based on the time span passed in.
        /// </summary>
        /// <param name="t"></param>
        private void UpdateTimer(TimerModelEventArgs e)
        {
            UpdateTimerValues();
            UpdateTimerStatusColor(e);
        }

        /// <summary>
        /// Set the solid colour based on the timer status.
        /// </summary>
        /// <param name="e"></param>
        private void UpdateTimerStatusColor(TimerModelEventArgs e)
        {
            if (_settings.Colours == false)
            {
                StatusColor = Colors.Transparent;
                return;
            }

            switch (e.State)
            {
                case TimerModelEventArgs.Status.NotSpecified:
                    StatusColor = Colors.Beige;
                    break;
                case TimerModelEventArgs.Status.Stopped:
                    StatusColor = Colors.Beige;
                    break;
                case TimerModelEventArgs.Status.Started:
                    StatusColor = Colors.LightSalmon;
                    break;
                case TimerModelEventArgs.Status.Running:
                    StatusColor = Colors.LightSalmon;
                    break;
                case TimerModelEventArgs.Status.Completed:
                    StatusColor = Colors.LightGreen;
                    break;
                case TimerModelEventArgs.Status.Reset:
                    StatusColor = Colors.Beige;
                    break;
            }
        }

        /// <summary>
        /// Update the timer view model properties based on the time span passed in.
        /// </summary>
        /// <param name="t"></param>
        private void UpdateTimerValues()
        {
            TimeSpan t = _timer.Remaining;
            TimerValue = string.Format("{0}:{1}:{2}", t.Hours.ToString("D2"),
                t.Minutes.ToString("D2"), t.Seconds.ToString("D2"));

            PercentElapsed = 100.0 - (100.0 * _timer.Remaining.TotalSeconds / _timer.Duration.TotalSeconds);
        }

        /// <summary>
        /// Add the event handlers.
        /// </summary>
        private void AddEventHandlers()
        {
            _timer.Tick += (sender, e) => OnTick(sender, e);
            _timer.Completed += (sender, e) => OnCompleted(sender, e);
            _timer.Started += (sender, e) => OnStarted(sender, e);
            _timer.Stopped += (sender, e) => OnStopped(sender, e);
            _timer.TimerReset += (sender, e) => OnReset(sender, e);
        }
        #endregion

        #region Event handlers

        /// <summary>
        /// Fires where the timer completes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCompleted(object sender, TimerModelEventArgs e)
        {
            UpdateTimer(e);

            if (_settings.PlayExclamation)
            {
                SystemSounds.Exclamation.Play();
            }

            Messenger.Default.Send(new TaskbarItemMessage { State = TaskbarItemProgressState.Normal, Value = 1.0 });
        }

        /// <summary>
        /// Fires when the timer ticks.  Ticks out to be of the order of 
        /// tenths of a second or so to prevent excessive spamming of this method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTick(object sender, TimerModelEventArgs e)
        {
            UpdateTimer(e);

            Messenger.Default.Send(new TaskbarItemMessage { State = TaskbarItemProgressState.Normal, Value = PercentElapsed / 100.0 });
            Messenger.Default.Send(new SimpleMessage(SimpleMessage.MessageType.TimerTick, TimerValue));
        }

        /// <summary>
        /// Fires when the timer starts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnStarted(object sender, TimerModelEventArgs e)
        {
            UpdateTimer(e);

            if (_settings.PlayBeep)
            {
                SystemSounds.Beep.Play();
            }

            Messenger.Default.Send(new TaskbarItemMessage { State = TaskbarItemProgressState.Normal });
        }

        /// <summary>
        /// Fires when the timer stops.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStopped(object sender, TimerModelEventArgs e)
        {
            UpdateTimer(e);

            Messenger.Default.Send(new TaskbarItemMessage { State = TaskbarItemProgressState.Paused });
        }

        /// <summary>
        /// Fires when the timer resets.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReset(object sender, TimerModelEventArgs e)
        {
            var timer = sender as TimerModel;
            UpdateTimer(e);

            Messenger.Default.Send(new TaskbarItemMessage { State = TaskbarItemProgressState.None, Value = 0.0 });
            Messenger.Default.Send(new SimpleMessage(SimpleMessage.MessageType.TimerReset));
        }
        #endregion
    }
}