using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy
{
    /// <summary>
    /// When something inherited or implemented that actually has no need is mistakenly used,
    /// Throw this exception to draw attention.
    /// </summary>
    class YouDontNeedThisException : Exception
    {
        // --- Constructors ---
        /// <summary>
        /// Just an Exception!
        /// </summary>
        public YouDontNeedThisException()
        {
        }
        /// <summary>
        /// Instantiates the Exception with an error message
        /// </summary>
        public YouDontNeedThisException(string message) : base(message)
        {
        }
    }
}
