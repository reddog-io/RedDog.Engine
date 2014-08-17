using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Threading;
using RedDog.Engine.Diagnostics;

namespace RedDog.Engine
{
    public class JobHost : IDisposable, IJobHost
    {
        private readonly IJobEventContext _eventContext;

        private readonly AsyncSubject<Unit> _shutdownSignal;

        public event EventHandler Stopped;

        public bool IsStopped
        {
            get;
            private set;
        }

        public IDictionary<string, Job> Jobs
        {
            get;
            private set;
        }

        public CancellationTokenSource CancellationToken
        {
            get;
            private set;
        }

        public JobHost()
            : this(new NullJobEventContext())
        {
            
        }

        public JobHost(IJobEventContext eventContext)
        {
            // Job container.
            Jobs = new Dictionary<string, Job>();

            // Store the event context that will be used to track jobs and tasks.
            _eventContext = eventContext;

            // Signal 
            _shutdownSignal = new AsyncSubject<Unit>();
            _shutdownSignal.OnNext(Unit.Default);
        }

        /// <summary>
        /// Add a job to the runner.
        /// </summary>
        /// <param name="job.Name"></param>
        /// <param name="job"></param>
        public void Add(Job job)
        {
            _eventContext.JobRegistered(job);

            // Add the job to the job list.
            Jobs.Add(job.Name, job);

            // Store the event context in the job, so it can also send events.
            job.EventContext = _eventContext;
        }

        /// <summary>
        /// Initialize every job.
        /// </summary>
        public void Initialize()
        {
            InitializeJobs(Jobs.ToArray());
        }

        /// <summary>
        /// Initialize every job.
        /// </summary>
        private void InitializeJobs(KeyValuePair<string, Job>[] jobs)
        {
            EngineEventSource.Log.Info("Initializing {0} jobs.", jobs.Length);

            // Initialize every job.
            foreach (var job in jobs)
            {
                try
                {
                    // Log.
                    EngineEventSource.Log.Info("Initializing '{0}'.", job.Key);

                    // Initialize the job.
                    job.Value.Initialize();

                    // Log.
                    EngineEventSource.Log.Info("Initialization complete for '{0}'.", job.Key);
                }
                catch (Exception ex)
                {
                    EngineEventSource.Log.ErrorDetails(ex, "Error initializing job '{0}'.", job.Key);
                    throw;
                }
            }
        }

        /// <summary>
        /// Start every job.
        /// </summary>
        public void Start()
        {
            EngineEventSource.Log.Info("Starting job host.");

            // Reset the cancellation token.
            if (CancellationToken != null)
                CancellationToken.Cancel();
            CancellationToken = new CancellationTokenSource();
           
            // Schedule.
            ScheduleJobs(Jobs);
        }

        /// <summary>
        /// Schedule every job.
        /// </summary>
        /// <param name="jobs"></param>
        private void ScheduleJobs(IDictionary<string, Job> jobs)
        {
            EngineEventSource.Log.Info("Scheduling {0} jobs.", jobs.Count);

            // Create schedules.
            IDisposable[] subscriptions;
            try
            {
                subscriptions = jobs.Select(job => ScheduleJob(job.Value)).ToArray();
            }
            catch (Exception ex)
            {
                EngineEventSource.Log.ErrorDetails(ex, "Error scheduling jobs.");
                throw;
            }

            // Log.
            EngineEventSource.Log.Info("Job scheduling complete.");

            // Wait for stop.
            _shutdownSignal.Wait();

            // Log.
            EngineEventSource.Log.Info("Shutting down jobs.");

            // Stop every timer.
            foreach (var token in subscriptions)
            {
                token.Dispose();
            }
        }

        /// <summary>
        /// Schedule a single job.
        /// </summary>
        /// <param name="job.Name"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        private IDisposable ScheduleJob(Job job)
        {
            var startTime = DateTimeOffset.UtcNow + job.StartOffset;

            // Log.
            EngineEventSource.Log.Info("Scheduled job '{0}' to run every {1} (starting: {2}).",
                job.Name, job.Interval, job.StartOffset);

            // Create an infinite loop.
            return Observable.Timer(startTime, job.Interval).Subscribe(_ => RunJob(job));
        }

        /// <summary>
        /// Run a single job.
        /// </summary>
        /// <param name="job"></param>
        private void RunJob(Job job)
        {
            var runId = Guid.NewGuid();
            var startTime = DateTime.UtcNow;

            try
            {
                job.CancellationToken = CancellationToken.Token;
                job.RunId = runId;

                // Log start.
                EngineEventSource.Log.Info("Running job '{0}' at {1}.", job.Name, startTime);

                // Notify event.
                _eventContext.JobRunning(job, startTime);

                // Execute the job.
                job.Run();

                // Total duration.
                var duration = (DateTime.UtcNow - startTime);

                // Notify event.
                _eventContext.JobComplete(job, startTime, duration);

                // Log end.
                EngineEventSource.Log.Info("Job run complete for '{0}' in {1}.", job.Name, duration);
            }
            catch (Exception ex)
            {
                // Notify event.
                _eventContext.JobFailed(job, startTime, ex);

                // Log.
                EngineEventSource.Log.ErrorDetails(ex, "Error running job '{0}'.", job.Name);
            }
        }

        /// <summary>
        /// Stop the job runner.
        /// </summary>
        public void Stop()
        {
            CancellationToken.Cancel();

            // Stop the timer.
            _shutdownSignal.OnCompleted();

            // Mark the host as stopped.
            IsStopped = true;

            try
            {
                // Raise stopped event.
                if (Stopped != null)
                    Stopped(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                EngineEventSource.Log.CriticalDetails(ex, "Error stopping jobs.");
            }

            // Log stop complete.
            EngineEventSource.Log.Info("All jobs have been stopped.");
        }

        public void Dispose()
        {
            _shutdownSignal.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
