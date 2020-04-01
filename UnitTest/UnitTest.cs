using Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using COVI19_Function;
using UnitTestHelper;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async System.Threading.Tasks.Task TestCheckGithubAsyncAsync()
        {

            var logger = new MockLogger();
            var timerInfo = new TimerInfo(new MockSchedule(), null);
            var context = new Microsoft.Azure.WebJobs.ExecutionContext()
            {
                InvocationId = Guid.NewGuid(),
                FunctionAppDirectory = $@"C:\code\COVID19\COVID19_Azure_DataSync\COVI19_Timer_Function\bin\Debug\netcoreapp2.1"
            };


            IConfigurationRoot config = new ConfigurationBuilder()
                   .SetBasePath(context.FunctionAppDirectory)
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .Build();

            var AzureWebJobsStorage = config.GetValue<string>("Values:AzureWebJobsStorage");

            var covidTable = Helper.GetCloudTable("Covid19", AzureWebJobsStorage, logger, context);
            var syncTable = Helper.GetCloudTable("SyncStatus", AzureWebJobsStorage, logger, context);
            var errorTable = Helper.GetCloudTable("ErrorLogs", AzureWebJobsStorage, logger, context);

            ICollector<string> queue = new MockCollector<string>();


            var function = new COIVD19_Api(new COVIDService(new System.Net.Http.HttpClient()));
            await function.CheckGithubAsync(timerInfo, syncTable, queue, logger);

        }

    }
}
