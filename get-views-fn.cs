using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Azure.Data.Tables;
using Azure;
using System;
using Microsoft.Extensions.Configuration;

namespace Resume.Functions
{
    public static class get_views_fn
    {
        [FunctionName("get_views_fn")]
        public static async Task<int> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("local.settings.json",true, true).AddEnvironmentVariables().Build();
            string connectionString = Environment.GetEnvironmentVariable("connection_string",EnvironmentVariableTarget.Process);
            string tableName = "views";
            int views = 10;

            try{
                TableClient tableClient = new TableClient(connectionString, tableName);

                Pageable<TableEntity> results = tableClient.Query<TableEntity>(entity => entity.PartitionKey == "views");

                foreach (TableEntity tableEntity in results)
                {
                    views = tableEntity.views;
                }

                
                TableEntity entity = new TableEntity()
                {
                    PartitionKey = "views",
                    RowKey = "0",
                    views = views+1
                };

                tableClient.UpsertEntity(entity);

                
                Pageable<TableEntity> result = tableClient.Query<TableEntity>(entity => entity.PartitionKey == "views");

                foreach (TableEntity tableEntity in result)
                {
                    views = tableEntity.views;
                } 

                return views;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            

            return views;
        }
    }
}