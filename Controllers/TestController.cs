using System.Threading;
using Hangfire;
using Hangfire_Mongo_Bug.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire_Mongo_Bug.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestService testService;
        private readonly IBackgroundJobClient backgroundJobs;

        public TestController(ITestService testService, IBackgroundJobClient backgroundJobs)
        {
            this.testService = testService;
            this.backgroundJobs = backgroundJobs;
        }

        [HttpPost]
        [Route("LongJobtesting")]
        [AllowAnonymous]
        public void PostLongJob(int parameter)
        {
            this.backgroundJobs.Enqueue(() => this.testService.ExecuteLongTaskAsync(parameter, CancellationToken.None, null));
        }

        [HttpPost]
        [Route("ShortJobtesting")]
        [AllowAnonymous]
        public void PostShortJob(int parameter)
        {
            this.backgroundJobs.Enqueue(() => this.testService.ExecuteShortTaskAsync(parameter, CancellationToken.None, null));
        }
    }
}
