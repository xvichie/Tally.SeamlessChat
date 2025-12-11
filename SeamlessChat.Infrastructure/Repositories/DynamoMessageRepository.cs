using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using SeamlessChat.Core.Entities;
using SeamlessChat.Core.Enums;
using SeamlessChat.Core.Interfaces;
using SeamlessChat.Core.ValueObjects;

namespace SeamlessChat.Infrastructure.Repositories;

public class DynamoMessageRepository : IMessageRepository
{
    private readonly IAmazonDynamoDB _dynamo;
    private readonly string _tableName;

    public DynamoMessageRepository(IAmazonDynamoDB dynamo)
    {
        _dynamo = dynamo;
        _tableName = "SeamlessChat";
    }

    // ------------------------------------------------------------
    // 1) ADD MESSAGE
    // ------------------------------------------------------------
    public async Task AddMessageAsync(Message message)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue($"CONVERSATION#{message.ConversationId}"),

            // Numeric sort key
            ["SK"] = new AttributeValue { N = message.SentAt.Ticks.ToString() },

            ["Type"] = new AttributeValue("MESSAGE"),

            ["MessageId"] = new AttributeValue(message.MessageId.ToString()),
            ["SenderId"] = new AttributeValue(message.SenderId.ToString()),
            ["ReceiverId"] = new AttributeValue(message.ReceiverId.ToString()),
            ["SentAt"] = new AttributeValue { N = message.SentAt.Ticks.ToString() },
            ["Status"] = new AttributeValue(message.Status.ToString())
        };

        if (message.Content is not null)
            item["Content"] = new AttributeValue(message.Content);

        if (message.Attachment is not null)
        {
            item["AttachmentUrl"] = new AttributeValue(message.Attachment.Url);
            item["AttachmentType"] = new AttributeValue(message.Attachment.MediaType.ToString());
        }

        await _dynamo.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        });
    }

    // ------------------------------------------------------------
    // 2) LOAD MESSAGES WITH CURSOR PAGINATION
    // ------------------------------------------------------------
    public async Task<List<Message>> GetMessagesAsync(
        string conversationId,
        int limit,
        DateTime? before)
    {
        var pk = $"CONVERSATION#{conversationId}";
        var cursorTicks = before?.Ticks ?? long.MaxValue;

        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "PK = :pk AND SK < :cursor",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue(pk),
                [":cursor"] = new AttributeValue { N = cursorTicks.ToString() }
            },
            Limit = limit,
            ScanIndexForward = false // newest first
        };

        var result = await _dynamo.QueryAsync(request);
        return result.Items.Select(MapToMessage).ToList();
    }

    // ------------------------------------------------------------
    // 3) MARK DELIVERED
    // ------------------------------------------------------------
    public async Task MarkDeliveredAsync(
        string conversationId,
        Guid messageId,
        DateTime deliveredAt)
    {
        var (pk, sk) = await FindMessageKeys(conversationId, messageId);
        if (pk == null || sk == null) return;

        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue(pk),
                ["SK"] = new AttributeValue { N = sk }
            },
            UpdateExpression = "SET DeliveredAt = :d, #s = :status",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":d"] = new AttributeValue { N = deliveredAt.Ticks.ToString() },
                [":status"] = new AttributeValue(MessageStatus.Delivered.ToString())
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#s"] = "Status"
            }
        };

        await _dynamo.UpdateItemAsync(request);
    }

    // ------------------------------------------------------------
    // 4) MARK SEEN
    // ------------------------------------------------------------
    public async Task MarkSeenAsync(
        string conversationId,
        Guid messageId,
        DateTime seenAt)
    {
        var (pk, sk) = await FindMessageKeys(conversationId, messageId);
        if (pk == null || sk == null) return;

        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue(pk),
                ["SK"] = new AttributeValue { N = sk }
            },
            UpdateExpression = "SET SeenAt = :s, #st = :status",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":s"] = new AttributeValue { N = seenAt.Ticks.ToString() },
                [":status"] = new AttributeValue(MessageStatus.Seen.ToString())
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#st"] = "Status"
            }
        };

        await _dynamo.UpdateItemAsync(request);
    }

    // ------------------------------------------------------------
    // Helper: Find message by ID (inefficient but works)
    // ------------------------------------------------------------
    private async Task<(string? pk, string? sk)> FindMessageKeys(
        string conversationId,
        Guid messageId)
    {
        var pk = $"CONVERSATION#{conversationId}";

        var query = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue(pk)
            }
        };

        var result = await _dynamo.QueryAsync(query);

        var item = result.Items.FirstOrDefault(
            x => x["MessageId"].S == messageId.ToString());

        if (item == null)
            return (null, null);

        return (pk, item["SK"].N);
    }

    // ------------------------------------------------------------
    // Mapping
    // ------------------------------------------------------------
    private static Message MapToMessage(IDictionary<string, AttributeValue> item)
    {
        var conversationId = item["PK"].S.Replace("CONVERSATION#", "");
        var ticks = long.Parse(item["SentAt"].N);

        MediaAttachment? attachment = null;
        if (item.ContainsKey("AttachmentUrl"))
        {
            attachment = new MediaAttachment(
                item["AttachmentUrl"].S,
                Enum.Parse<MediaType>(item["AttachmentType"].S)
            );
        }

        var message = new Message(
            conversationId,
            Guid.Parse(item["SenderId"].S),
            Guid.Parse(item["ReceiverId"].S),
            item.ContainsKey("Content") ? item["Content"].S : null,
            attachment,
            new DateTime(ticks, DateTimeKind.Utc),
            Guid.Parse(item["MessageId"].S)
        );

        if (item.ContainsKey("DeliveredAt"))
            message.MarkDelivered(new DateTime(long.Parse(item["DeliveredAt"].N)));

        if (item.ContainsKey("SeenAt"))
            message.MarkSeen(new DateTime(long.Parse(item["SeenAt"].N)));

        return message;
    }
}