using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace Hangfire_Mongo_Bug.Services
{
    public class TestService : ITestService
    {
        private ILogger<TestService> logger;

        public TestService(ILogger<TestService> logger)
        {
            this.logger = logger;
        }

        [Queue(QueueNames.Long)]
        public async Task ExecuteLongTaskAsync(int paramter, CancellationToken cancellationToken, PerformContext context)
        {
            var workerId = string.Empty;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var state = context.Connection.GetStateData(context.BackgroundJob.Id);
                state?.Data?.TryGetValue("WorkerId", out workerId);

                this.logger.LogInformation($"{nameof(ExecuteLongTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - Started");

                for (int i = 0; i < 20000; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    this.logger.LogInformation($"{nameof(ExecuteLongTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - i = {i} - Execution time: {stopwatch.Elapsed}");

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
                
                this.logger.LogInformation($"{nameof(ExecuteLongTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - Finished - Execution time: {stopwatch.Elapsed}");
            }
            catch (OperationCanceledException)
            {
                this.logger.LogError($"{nameof(ExecuteLongTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - Token was cancelled - Execution time: {stopwatch.Elapsed}");
                throw;
            }
        }

        [Queue(QueueNames.Short)]
        public async Task ExecuteShortTaskAsync(int paramter, CancellationToken cancellationToken, PerformContext context)
        {
            var workerId = string.Empty;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var state = context.Connection.GetStateData(context.BackgroundJob.Id);
                state?.Data?.TryGetValue("WorkerId", out workerId);

                this.logger.LogInformation($"{nameof(ExecuteShortTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - Started");

                cancellationToken.ThrowIfCancellationRequested();
                this.logger.LogInformation($"{nameof(ExecuteShortTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - Execution time: {stopwatch.Elapsed}");

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                this.logger.LogInformation($"{nameof(ExecuteShortTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - Finished - Execution time: {stopwatch.Elapsed}");
            }
            catch (OperationCanceledException)
            {
                this.logger.LogError($"{nameof(ExecuteShortTaskAsync)} - Job: {context.BackgroundJob.Id} -  Worker: {workerId} - Token was cancelled - Execution time: {stopwatch.Elapsed}");
                throw;
            }
        }
    }
}
