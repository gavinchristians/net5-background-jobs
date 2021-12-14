using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundJobs.Data.Entities.Application
{
    public class Job
    {
        // Job Names
        public const string INITIAL_GREETING = "Initial Greeting Mail Message";
        // Job Types
        public const string MAIL_PROCESS = "Mail Process";
        // Job Statuses
        public const string IN_PROGRESS = "In Progress";
        public const string COMPLETED = "Completed";
        public const string CANCELLED = "Cancelled";
        public const string FAILED = "Failed";
        public const string QUEUED = "Queued";

        [Display(Name = "Job Id")]
        public int JobId { get; set; }
        [Display(Name = "Job Name")]
        public string JobName { get; set; }
        [Display(Name = "Job Type")]
        public string JobType { get; set; }
        [Display(Name = "Job Status")]
        public string JobStatus { get; set; }
        [Display(Name = "Job Queued")]
        public DateTime JobQueued { get; set; }
        [Display(Name = "Job Started")]
        public DateTime? JobStarted { get; set; }
        [Display(Name = "Job Ended")]
        public DateTime? JobEnded { get; set; }
        [Display(Name = "Job Progress")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal JobProgress { get; set; }
        [Display(Name = "Job Result")]
        public string JobResult { get; set; }
        [Display(Name = "Started By")]
        public string StartedBy { get; set; }
        [Display(Name = "Last Modified By")]
        public string LastModifiedBy { get; set; }
    }
}
