using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvScraper.Scraper
{
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
