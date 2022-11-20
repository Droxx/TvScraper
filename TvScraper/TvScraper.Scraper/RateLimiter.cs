using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvScraper.Scraper
{
    /// <summary>
    /// The RateLimiter class stores a list of timestamps for previous calls
    /// once the count has been reached, we wait until one of those timestamps is 
    /// as old as a specified period. We then release that item in the queue, and allow the wait call to return
    /// </summary>
    internal class RateLimiter
    {
        private readonly int count;
        private readonly TimeSpan period;

        public RateLimiter(int count, TimeSpan period) 
        {
            this.count = count;
            this.period = period;
        }

        private readonly Queue<DateTime> timestamps  = new Queue<DateTime>();

        /// <summary>
        /// When called will only return once a space in the limit queue is free
        /// </summary>
        /// <returns></returns>
        public async Task Wait()
        {
            var delay = TimeSpan.Zero;
            lock (this)
            {
                while(timestamps.Count > count)
                {
                    timestamps.Dequeue();
                }
                var now = DateTime.UtcNow;
                if(timestamps.Count == count)
                {
                    var gateTime = timestamps.Peek();
                    if(gateTime > now)
                    {
                        delay = gateTime.Subtract(now);
                    }
                }
            }
            if(delay != TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }
            lock (this)
            {
                timestamps.Enqueue(DateTime.UtcNow.Add(period));
            }
        }
    }
}
