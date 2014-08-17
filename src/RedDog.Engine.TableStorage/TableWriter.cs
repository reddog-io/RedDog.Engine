using System;
using System.Collections.Generic;

using Microsoft.WindowsAzure.Storage.Table;

namespace RedDog.Engine.TableStorage
{
    internal class TableWriter
    {
        private readonly CloudTable _table;

        private bool _tableCreated;

        public TableWriter(CloudTableClient tableClient, string tableName)
        {
            _table = tableClient.GetTableReference(tableName);
        }

        public void Write(string partitionKey, string rowKey, IDictionary<string, EntityProperty> values)
        {
            try
            {
                // Make sure the table exists.
                if (!_tableCreated)
                {
                    Diagnostics.TableStorageEventSource.Log.Info("Ensuring table '{0}' exists.", _table.Name);

                    // Create.
                    _table.CreateIfNotExists();
                    _tableCreated = true;
                }

                // Write.
                _table.Execute(TableOperation.InsertOrMerge(new DynamicTableEntity(partitionKey, rowKey, "*", values)));
            }
            catch (Exception ex)
            {
                Diagnostics.TableStorageEventSource.Log.ErrorDetails(ex, "Error writing to table '{0}'.", _table.Name);
            }
        }
    }
}