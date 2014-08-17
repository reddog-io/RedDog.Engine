using System;
using System.Threading;

namespace RedDog.Engine
{
    public abstract class JobTask
    {
        public Guid RunId
        {
            get;
            internal set;
        }

        public CancellationToken CancellationToken
        {
            get;
            internal set;
        }

        public Job Job
        {
            get;
            internal set;
        }

        public abstract void Execute();

        protected void ReportProgress(string message, params object[] args)
        {
            Job.EventContext.TaskProgress(Job, this, message, args);
        }
    }
}