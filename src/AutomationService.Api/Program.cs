using AutomationService.Api.Background;
using AutomationService.Api.Hubs;
using AutomationService.Api.Jobs;
using AutomationService.Api.Services;
using AutomationService.Application.Abstractions;
using AutomationService.Application.Configuration;
using AutomationService.Application.Rules;
using AutomationService.Application.Services;
using AutomationService.Domain.Models;
using AutomationService.Infrastructure.Clients;
using AutomationService.Infrastructure.Configuration;
using AutomationService.Infrastructure.Messaging;
using AutomationService.Infrastructure.Persistence;
using AutomationService.Infrastructure.State;
using AutomationService.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.Configure<AquariumRegistryOptions>(builder.Configuration.GetSection(AquariumRegistryOptions.SectionName));
builder.Services.Configure<GraphQlOptions>(builder.Configuration.GetSection(GraphQlOptions.SectionName));
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection(OutboxOptions.SectionName));
builder.Services.Configure<FeedingOptions>(builder.Configuration.GetSection(FeedingOptions.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    var allowAllOrigins = builder.Configuration.GetValue<bool>("Cors:AllowAllOrigins");
    options.AddPolicy("FrontendCors", policyBuilder =>
    {
        policyBuilder
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (allowAllOrigins)
        {
            policyBuilder
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials();
            return;
        }

        if (allowedOrigins.Length == 0)
        {
            return;
        }

        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowCredentials();
    });
});

builder.Services.AddHttpClient<IAquariumDataClient, GraphQlAquariumDataClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<GraphQlOptions>>();
    client.BaseAddress = new Uri(options.Value.Endpoint);
});
builder.Services.AddHttpClient<IAquariumRegistryClient, AquariumRegistryClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AquariumRegistryOptions>>();
    client.BaseAddress = new Uri(options.Value.BaseUrl);
});

builder.Services.AddDbContext<AutomationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

builder.Services.AddScoped<IEventDetectionService, EventDetectionService>();
builder.Services.AddScoped<IFeedingService, FeedingService>();
builder.Services.AddScoped<IPollingOrchestrator, PollingOrchestrator>();
builder.Services.AddScoped<IClock, SystemClock>();
builder.Services.AddScoped<IStateStore, RedisStateStore>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddScoped<IOutboxMessageRepository, EfOutboxMessageRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IEventPublisher, OutboxEventPublisher>();
builder.Services.AddScoped<IOutboxMessageDispatcher, OutboxMessageDispatcher>();
builder.Services.AddScoped<IOutboxProcessor, OutboxProcessor>();
builder.Services.AddScoped<IRule<AquariumData>, WaterQualityRule>();
builder.Services.AddScoped<IRule<AquariumData>, TemperatureRule>();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("polling-job");
    q.AddJob<PollingJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("polling-job-trigger")
        .WithSimpleSchedule(schedule => schedule
            .WithInterval(TimeSpan.FromSeconds(30))
            .RepeatForever()));
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddHostedService<OutboxProcessorBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AutomationDbContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Database.ExecuteSqlRaw("""
        ALTER TABLE "OutboxMessages"
        ADD COLUMN IF NOT EXISTS "AquariumId" uuid NULL;
        """);
    dbContext.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_OutboxMessages_AquariumId_OccurredAtUtc"
        ON "OutboxMessages" ("AquariumId", "OccurredAtUtc");
        """);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontendCors");
app.UseAuthorization();
app.UseSerilogRequestLogging();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
