using Microsoft.VisualStudio.TestTools.UnitTesting;
using Azure.Data.Tables;
using Azure;
using System;
using Microsoft.Extensions.Configuration;
using static Resume.Functions.get_views_fn;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Resume
{
    [TestClass]
    public class AzFunctionTest
    {
        private static string connString;
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("local.settings.json", true, true).AddEnvironmentVariables().Build();
            Environment.SetEnvironmentVariable("connection_string", config["Values:connection_string"]);
            connString = Environment.GetEnvironmentVariable("connection_string",EnvironmentVariableTarget.Process);
        }

        [TestMethod]
        public async Task CheckViewCountIncreases()
        {
            int resultBeforeFunctionCall = 0;
            int resultAfterFunctionCall = 0;
            string tableName = "views";
            var httpContext = new DefaultHttpContext();
            
            Assert.IsTrue(connString != null, $"connection should not be null!");
            
            //get initial count
            TableClient tableClient = new TableClient(connString, tableName);
            
            Pageable<TableEntity> results = tableClient.Query<TableEntity>(entity => entity.PartitionKey == "views");

            foreach (TableEntity tableEntity in results)
            {
                resultBeforeFunctionCall = tableEntity.views;
            }

            //call function to get new count
            resultAfterFunctionCall = await Run(httpContext.Request);

            Assert.IsTrue(resultBeforeFunctionCall < resultAfterFunctionCall);
        }

        [TestCleanup]
        public static void CleanUp()
        {
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("local.settings.json", true, true).AddEnvironmentVariables().Build();
            Environment.SetEnvironmentVariable("connection_string", config["Values:connection_string"]);
            string tableName = "views";
            connString = Environment.GetEnvironmentVariable("connection_string",EnvironmentVariableTarget.Process);
            int views = 0;

            TableClient tableClient = new TableClient(connString, tableName);

            Pageable<TableEntity> results = tableClient.Query<TableEntity>(entity => entity.PartitionKey == "views");

            foreach (TableEntity tableEntity in results)
            {
                views = tableEntity.views;
            }

            //remove the view count added by the test
            TableEntity entity = new TableEntity()
            {
                PartitionKey = "views",
                RowKey = "0",
                views = views-1
            };

            tableClient.UpsertEntity(entity);
        }
    }
}
