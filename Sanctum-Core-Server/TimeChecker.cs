using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctum_Core_Server
{
    public class TimeChecker
    {
        private DateTime lastCheckedTime;
        private readonly double timeToWait;

        public TimeChecker(double timeToWait = 0.5)
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
