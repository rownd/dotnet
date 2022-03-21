using Microsoft.Extensions.Caching.Memory;
using Rownd.Models;

namespace Rownd.Core
{
	public class UserProfile
	{
		private readonly IMemoryCache _memoryCache;
		private readonly Config _config;

		private string _appId { get; set; }
		private string _appUserId { get; set; }
		//private string _rowndUserId { get; set; }
		private RowndUser? _rowndUser { get; set; }

		public UserProfile(string appId, string appUserId, Config config)
		{
			_config = config;
			var cacheOptions = new MemoryCacheOptions();
			_memoryCache = new MemoryCache(cacheOptions);

			_appId = appId;
			_appUserId = appUserId;

			_ = fetchUserInfo();
		}

		public UserProfile(string appId, string appUserId, Config config, IMemoryCache memoryCache)
		{
			_config = config;
			_memoryCache = memoryCache;

			_appId = appId;
			_appUserId = appUserId;

            _ = fetchUserInfo();
		}

		private async Task fetchUserInfo()
		{
            var httpClient = new HttpClient();
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri($"{_config.ApiUrl}/applications/{_appId}/users/{_appUserId}/data"),
				Method = HttpMethod.Get,
			};

			request.Headers.Add("x-rownd-app-key", _config.AppKey);
			request.Headers.Add("x-rownd-app-secret", _config.AppSecret);

			var response = await httpClient.SendAsync(request);

			_rowndUser = await response.Content.ReadFromJsonAsync<RowndUser>();



            // _rowndUser = await httpClient.GetFromJsonAsync<RowndUser>($"{_config.ApiUrl}/applications/{_appId}/users/{_appUserId}/data", );
        }

		public async Task<RowndUser> GetProfile()
        {
			if (_rowndUser == null)
            {
				await fetchUserInfo();
            }

			if (_rowndUser == null)
            {
				throw new FieldAccessException($"The user profile for '{_appUserId}' could not be retrieved.");
            }

			return _rowndUser;
        }
	}
}

