using AutomationService.Application.Abstractions;
using Quartz;

namespace AutomationService.Api.Jobs;

public sealed class PollingJob(IPollingOrchestrator pollingOrchestrator) : IJob
{
    public Task Execute(IJobExecutionContext context) =>
        pollingOrchestrator.PollAllAsync(context.CancellationToken);
}
