using BackgroundJobs.Data.Entities.Application;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static BackgroundJobs.Tests.SliceFixture;

namespace BackgroundJobs.Tests.Business
{
    public class JobManagerTests : IntegrationTestBase
    {
        [Fact]
        public async Task Should_cancel_all_jobs()
        {
            // Arrange
            var jobs = new List<Job>()
            {
                new Job { JobName = Job.INITIAL_GREETING, JobType = Job.MAIL_PROCESS, JobQueued = DateTime.Now, JobStarted = DateTime.Now.AddSeconds(1), JobStatus = Job.IN_PROGRESS, StartedBy = "TestUser" }
            };
            await InsertAsync(jobs[0]);

            // Act
            await _jobManager.CancelAllJobs(Job.MAIL_PROCESS, "Job cancelled due to testing.", "TestUser");

            // Assert
            var job1 = await _jobManager.GetJob(jobs[0].JobId);
            job1.JobStatus.ShouldBe(Job.CANCELLED);

        }
    }
}
