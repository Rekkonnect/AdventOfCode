using System.Collections.Generic;

namespace AdventOfCode.Utilities
{
    // TODO: To Garyon?
    /// <summary>Represents a cooldown queue.</summary>
    public class CooldownQueue
    {
        private Queue<double> callQueue;
        private double rate;

        public CooldownQueue(int calls, double rate)
        {
            callQueue = new Queue<double>(calls);
            this.rate = rate;
        }

        /// <summary>Attempts to register a call at a given timestamp, and registers it if it can be called.</summary>
        /// <param name="timestamp">The timestamp at which the call is performed.</param>
        /// <returns><see langword="true"/> if the call can be performed, and is successfully registered, otherwise <see langword="false"/>.</returns>
        public bool ConditionallyRegisterCall(double timestamp)
        {
            if (callQueue.Peek() + rate < timestamp)
                return false;

            callQueue.Dequeue();
            callQueue.Enqueue(timestamp);
            return true;
        }
    }
}
