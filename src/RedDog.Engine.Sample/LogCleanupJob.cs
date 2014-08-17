using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedDog.Engine.Sample
{
    public class LogCleanupJob : Job
    {
        public override TimeSpan Interval
        {
            get { return TimeSpan.FromSeconds(15); }
        }

        /// <summary>
        /// Run the job.
        /// </summary>
        public override void Run()
        {
            Execute(new LogCleanupTask());
        }
    }

    public class LogCleanupTask : JobTask
    {
        public override void Execute()
        {
            Console.WriteLine("Running LogCleanupTask on thread {0} and task {1}", Thread.CurrentThread.ManagedThreadId,
                Task.CurrentId);
        }
    }
}