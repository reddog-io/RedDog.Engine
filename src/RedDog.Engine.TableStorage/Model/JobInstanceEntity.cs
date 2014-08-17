using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace RedDog.Engine.TableStorage.Model
{
    public class JobInstanceEntity : TableEntity
    {
        public string Name
        {
            get;
            set;
        }

        public Guid LastRunId
        {
            get;
            set;
        }

        public string LastRunStatus
        {
            get;
            set;
        }

        public DateTimeOffset LastRunDate
        {
            get;
            set;
        }
    }
}
