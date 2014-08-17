using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedDog.Engine.Sample
{
    public class SendNewsletterJob : Job
    {
        public override TimeSpan Interval
        {
            get { return TimeSpan.FromMinutes(1); }
        }

        /// <summary>
        /// Run the job.
        /// </summary>
        public override void Run()
        {
            Execute(new SendNewsletterTask(new [] { "someone@live.com", "someone@gmail.com" }));
        }
    }


    public class SendNewsletterTask : JobTask
    {
        private readonly string[] _emailAddresses;

        public SendNewsletterTask(string[] emailAddresses)
        {
            _emailAddresses = emailAddresses;
        }

        public void Execute(Guid runId, IJobEventContext eventContext)
        {

        }

        public override void Execute()
        {
            Console.WriteLine("Running SendNewsletterTask on thread {0} and task {1}", Thread.CurrentThread.ManagedThreadId,
                Task.CurrentId);

            foreach (var emailAddress in _emailAddresses)
            {
                ReportProgress("Sending email to: {0}", emailAddress);
            }
        }
    }
}