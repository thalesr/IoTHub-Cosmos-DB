using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventHubs;
using System;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IotHubHandler.Function.Model;

namespace IotHubHandler.Function
{
    public class IotHubHandlerFunction
    {

        // The Azure Cosmos DB endpoint
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDbEndpoint");

        // The primary key for the Azure Cosmos account
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDbPrimaryKey");

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container containerTelemetry;

        // The name of the database and container we will create
        private string databaseId = "IotDevicesData";
        private string containerTelemetryId = "TelemetryData";

        [FunctionName("IotHubHandlerFunction")]
        public async Task Run([IoTHubTrigger("iothub-ehub-iothub-dev-17074838-a37ae46429",
                         Connection = "IotHubConnectionString")]EventData message,
                         ILogger log)
        {
            //Device connected to IotHub
            var deviceid = message.SystemProperties["iothub-connection-device-id"].ToString();

            //Cosmos DB connection
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "IotDeviceHandler" });
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            this.containerTelemetry = await this.database.CreateContainerIfNotExistsAsync(containerTelemetryId, "/DeviceId");

            //Converts the data received to an object
            var telemetry = JsonConvert.DeserializeObject<TelemetryData>(Encoding.UTF8.GetString(message.Body.ToArray()));
            telemetry.ID = Guid.NewGuid().ToString();
            telemetry.DeviceId = deviceid;

            //Post data to database
            ItemResponse<TelemetryData> jsonResponse = await this.containerTelemetry
                                                       .CreateItemAsync<TelemetryData>(telemetry, new PartitionKey(telemetry.DeviceId));
            //LOGS
            log.LogWarning($"JSON sent to Cosmos DB: {JsonConvert.SerializeObject(telemetry)}");
            log.LogWarning($"Processed a message: {Encoding.UTF8.GetString(message.Body.ToArray())}");
            log.LogWarning($"DEVICE: {deviceid}");
            log.LogWarning("Created item in database with id: {0} Operation consumed {1} RUs.\n", jsonResponse.Resource.ID, jsonResponse.RequestCharge);


        }
    }
}