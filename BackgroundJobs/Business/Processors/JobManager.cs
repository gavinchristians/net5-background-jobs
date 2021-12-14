using BackgroundJobs.Data.DbContexts;
using BackgroundJobs.Data.Entities.Application;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundJobs.Business.Processors
{
    public class JobManager : IJobManager
    {
        private readonly ApplicationDbContext _db;
        public JobManager(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Job> GetJob(int jobId)
        {
            return await _db.Jobs.Where(x => x.JobId.Equals(jobId)).FirstOrDefaultAsync();
        }
        public async Task SetJobToInProgress(Job job)
        {
            job.JobStarted = DateTime.Now;
            job.JobStatus = Job.IN_PROGRESS;
            _db.Update(job);
            await _db.SaveChangesAsync();
        }
        public async Task FailJob(Job job, string error)
        {
            job.JobStatus = Job.FAILED;
            job.JobEnded = DateTime.Now;
            job.JobResult = error;
            _db.Update(job);
            await _db.SaveChangesAsync();
        }
        public async Task CancelJob(Job job, string user)
        {
            job.JobStatus = Job.CANCELLED;
            job.JobEnded = DateTime.Now;
            job.JobResult = $"<p>Job cancelled by {user}.</p>";
            job.LastModifiedBy = user;
            _db.Update(job);
            await _db.SaveChangesAsync();
        }
        public async Task<bool> IsJobCancelled(int jobId)
        {
            try
            {
                var job = await _db.Jobs.AsNoTracking().Where(x => x.JobId.Equals(jobId)).FirstOrDefaultAsync();
                if (job != null)
                    return job.JobStatus.Equals(Job.CANCELLED);
                else
                    return true;
            }
            catch (Exception)
            {
                // Log error
                return true;
            }
        }
        public async Task CompleteJob(Job job, string result)
        {
            // Get current job status
            var jobStatus = await _db.Jobs
                .AsNoTracking()
                .Where(x => x.JobId.Equals(job.JobId))
                .Select(s => s.JobStatus)
                .FirstOrDefaultAsync();

            // Check if job is in progress
            if (jobStatus.Equals(Job.IN_PROGRESS))
            {
                job.JobStatus = Job.COMPLETED;
                job.JobEnded = DateTime.Now;
                job.JobResult = result;
                _db.Update(job);
                await _db.SaveChangesAsync();
            }
        }
        public async Task<Job> QueueJob(string jobName, string jobType, string user)
        {
            var job = new Job
            {
                JobName = jobName,
                JobType = jobType,
                JobQueued = DateTime.Now,
                JobStatus = Job.QUEUED,
                StartedBy = user
            };
            _db.Add(job);
            await _db.SaveChangesAsync();

            return job;
        }

        public async Task<Job> CheckForDuplicateJob(string jobName, string jobType, DateTime date)
        {
            return await _db.Jobs.AsNoTracking()
                           .OrderByDescending(x => x.JobQueued)
                           .Where(x =>
                               x.JobName.Equals(jobName) &&
                               x.JobType.Equals(jobType) &&
                               x.JobQueued.ToString("dd/MM/yyyy").Equals(date.ToString("dd/MM/yyy"))
                           )
                           .FirstOrDefaultAsync();
        }

        public async Task CancelAllJobs(string jobType, string result, string user)
        {
            // Cancel statuses
            string[] statuses = new string[]
            {
                Job.QUEUED,
                Job.IN_PROGRESS
            };
            var jobs = await _db.Jobs
                        .OrderByDescending(x => x.JobQueued)
                        .Where(x => statuses.Any(s => x.JobStatus.Equals(s)) &&
                            x.JobType.Equals(jobType)
                        ).ToListAsync();

            string message = "CancelAllJobs; JobIds: ";
            jobs.ForEach(x =>
            {
                x.JobStatus = Job.CANCELLED;
                x.JobEnded = DateTime.Now;
                x.JobResult = result;
                x.LastModifiedBy = user;

                message += $"{x.JobId}, ";
            });
            _db.UpdateRange(jobs);
            await _db.SaveChangesAsync();

        }
    }
    public interface IJobManager
    {
        Task<Job> GetJob(int jobId);
        Task SetJobToInProgress(Job job);
        Task FailJob(Job job, string error);
        Task CancelJob(Job job, string user);
        Task<bool> IsJobCancelled(int jobId);
        Task CompleteJob(Job job, string result);
        Task<Job> QueueJob(string jobName, string jobType, string user);
        Task<Job> CheckForDuplicateJob(string jobName, string jobType, DateTime date);
        Task CancelAllJobs(string jobType, string result, string user);
    }
}
