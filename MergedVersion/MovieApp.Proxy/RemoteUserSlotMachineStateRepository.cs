using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Proxy;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MovieApp.Proxy;

public class RemoteUserSlotMachineStateRepository : IUserSlotMachineStateRepository
{
    private readonly ApiClient apiClient;
    public RemoteUserSlotMachineStateRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<UserSpinData?> GetByUserIdAsync(int userIdentifier, CancellationToken cancellationToken = default) =>
        await this.apiClient.GetAsync<UserSpinData>($"api/slotmachine/state/{userIdentifier}", cancellationToken);

    public Task CreateAsync(UserSpinData userSpinData, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task UpdateAsync(UserSpinData userSpinData, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
