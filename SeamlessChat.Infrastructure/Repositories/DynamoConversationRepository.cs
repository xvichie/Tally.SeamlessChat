using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using SeamlessChat.Core.Entities;
using SeamlessChat.Core.Interfaces;

namespace SeamlessChat.Infrastructure.Repositories;

public class DynamoConversationRepository : IConversationRepository
{
    private readonly IAmazonDynamoDB _dynamo;
    private readonly string _tableName = "SeamlessChat";

    public DynamoConversationRepository(IAmazonDynamoDB dynamo)
    {
        _dynamo = dynamo;
    }

    // ------------------------------------------------------------
    // GET CONVERSATION BY ID (user1_user2)
    // ------------------------------------------------------------
    public async Task<Conversation?> GetByIdAsync(string conversationId)
    {
        var pk = $"CONVERSATION#{conversationId}";

        var response = await _dynamo.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new()
            {
                ["PK"] = new AttributeValue(pk),
                ["SK"] = new AttributeValue("METADATA")
            }
        });

        if (response.Item == null || response.Item.Count == 0)
            return null;

        return MapToConversation(response.Item);
    }

    // ------------------------------------------------------------
    // CREATE if not exists
    // ------------------------------------------------------------
    public async Task CreateIfNotExistsAsync(Conversation conversation)
    {
        var pk = $"CONVERSATION#{conversation.ConversationId}";

        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(pk),
            ["SK"] = new AttributeValue("METADATA"),

            ["Type"] = new AttributeValue("CONVERSATION"),

            ["User1Id"] = new AttributeValue(conversation.User1.UserId.ToString()),
            ["User2Id"] = new AttributeValue(conversation.User2.UserId.ToString()),
            ["LastMessageText"] = new AttributeValue(conversation.LastMessageText ?? ""),
            ["LastMessageAt"] = new AttributeValue { N = conversation.LastMessageAt.Ticks.ToString() }
        };

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item,
            ConditionExpression = "attribute_not_exists(PK)"
        };

        try { await _dynamo.PutItemAsync(request); }
        catch (ConditionalCheckFailedException)
        {
            // already exists, ignore
        }

        // Create inbox rows
        await UpsertInbox(conversation, conversation.User1.UserId, conversation.User2.UserId);
        await UpsertInbox(conversation, conversation.User2.UserId, conversation.User1.UserId);
    }

    // ------------------------------------------------------------
    // UPDATE last message in conversation + inbox view
    // ------------------------------------------------------------
    public async Task UpdateLastMessageAsync(string conversationId, string? text, DateTime at)
    {
        var pk = $"CONVERSATION#{conversationId}";

        await _dynamo.UpdateItemAsync(new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new()
            {
                ["PK"] = new AttributeValue(pk),
                ["SK"] = new AttributeValue("METADATA")
            },
            UpdateExpression = "SET LastMessageText = :t, LastMessageAt = :a",
            ExpressionAttributeValues = new()
            {
                [":t"] = new AttributeValue(text ?? ""),
                [":a"] = new AttributeValue { N = at.Ticks.ToString() }
            }
        });

        var convo = await GetByIdAsync(conversationId);
        if (convo == null) return;

        await UpsertInbox(convo, convo.User1.UserId, convo.User2.UserId);
        await UpsertInbox(convo, convo.User2.UserId, convo.User1.UserId);
    }

    // ------------------------------------------------------------
    // WRITE inbox GSI entry
    // ------------------------------------------------------------
    private async Task UpsertInbox(Conversation convo, Guid viewerUserId, Guid otherUserId)
    {
        var inboxPk = $"USER#{viewerUserId}";
        var sk = convo.LastMessageAt.Ticks.ToString();

        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(inboxPk),
            ["SK"] = new AttributeValue { N = sk },

            ["GSI1PK"] = new AttributeValue(inboxPk),
            ["GSI1SK"] = new AttributeValue { N = sk },

            ["Type"] = new AttributeValue("INBOX"),

            ["ConversationId"] = new AttributeValue(convo.ConversationId),
            ["LastMessageText"] = new AttributeValue(convo.LastMessageText ?? ""),
            ["LastMessageAt"] = new AttributeValue { N = sk },

            ["User1Id"] = new AttributeValue(convo.User1.UserId.ToString()),
            ["User2Id"] = new AttributeValue(convo.User2.UserId.ToString()),
            ["OtherParticipantId"] = new AttributeValue(otherUserId.ToString())
        };

        await _dynamo.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        });
    }

    // ------------------------------------------------------------
    // QUERY inbox for user
    // ------------------------------------------------------------
    public async Task<List<Conversation>> GetUserInboxAsync(Guid userId, int limit)
    {
        var req = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "UserInboxIndex",
            KeyConditionExpression = "GSI1PK = :pk",
            ExpressionAttributeValues = new()
            {
                [":pk"] = new AttributeValue($"USER#{userId}")
            },
            ScanIndexForward = false,
            Limit = limit
        };

        var result = await _dynamo.QueryAsync(req);

        return result.Items.Select(MapToConversationFromInbox).ToList();
    }

    // ------------------------------------------------------------
    // MAPPERS
    // ------------------------------------------------------------
    private static Conversation MapToConversation(IDictionary<string, AttributeValue> item)
    {
        var user1 = Guid.Parse(item["User1Id"].S);
        var user2 = Guid.Parse(item["User2Id"].S);

        var ticks = long.Parse(item["LastMessageAt"].N);

        return new Conversation(user1, user2, new DateTime(ticks, DateTimeKind.Utc));
    }

    private static Conversation MapToConversationFromInbox(IDictionary<string, AttributeValue> item)
    {
        var user1 = Guid.Parse(item["User1Id"].S);
        var user2 = Guid.Parse(item["User2Id"].S);

        var ticks = long.Parse(item["LastMessageAt"].N);

        return new Conversation(user1, user2, new DateTime(ticks, DateTimeKind.Utc));
    }
}
