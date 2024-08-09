using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ProjectProphecy.ns_Utility
{
    /// <summary>
    /// A timer that needs manual checks (does not trigger when time is over)
    /// </summary>
    public class DateTimer
    {
        public delegate void TimerEvent();
        // --- Properties ---
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public event TimerEvent OnEnd;

        public bool IsOver
        {
            get => DateTime.Now > EndTime;
        }

        public TimeSpan TimeLeft
        {
            get
            {
                if (!IsOver)
                {
                    return EndTime - DateTime.Now;
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        // --- Constructors ---
        public DateTimer(TimeSpan duration)
        {
            Duration = duration;
            StartTime = DateTime.Now;
            EndTime = StartTime + duration;
        }

        public DateTimer(int milliseconds)
        {
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, milliseconds);
            Duration = duration;
            StartTime = DateTime.Now;
            EndTime = StartTime + duration;
        }

        // --- Methods ---
        /// <summary>
        /// Checks if the timer is over and executes the delegate if so
        /// </summary>
        /// <returns>Whether or not the duration time has passed</returns>
        public bool CheckValid(bool restart)
        {
            if (IsOver)
            {
                OnEnd?.Invoke();
                if (restart)
                {
                    Restart();
                }
            }
            return IsOver;
        }

        public void Restart()
        {
            StartTime = DateTime.Now;
            EndTime = StartTime + Duration;
        }
    }
}
