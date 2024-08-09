using Microsoft.Xna.Framework.Input;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ProjectProphecy.ns_Controls
{
    /// <summary>
    /// Handles all interactions input by the mouse and keyboard
    /// </summary>
    public class InputManager
    {
        // Singleton of the manager class
        private static readonly Lazy<InputManager> manager = new Lazy<InputManager>(() => new InputManager());
        private KeyboardState kbState;                                     // Current KeyboardState
        private MouseState msState;                                        // Current MouseState
        private KeyboardState prevKeyboardState;                           // Previous KeyboardState
        private MouseState prevMouseState;                                 // Previous MouseState
        public readonly Dictionary<Keys, DateTime> keyLastPressedTime     // Stores times when the keys were last pressed
                         = new Dictionary<Keys, DateTime>();
        private readonly Dictionary<Keys, DateTime> keyPressNoReactionTime // Stores times when the keys were last pressed
                         = new Dictionary<Keys, DateTime>();

        /// <summary>
        /// Use this property to call manager functions.
        /// </summary>
        public static InputManager Singleton
        {
            get => manager.Value;
        }

        /// <summary>
        /// Keyboard state of current frame (Read only)
        /// </summary>
        public KeyboardState Keyboard
        {
            get => kbState;
        }

        /// <summary>
        /// Mouse state of current frame (Read only)
        /// </summary>
        public MouseState Mouse
        {
            get => msState;
        }

        /// <summary>
        /// Keyboard state of previous frame (Read only)
        /// </summary>
        public KeyboardState PrevKeyboard
        {
            get => prevKeyboardState;
        }

        /// <summary>
        /// Mouse state of previous frame (Read only)
        /// </summary>
        public MouseState PrevMouse
        {
            get => prevMouseState;
        }

        /// <summary>
        /// Logically, gets the current input states at the start of frame. Stores the input states as previous
        /// to be used in the next frame. However, the Storing process is done at the start of a frame, on top
        /// of refreshing the current states. (That actually works better and doesn't break the method apart.)
        /// </summary>
        public void Update()
        {
            // Stores the input states from last frame into previous-state fields, so we don't need
            // to break the method into two parts, Get at the start and Store at the end of update.
            prevKeyboardState = kbState;
            prevMouseState = msState;
            // Gets the current input states
            kbState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            msState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            // Updates the last pressed times of all the pressed keys in the dictionary.
            foreach (Keys key in kbState.GetPressedKeys())
            {
                if (!IsHolding(key) && CanBeRecorded(key))
                {
                    Logger.Log($"Logged new key press for {key}!");
                    keyLastPressedTime[key] = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Checks for single key press
        /// </summary>
        /// <param name="key"> key being checked </param>
        /// <param name="onHold">If true, the key press is considered down in previous and up in current,
        /// allowing holding for a while; Otherwise, it's up in previous and down in current, which means
        /// instant response.</param> 
        /// <returns> If user has just pressed the key </returns>
        public bool IsPressed(Keys key, bool onHold = true)
        {
            if (onHold)
            {
                return prevKeyboardState.IsKeyDown(key) && kbState.IsKeyUp(key);
            }
            else
            {
                return prevKeyboardState.IsKeyUp(key) && kbState.IsKeyDown(key);
            }
        }

        /// <summary>
        /// Returns if the specified key has been pressed in given milliseconds.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="milliseconds"></param>
        /// <param name="consume"> If to remove the data stored in lastPressed before
        /// <param name="noReactionTime"> For how long the key would not be recorded into the lastPressedTime dictionary
        /// this returns true so that next time when called it does not pass again. </param>
        /// <returns> If the key has been pressed in given time period </returns>
        public bool HasPressed(Keys key, long milliseconds, bool consume, long noReactionTime = 50)
        {
            if (!keyLastPressedTime.ContainsKey(key))
            {
                return false;
            }
            DateTime lastPressedTime = keyLastPressedTime[key];
            bool pressed = (DateTime.Now - lastPressedTime).TotalMilliseconds <= milliseconds;
            if (pressed)
            {
                if (consume)
                {
                    ConsumeKeyPress(key);
                }
                SetKeyPressNoReactionTime(key, noReactionTime);
            }
            return pressed;
        }

        /// <summary>
        /// Removes the specified key from the keyLastPressedTime dictionary so
        /// that it would no longer store the key as it has been pressed in any
        /// time until it's pressed next time.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ConsumeKeyPress(Keys key)
        {
            if (!keyLastPressedTime.ContainsKey(key))
            {
                return false;
            }
            keyLastPressedTime.Remove(key);
            return true;
        }

        /// <summary>
        /// Sets the no reaction time for press on a specified key.
        /// The key would not be recorded after the time interval.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="milliseconds"></param>
        public void SetKeyPressNoReactionTime(Keys key, long milliseconds)
        {
            keyPressNoReactionTime[key] = DateTime.Now + new TimeSpan(milliseconds * TimeSpan.TicksPerMillisecond);
        }

        /// <summary>
        /// If a key press of the specified key can be recorded
        /// </summary>
        /// <param name="key"></param>
        /// <param name="milliseconds"></param>
        public bool CanBeRecorded(Keys key)
        {
            if (!keyPressNoReactionTime.ContainsKey(key))
            {
                return true;
            }
            return DateTime.Now > keyPressNoReactionTime[key];
        }

        /// <summary>
        /// Checks if the player is pressing the key in this instant (equivilant to IsKeyDown)
        /// </summary>
        /// <param name="key"> Key being checked </param>
        /// <returns> If user has key held down </returns>
        public bool IsPressing(Keys key)
        {
            return kbState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the player is holding the key (both in previous and current frame)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsHolding(Keys key)
        {
            return prevKeyboardState.IsKeyDown(key) && kbState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks for single mouse button press (down in previous and up in current) 
        /// </summary>
        /// <param name="button"> Mouse button being checked </param>
        /// <returns> If user has just pressed the mouse button </returns>
        public bool IsPressed(MouseButtons button)
        {
            // Using reflection to check the status of the property of the same name
            Type mouse = typeof(MouseState);
            return
                (ButtonState)mouse.GetProperty(button.ToString()).GetValue(prevMouseState) == ButtonState.Pressed
                && (ButtonState)mouse.GetProperty(button.ToString()).GetValue(msState) == ButtonState.Released;
        }

        /// <summary>
        /// Checks if the player is pressing the mouse button in this instant (equivilant to ButtonState.Pressed)
        /// </summary>
        /// <param name="button"> Mouse button being checked</param>
        /// <returns> If user has mouse button held down </returns>
        public bool IsPressing(MouseButtons button)
        {
            Type mouse = typeof(MouseState);
            return (ButtonState)mouse.GetProperty(button.ToString()).GetValue(msState) == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if the player is holding the mouse button (both in previous and current frame)
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsHolding(MouseButtons button)
        {
            Type mouse = typeof(MouseState);
            return
                (ButtonState)mouse.GetProperty(button.ToString()).GetValue(prevMouseState) == ButtonState.Pressed
                && (ButtonState)mouse.GetProperty(button.ToString()).GetValue(msState) == ButtonState.Pressed;
        }
    }

    /// <summary>
    /// Available mouse buttons
    /// </summary>
    public enum MouseButtons
    {
        LeftButton,
        MiddleButton,
        RightButton,
        XButton1,
        XButton2
    }
}
