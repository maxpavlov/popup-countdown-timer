﻿// Copyright 2012 lapthorn.net.
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


namespace Btl.Messaging
{
    /// <summary>
    /// A simple message class that we use to pass messages around the application
    /// via the Messenger singleton, but keep the parts sufficiently decoupled
    /// for the MVVM style of work.
    /// </summary>
    public class SimpleMessage
    {
        public SimpleMessage() : this(MessageType.SwitchToTimerView)
        {
        }

        public SimpleMessage(MessageType type)
            : this(type, string.Empty)
        {
        }

        public SimpleMessage(MessageType type, string message)
        {
            Type = type;
            Message = message;
        }

        public enum MessageType
        {
            SwitchToTimerView,
            SwitchToSettingsView,
            SwitchToAboutView,
            SettingsChanged,
            TimerStop,
            TimerStart,
            TimerTick,
            TimerReset,
            SwitchToPopUpView
        }

        public MessageType Type { get; set; }

        public string Message { get; set; }

    }
}
