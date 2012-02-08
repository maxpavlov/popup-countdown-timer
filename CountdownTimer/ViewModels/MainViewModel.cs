// Copyright 2012 lapthorn.net.
//
// This software is provided "as is" without a warranty of any kind. All
// express or implied conditions, representations and warranties, including
// any implied warranty of merchantability, fitness for a particular purpose
// or non-infringement, are hereby excluded. lapthorn.net and its licensors
// shall not be liable for any damages suffered by licensee as a result of
// using the software. In no event will lapthorn.net be liable for any
// lost revenue, profit or data, or for direct, indirect, special,
// consequential, incidental or punitive damages, however caused and regardless
// of the theory of liability, arising out of the use of or inability to use
// software, even if lapthorn.net has been advised of the possibility of
// such damages.
//
// You are free to fork this via github:  https://github.com/barrylapthorn/countdown_timer

using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Btl.Models;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using System.Windows.Shell;

namespace Btl.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _WindowTitle;
        private double _ProgressValue;
        private TaskbarItemProgressState _ProgressState;
        private bool _TopMost;
        private ViewModelBase _currentViewModel = null;
        private string _originalWindowTitle = "Countdown Timer";

        private readonly static TimerViewModel _timerViewModel;
        private readonly static SettingsViewModel _settingsViewModel;
        private readonly static AboutViewModel _aboutViewModel;


        /// <summary>
        /// Use a static constructor as it is the easiest way to handle any
        /// exceptions that might be thrown when creating the view models.
        /// </summary>
        static MainViewModel()
        {
            try
            {
                _timerViewModel = new TimerViewModel();
                _settingsViewModel = new SettingsViewModel();
                _aboutViewModel = new AboutViewModel();
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(string.Format("An exception was thrown trying to construct the ViewModels:  {0}", exception.Message), 
                    "Oh Dear!", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            //  Set the start-up view model.
            CurrentViewModel = _timerViewModel;

            //  Create the commands
            TimerViewCommand = new RelayCommand(() => ExecuteViewTimerCommand());
            SettingsViewCommand = new RelayCommand(() => ExecuteViewSettingsCommand());
            PlayCommand = new RelayCommand(() => ExecutePlayCommand());
            PauseCommand = new RelayCommand(() => ExecutePauseCommand());

            //  Set the window title.
            WindowTitle = _originalWindowTitle;

            //  Update anything else that is user-settings related.
            OnSettingsChanged();

            //  Lastly, listen for messages from other view models.
            Messenger.Default.Register<SimpleMessage>(this, ConsumeMessage);
            Messenger.Default.Register<TaskbarItemMessage>(this, ConsumeTaskbarItemMessage);
        }

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

        /// <summary>
        /// Consume the SimpleMessage class and perform actions based on its content.
        /// </summary>
        /// <param name="message"></param>
        private void ConsumeMessage(SimpleMessage message)
        {
            switch (message.Type)
            {
                case SimpleMessage.MessageType.TimerTick:
                    //  this happens the most so put it first
                    WindowTitle = message.Message;
                    break;
                case SimpleMessage.MessageType.SwitchToTimerView:
                    ExecuteViewTimerCommand();
                    break;
                case SimpleMessage.MessageType.SwitchToSettingsView:
                    ExecuteViewSettingsCommand();
                    break;
                case SimpleMessage.MessageType.SwitchToAboutView:
                    ExecuteViewAboutCommand();
                    break;
                case SimpleMessage.MessageType.SettingsChanged:
                    OnSettingsChanged();
                    break;
                case SimpleMessage.MessageType.TimerStop:
                    WindowTitle = _originalWindowTitle;
                    break;
                case SimpleMessage.MessageType.TimerReset:
                    //  restore window title if we reset.
                    WindowTitle = _originalWindowTitle;
                    break;
            }  
        }

        private void OnSettingsChanged()
        {
            var settings = SettingsModelFactory.GetNewSettings();
            TopMost = settings.TopMost;
        }

        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (_currentViewModel == value)
                    return;
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public ICommand TimerViewCommand { get; private set; }
        public ICommand SettingsViewCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        
        private void ExecuteViewTimerCommand()
        {
            CurrentViewModel = _timerViewModel;
        }

        private void ExecuteViewSettingsCommand()
        {
            CurrentViewModel = _settingsViewModel;
        }

        private void ExecuteViewAboutCommand()
        {
            CurrentViewModel = _aboutViewModel;
        }

        private void ExecutePlayCommand()
        {
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.TimerStart });
        }

        private void ExecutePauseCommand()
        {
            Messenger.Default.Send(new SimpleMessage { Type = SimpleMessage.MessageType.TimerStop });
        }

        public bool TopMost
        {
            get
            {
                return _TopMost;
            }
            set
            {
                if (_TopMost == value)
                    return;
                _TopMost = value;
                RaisePropertyChanged("TopMost");
            }
        }

        public string WindowTitle
        {
            get
            {
                return _WindowTitle;
            }
            set
            {
                if (_WindowTitle == value)
                    return;
                _WindowTitle = value;
                RaisePropertyChanged("WindowTitle");
            }
        }
    }
}