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
        private static string connectionString;
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("local.settings.json", true, true).Build();
            connectionString = config["ConnectionStrings:connection_string"];
        }

        [TestMethod]
        public async Task CheckViewCountIncreases()
        {
            int resultBeforeFunctionCall = 0;
            int resultAfterFunctionCall = 0;
            string tableName = "views";
            var httpContext = new DefaultHttpContext();

            //get initial count
            TableClient tableClient = new TableClient(connectionString, tableName);

            Pageable<TableEntity> results = tableClient.Query<TableEntity>(entity => entity.PartitionKey == "views");

            foreach (TableEntity tableEntity in results)
            {
                resultBeforeFunctionCall = tableEntity.views;
            }

            //call function to get new count
            resultAfterFunctionCall = await Run(httpContext.Request);

            Assert.IsTrue(resultBeforeFunctionCall < resultAfterFunctionCall);
        }
    }
}