using System;

namespace RedDog.Engine
{
    public interface IJobEventContext
    {
        void JobRegistered(Job job);
        void JobRunning(Job job, DateTime startTime);
        void JobComplete(Job job, DateTime startTime, TimeSpan duration);
        void JobFailed(Job job, DateTime startTime, Exception exception);
        void TaskRunning(Job job, JobTask task, DateTime startTime);
        void TaskProgress(Job job, JobTask task, string message, object[] args);
        void TaskComplete(Job job, JobTask task, DateTime startTime, TimeSpan duration);
        void TaskFailed(Job job, JobTask task, DateTime startTime, Exception exception);
    }
}
