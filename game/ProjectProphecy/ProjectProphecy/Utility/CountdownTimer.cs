using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Utility
{
    /// <summary>
    /// A timer that counts down every frame and executes assigned task when time is over
    /// </summary>
    public class CountdownTimer
    {
        public delegate void TimerEvent();
        // --- Properties ---
        public TimeSpan Duration { get; set; }

        public TimeSpan Counter { get; set; }

        public TimeSpan TimeLeft
        {
            get
            {
                if (Duration > Counter)
                {
                    return Duration - Counter;
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        public event TimerEvent OnEnd;

        public bool IsOver
        {
            get; set;
        }


        // --- Constructors ---
        public CountdownTimer(TimeSpan duration)
        {
            Duration = duration;
            Counter = new TimeSpan(0);
        }

        public CountdownTimer(int milliseconds)
        {
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, milliseconds);
            Duration = duration;
            Counter = new TimeSpan(0);
        }

        // --- Methods ---
        public void Update(GameTime gameTime)
        {
            if (IsOver)
            {
                return;
            }
            Counter = Counter.Add(new TimeSpan(gameTime.ElapsedGameTime.Ticks));
            if (Counter > Duration)
            {
                IsOver = true;
                OnEnd?.Invoke();
            }
        }

        public void Restart()
        {
            Counter = new TimeSpan(0);
        }
    }
}
