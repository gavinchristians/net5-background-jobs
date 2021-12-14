using BackgroundJobs.Business.BackgroundTasks;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundJobs.Business.Services
{
    public class QueueService : BackgroundService
    {
        private IBackgroundQueue _queue;
        public QueueService(IBackgroundQueue queue)
        {
            _queue = queue;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                var task = await _queue.PopQueue(stoppingToken);

                await task(stoppingToken);
            }
        }
    }
}
