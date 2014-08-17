using System;

namespace RedDog.Engine
{
    public class NullJobEventContext : IJobEventContext
    {
        public void JobRegistered(Job job)
        {
            
        }

        public void JobRunning(Job job, DateTime startTime)
        {
            
        }

        public void JobComplete(Job job, DateTime startTime, TimeSpan duration)
        {
            
        }

        public void JobFailed(Job job, DateTime startTime, Exception exception)
        {
            
        }

        public void TaskRunning(Job job, JobTask task, DateTime startTime)
        {
            
        }

        public void TaskProgress(Job job, JobTask task, string message, object[] args)
        {
            
        }

        public void TaskComplete(Job job, JobTask task, DateTime startTime, TimeSpan duration)
        {
            
        }

        public void TaskFailed(Job job, JobTask task, DateTime startTime, Exception exception)
        {
            
        }
    }
}