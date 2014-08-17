using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedDog.Engine.Sample
{
    public class LongRunningJob : Job
    {
        public override TimeSpan Interval
        {
            get { return TimeSpan.FromSeconds(7); }
        }

        /// <summary>
        /// Run the job.
        /// </summary>
        public override void Run()
        {
            Execute(new LongRunningTask());
        }
    }

    public class LongRunningTask : JobTask
    {
        public override void Execute()
        {
            var wait = new Random().Next(1, 30);

            Console.WriteLine("Running LongRunningTask on thread {0} and task {1}. Waiting for: {2} sec.", Thread.CurrentThread.ManagedThreadId,
                Task.CurrentId, wait);

            if (wait % 2 == 0)
            {
                throw new InvalidOperationException("Some random exception.");
            }

            Thread.Sleep(TimeSpan.FromSeconds(wait));
        }
    }
}