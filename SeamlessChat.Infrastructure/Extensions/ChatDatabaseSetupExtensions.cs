namespace SeamlessChat.Infrastructure.Extensions;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;

public static class ChatDatabaseSetupExtensions
{
    private const string DefaultTableName = "SeamlessChat";

    public static async Task SetupChatDatabaseAsync(this IServiceProvider services, string? tableName = null)
    {
        tableName ??= DefaultTableName;

        using var scope = services.CreateScope();
        var dynamo = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();

        // 1) Check if exists
        if (await TableExists(dynamo, tableName))
            return;

        // 2) Create table
        var request = new CreateTableRequest
        {
            TableName = tableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,

            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("PK", ScalarAttributeType.S),
                new AttributeDefinition("SK", ScalarAttributeType.S),

                // GSI attributes
                new AttributeDefinition("GSI1PK", ScalarAttributeType.S),
                new AttributeDefinition("GSI1SK", ScalarAttributeType.N)
            },

                    KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("PK", KeyType.HASH),
                new KeySchemaElement("SK", KeyType.RANGE)
            },

                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new GlobalSecondaryIndex
                {
                    IndexName = "UserInboxIndex",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("GSI1PK", KeyType.HASH),
                        new KeySchemaElement("GSI1SK", KeyType.RANGE)
                    },
                    Projection = new Projection { ProjectionType = "ALL" }
                }
            }
        };



        await dynamo.CreateTableAsync(request);

        // 3) Wait until table becomes active
        await WaitForTableActivation(dynamo, tableName);
    }


    private static async Task<bool> TableExists(IAmazonDynamoDB dynamo, string tableName)
    {
        try
        {
            await dynamo.DescribeTableAsync(tableName);
            return true;
        }
        catch (ResourceNotFoundException)
        {
            return false;
        }
    }

    private static async Task WaitForTableActivation(IAmazonDynamoDB dynamo, string tableName)
    {
        while (true)
        {
            var response = await dynamo.DescribeTableAsync(tableName);
            if (response.Table.TableStatus == TableStatus.ACTIVE)
                break;

            await Task.Delay(1000);
        }
    }
}
