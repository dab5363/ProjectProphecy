using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy
{
    /// <summary>
    /// Exception that involves a major issue that can't ensure the game to be run and work properly,
    /// and that there's no need to continue the game because it'll be broken otherwise.
    /// </summary>
    public class MajorIssueException : Exception
    {
        // --- Constructors ---
        /// <summary>
        /// Just an Exception!
        /// </summary>
        public MajorIssueException()
        {
        }
        /// <summary>
        /// Instantiates the Exception with an error message
        /// </summary>
        public MajorIssueException(string message) : base(message)
        {
        }
    }
}
