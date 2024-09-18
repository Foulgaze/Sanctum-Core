using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Server
{
    /// <summary>
    /// Class used to time certain aspects of lobby/server maintainence
    /// </summary>
    public class CountdownTimer
    {
        private DateTime lastCheckedTime;
        private readonly double timeToWait;

        public CountdownTimer(double timeToWait = 0.5)
        {
            this.lastCheckedTime = DateTime.Now;
            this.timeToWait = timeToWait;
        }

        public bool HasTimerPassed()
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan timeElapsed = currentTime - this.lastCheckedTime;

            if (timeElapsed.TotalMinutes >= this.timeToWait)
            {
                this.lastCheckedTime = currentTime;
                return true;
            }

            return false;
        }
    }
}
