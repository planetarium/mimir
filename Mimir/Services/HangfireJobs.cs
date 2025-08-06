using Hangfire;
using Libplanet.Crypto;
using Mimir.Services;

namespace Mimir.Services;

public static class HangfireJobs
{
    public static void EnqueueAgentStateRecovery(Address agentAddress)
    {
        BackgroundJob.Enqueue<IStateRecoveryService>(service =>
            service.TryRecoverAgentStateAsync(agentAddress)
        );
    }

    public static void EnqueueAvatarStateRecovery(Address avatarAddress)
    {
        BackgroundJob.Enqueue<IStateRecoveryService>(service =>
            service.TryRecoverAvatarStateAsync(avatarAddress)
        );
    }

    public static void EnqueueNCGBalanceRecovery(Address agentAddress)
    {
        BackgroundJob.Enqueue<IStateRecoveryService>(service =>
            service.TryRecoverNCGBalanceAsync(agentAddress)
        );
    }
}
