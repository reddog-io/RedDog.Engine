using System;
using System.Threading;

using RedDog.Engine.Diagnostics;

namespace RedDog.Engine
{
    public abstract class Job
    {
        public virtual string Name
        {
            get { return GetType().Name; }
        }

        public Guid RunId
        {
            get;
            set;
        }

        public virtual TimeSpan Interval
        {
            get { return TimeSpan.FromDays(1); }
        }


        public virtual TimeSpan StartOffset
        {
            get { return TimeSpan.Zero; }
        }

        public IJobEventContext EventContext
        {
            get;
            internal set;
        }

        public CancellationToken CancellationToken
        {
            get;
            internal set;
        }

        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Execute a task.
        /// </summary>
        /// <param name="task"></param>
        public void Execute(JobTask task)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // Log.
                EngineEventSource.Log.Verbose("Executing task '{0}' at {1}.", task.GetType().Name, startTime);

                // Send out event.
                EventContext.TaskRunning(this, task, startTime);

                // Execute the task.
                task.Job = this;
                task.CancellationToken = CancellationToken;
                task.Execute();

                // Calculate duration.
                var duration = DateTime.UtcNow - startTime;

                // Send out event.
                EventContext.TaskComplete(this, task, startTime, duration);

                // Log complete.
                EngineEventSource.Log.Verbose("Executed task '{0}' in {1}.", task.GetType().Name, duration.ToString());
            }
            catch (Exception ex)
            {
                EventContext.TaskFailed(this, task, startTime, ex);

                // Log.
                EngineEventSource.Log.ErrorDetails(ex, "Error executing task '{0}'", task.GetType().Name);

                throw;
            }
        }

        /// <summary>
        /// Run the job.
        /// </summary>
        public abstract void Run();
    }
}