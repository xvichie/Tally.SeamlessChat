namespace SeamlessChat.Infrastructure.Extensions;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SeamlessChat.Core.Settings;

public class ChatDatabaseInitializer
{
    private readonly string _tableName;
    private readonly IAmazonDynamoDB _amazonDynamoDB;

    public ChatDatabaseInitializer(IOptions<DynamoSettings> options, IAmazonDynamoDB amazonDynamoDB)
    {
        _tableName = options.Value.TableName;
        _amazonDynamoDB = amazonDynamoDB;
    }

    public async Task SetupChatDatabaseAsync(string? tableName = null)
    {
        tableName ??= _tableName;

        if (await TableExists(_amazonDynamoDB, tableName))
            return;

        var request = new CreateTableRequest
        {
            TableName = tableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,

            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("PK", ScalarAttributeType.S),
                new AttributeDefinition("SK", ScalarAttributeType.S),

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



        await _amazonDynamoDB.CreateTableAsync(request);

        // 3) Wait until table becomes active
        await WaitForTableActivation(_amazonDynamoDB, tableName);
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
