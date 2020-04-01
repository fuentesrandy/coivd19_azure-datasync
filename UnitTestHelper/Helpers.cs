
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestHelper
{

    public static class Helper
    {

        public static CloudTable GetCloudTable(string TableName
         , string AzureWebJobsStorage
         , Microsoft.Extensions.Logging.ILogger log
         , Microsoft.Azure.WebJobs.ExecutionContext context)
        {

            var cloudStorageAccount = CloudStorageAccount.Parse(AzureWebJobsStorage);
            var client = cloudStorageAccount.CreateCloudTableClient();
            var tableClient = client.GetTableReference(TableName);
            return tableClient;

        }
    }


    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope() { }

        public void Dispose() { }
    }

    public class MockLogger : Microsoft.Extensions.Logging.ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            Debug.WriteLine(message);
        }
    }

    public class MockSchedule : TimerSchedule
    {
        public override bool AdjustForDST => throw new NotImplementedException();

        public override DateTime GetNextOccurrence(DateTime now)
        {
            throw new NotImplementedException();
        }


    }


    public class MockCollector<T> : ICollector<T>
    {
        public readonly List<T> Items = new List<T>();
        public void Add(T item)
        {
            Items.Add(item);
        }
    }

    public class MockAsyncCollector<T> : IAsyncCollector<T>
    {
        public readonly List<T> Items = new List<T>();

        public Task AddAsync(T item, CancellationToken cancellationToken = default(CancellationToken))
        {
            Items.Add(item);
            return Task.FromResult(true);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(true);
        }
    }

    public class MockCloudBlockBlob : CloudBlockBlob
    {
        public string fildata;

        public MockCloudBlockBlob()
            : base(new Uri("https://tempuri.org/container/blob"))
        {

        }

        public override Task<bool> ExistsAsync()
        {
            return Task.FromResult(true);
        }

        public override Task<string> DownloadTextAsync()
        {
            return Task.FromResult(JsonConvert.SerializeObject(this));
        }

        public override Task UploadTextAsync(string content)
        {
            fildata = content;
            return Task.CompletedTask;
        }
    }

    public class MockCloudBlobContainer : CloudBlobContainer
    {
        public MockCloudBlockBlob CloudBlockBlob;

        public MockCloudBlobContainer()
            : base(new Uri("https://tempuri.org/container/blob"))
        {
            CloudBlockBlob = new MockCloudBlockBlob();
        }

        public override Task<bool> ExistsAsync()
        {
            return Task.FromResult(true);
        }

        public override CloudBlockBlob GetBlockBlobReference(string blobName)
        {

            return CloudBlockBlob;
        }

    }

}
