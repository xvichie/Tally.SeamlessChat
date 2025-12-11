namespace SeamlessChat.Infrastructure.Extensions;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using SeamlessChat.Core.Interfaces;
using SeamlessChat.Core.Services;
using SeamlessChat.Core.Settings;
using SeamlessChat.Infrastructure.Repositories;
using SeamlessChat.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeamlessChat(
        this IServiceCollection services,
        AwsProfileSettings awsProfileSettings,
        S3BucketSettings s3BucketSettings,
        DynamoSettings dynamoSettings)
    {
        var regionEndpoint = RegionEndpoint.GetBySystemName(awsProfileSettings.Region);

        services.AddSingleton<IAmazonDynamoDB>(_ =>
            new AmazonDynamoDBClient(
                awsProfileSettings.AccessKey,
                awsProfileSettings.SecretKey,
                regionEndpoint));

        services.AddSingleton<IAmazonS3>(_ =>
            new AmazonS3Client(
                awsProfileSettings.AccessKey,
                awsProfileSettings.SecretKey,
                regionEndpoint));

        services.AddSingleton(s3BucketSettings);
        services.AddSingleton(dynamoSettings);

        services.AddScoped<IMessageRepository, DynamoMessageRepository>();
        services.AddScoped<IConversationRepository, DynamoConversationRepository>();

        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMediaStorage, S3MediaStorage>();

        services.AddSingleton<IChatDatabaseInitializer, ChatDatabaseInitializer>();

        return services;
    }
}


