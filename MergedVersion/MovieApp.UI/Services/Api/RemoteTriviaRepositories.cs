using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.UI.Services.Api;

public class RemoteTriviaRepository : ITriviaRepository
{
    private readonly ApiClient apiClient;
    public RemoteTriviaRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string categoryName, CancellationToken cancellationToken = default) => 
        await this.apiClient.GetAsync<IEnumerable<TriviaQuestion>>($"api/trivia/category/{categoryName}", cancellationToken) ?? new List<TriviaQuestion>();

    public async Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieIdentifier, int questionCount = 5, CancellationToken cancellationToken = default) => 
        await this.apiClient.GetAsync<IEnumerable<TriviaQuestion>>($"api/trivia/movie/{movieIdentifier}?count={questionCount}", cancellationToken) ?? new List<TriviaQuestion>();
}

public class RemoteTriviaRewardRepository : ITriviaRewardRepository
{
    private readonly ApiClient apiClient;
    public RemoteTriviaRewardRepository(ApiClient apiClient) => this.apiClient = apiClient;

    public async Task<TriviaReward?> GetUnredeemedByUserAsync(int userIdentifier, CancellationToken cancellationToken = default) => 
        await this.apiClient.GetAsync<TriviaReward>($"api/trivia/reward/{userIdentifier}", cancellationToken);

    public Task AddAsync(TriviaReward reward, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task MarkAsRedeemedAsync(int rewardId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
