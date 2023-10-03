using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Rownd.Helpers;
using Rownd.Models;

namespace Rownd.Core
{
	public class UserClient
	{
		private readonly IMemoryCache _memoryCache;

		private readonly RowndClient _rowndClient;

		public UserClient(RowndClient rowndClient) : this(rowndClient, new MemoryCache(new MemoryCacheOptions())) {}

		public UserClient(RowndClient rowndClient, IMemoryCache memoryCache)
		{
			_rowndClient = rowndClient;
			_memoryCache = memoryCache;
		}

		private async Task<RowndUserProfile> FetchUserProfile(string appUserId)
		{
            var httpClient = RowndHttp.GetHttpClient();
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri($"{_rowndClient.Config.ApiUrl}/applications/{_rowndClient.AppConfig.Id}/users/{appUserId}/data"),
				Method = HttpMethod.Get,
			};

			var response = await httpClient.SendAsync(request);

			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new RowndException($"The user profile for '{appUserId}' could not be retrieved (Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}).");
			}

			return await response.Content.ReadFromJsonAsync<RowndUserProfile>()  ?? throw new RowndException($"The user profile for '{appUserId}' could not be retrieved.");
        }

		private async Task<RowndUserProfile> SaveUserProfile(RowndUserProfile userProfile)
		{
			var httpClient = RowndHttp.GetHttpClient();
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri($"{_rowndClient.Config.ApiUrl}/applications/{_rowndClient.AppConfig.Id}/users/{userProfile.Id}/data"),
				Method = HttpMethod.Put,
				Content = JsonContent.Create(userProfile)
			};

			var response = await httpClient.SendAsync(request);

			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new RowndException($"The user profile for '{userProfile.Id}' could not be saved (Status: {response.StatusCode}; Message: {await response.Content.ReadAsStringAsync()}).");
			}

			return await response.Content.ReadFromJsonAsync<RowndUserProfile>() ?? throw new RowndException($"The user profile for '{userProfile.Id}' could not be retrieved.");
		}

		public async Task<RowndUserProfile> GetProfile(string userId, bool forceRefresh = false)
        {
            if (forceRefresh || !_memoryCache.TryGetValue<RowndUserProfile>(userId, out RowndUserProfile userProfile))
            {
                userProfile = await FetchUserProfile(userId);
				_memoryCache.Set(userProfile.Id, userProfile, new MemoryCacheEntryOptions {
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
				});
            }

            return userProfile;
		}

		public async Task<RowndUserProfile> UpdateProfile(RowndUserProfile userProfile)
		{
			var updatedProfile = await SaveUserProfile(userProfile);
			_memoryCache.Set(userProfile.Id, userProfile, new MemoryCacheEntryOptions {
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
			});

			return updatedProfile;
		}
	}
}

