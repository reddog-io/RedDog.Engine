using System;
using System.Collections.Generic;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;

using RedDog.Engine.TableStorage.Model;

namespace RedDog.Engine.TableStorage
{
    public class TableStorageJobEventContext : IJobEventContext
    {
        private readonly TableWriter _jobsWriter;
        private readonly TableWriter _jobInstancesWriter;
        private readonly TableWriter _jobInstanceEventsWriter;

        public TableStorageJobEventContext(CloudStorageAccount storageAccount, string jobsTableName = "Jobs", string jobInstancesTableName = "JobInstances", string jobInstanceEventsTableName = "JobInstanceEvents")
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            _jobsWriter = new TableWriter(tableClient, jobsTableName);
            _jobInstancesWriter = new TableWriter(tableClient, jobInstancesTableName);
            _jobInstanceEventsWriter = new TableWriter(tableClient, jobInstanceEventsTableName);
        }

        /// <summary>
        /// Store all jobs in a table so that we can list them.
        /// </summary>
        /// <param name="job"></param>
        public void JobRegistered(Job job)
        {
            _jobsWriter.Write("", job.Name, new Dictionary<string, EntityProperty>
            {
                { "Name", new EntityProperty(job.Name) }
            });
        }

        public void JobRunning(Job job, DateTime startTime)
        {
            // Update instance.
            _jobInstancesWriter.Write(job.Name, job.RunId.ToString(), new Dictionary<string, EntityProperty>()
            {
                { "StartTime", new EntityProperty(startTime) },
                { "Status", new EntityProperty(JobStatus.Running) },
            });

            // Update job.
            _jobsWriter.Write("", job.Name, new Dictionary<string, EntityProperty>
            {
                { "Name", new EntityProperty(job.Name) },
                { "LastRunId", new EntityProperty(job.RunId) },
                { "LastRunStart", new EntityProperty(startTime) },
                { "LastRunStatus", new EntityProperty(JobStatus.Running) },
                { "LastRunErrorMessage", new EntityProperty(String.Empty) }
            });
        }

        public void JobComplete(Job job, DateTime startTime, TimeSpan duration)
        {
            // Update instance.
            _jobInstancesWriter.Write(job.Name, job.RunId.ToString(), new Dictionary<string, EntityProperty>()
            {
                { "StartTime", new EntityProperty(startTime) },
                { "Status", new EntityProperty(JobStatus.Success) },
                { "Duration", new EntityProperty(duration.ToString()) },
            });

            // Update job.
            _jobsWriter.Write("", job.Name, new Dictionary<string, EntityProperty>
            {
                { "Name", new EntityProperty(job.Name) },
                { "LastRunId", new EntityProperty(job.RunId) },
                { "LastRunStart", new EntityProperty(startTime) },
                { "LastRunStatus", new EntityProperty(JobStatus.Success) },
                { "LastRunDuration", new EntityProperty(duration.ToString()) },
                { "LastRunErrorMessage", new EntityProperty(String.Empty) }
            });
        }

        public void JobFailed(Job job, DateTime startTime, Exception exception)
        {
            // Update instance.
            _jobInstancesWriter.Write(job.Name, job.RunId.ToString(), new Dictionary<string, EntityProperty>()
            {
                { "StartTime", new EntityProperty(startTime) },
                { "Status", new EntityProperty(JobStatus.Failed) },
                { "ErrorMessage", new EntityProperty(exception.Message) },
                { "ErrorType", new EntityProperty(exception.GetType().ToString()) },
                { "ErrorStackTrace", new EntityProperty(exception.StackTrace) },
                { "ErrorObject", new EntityProperty(JsonConvert.SerializeObject(exception)) }
            });

            // Update job.
            _jobsWriter.Write("", job.Name, new Dictionary<string, EntityProperty>
            {
                { "Name", new EntityProperty(job.Name) },
                { "LastRunId", new EntityProperty(job.RunId) },
                { "LastRunStart", new EntityProperty(startTime) },
                { "LastRunStatus", new EntityProperty(JobStatus.Failed) },
                { "LastRunErrorMessage", new EntityProperty(exception.Message) }
            });
        }

        public void TaskRunning(Job job, JobTask task, DateTime startTime)
        {
            TaskProgress(job, task, null, "Started task '{0}' at {1}.", task.GetType().Name, startTime);
        }

        public void TaskProgress(Job job, JobTask task, string message, params object[] args)
        {
            TaskProgress(job, task, null, message, args);
        }

        public void TaskProgress(Job job, JobTask task, Exception exception, string message, params object[] args)
        {
            var rowKey = String.Format("{0}-{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid());

            // Write event.
            _jobInstanceEventsWriter.Write(job.RunId.ToString(), rowKey, new Dictionary<string, EntityProperty>()
            {
                { "Task", new EntityProperty(task.GetType().Name) },
                { "Message", new EntityProperty(String.Format(message, args)) },
                { "ErrorMessage", new EntityProperty(exception != null ? exception.Message : null) },
                { "ErrorType", new EntityProperty(exception != null ? exception.GetType().ToString() : null) },
                { "ErrorStackTrace", new EntityProperty(exception != null ? exception.StackTrace : null) },
                { "ErrorObject", new EntityProperty(exception != null ? JsonConvert.SerializeObject(exception) : String.Empty) }
            });
        }

        public void TaskComplete(Job job, JobTask task, DateTime startTime, TimeSpan duration)
        {
            TaskProgress(job, task, null, "Completed task '{0}' in {1}.", task.GetType().Name, duration);
        }

        public void TaskFailed(Job job, JobTask task, DateTime startTime, Exception exception)
        {
            TaskProgress(job, task, exception, "Task '{0}' failed: {1}.", task.GetType().Name, exception.Message);
        }
    }
}
