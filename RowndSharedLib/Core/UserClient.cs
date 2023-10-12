using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Rownd.Helpers;
using Rownd.Models;

namespace Rownd.Core
{
	public class UserLookupOpts {
		public string[]? UserLookupCriteria { get; set; }

		public string[]? UserIds { get; set; }
	}
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

		private async Task<ResultSet<RowndUserProfile>> ListUserProfiles(UserLookupOpts opts)
		{
			var requestUriBuilder = new UriBuilder($"{_rowndClient.Config.ApiUrl}/applications/{_rowndClient.AppConfig.Id}/users/data");
			NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

			if (opts.UserIds != null && opts.UserIds.Length > 0)
			{
				query.Add("id_filter", string.Join(",", opts.UserIds));
			} else if (opts.UserLookupCriteria != null && opts.UserLookupCriteria.Length > 0)
			{
				query.Add("lookup_filter", string.Join(",", opts.UserLookupCriteria));
			}

			requestUriBuilder.Query = query.ToString();

			var httpClient = RowndHttp.GetHttpClient();
			var request = new HttpRequestMessage()
			{
				RequestUri = requestUriBuilder.Uri,
				Method = HttpMethod.Get,
			};

			var response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<ResultSet<RowndUserProfile>>() ?? throw new RowndException($"Failed to retrieve user profiles.");
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

		private async Task DeleteUserProfile(string userId)
		{
			var httpClient = RowndHttp.GetHttpClient();
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri($"{_rowndClient.Config.ApiUrl}/applications/{_rowndClient.AppConfig.Id}/users/{userId}/data"),
				Method = HttpMethod.Delete,
			};

			var response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
		}

		public async Task<ResultSet<RowndUserProfile>> ListProfiles(UserLookupOpts opts)
		{
			return await ListUserProfiles(opts);
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

		public async Task DeleteProfile(RowndUserProfile userProfile)
		{
			await DeleteProfile(userProfile.Id);
			_memoryCache.Remove(userProfile.Id);
		}

		public async Task DeleteProfile(string userId)
		{
			await DeleteUserProfile(userId);
			_memoryCache.Remove(userId);
		}
	}
}

