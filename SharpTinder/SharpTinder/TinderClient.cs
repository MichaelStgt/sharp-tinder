﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace SharpTinder
{
	public class TinderClient
	{
		const string TINDER_HOST = "https://api.gotinder.com/";
		const string TINDER_IMAGE_HOST = "https://imageupload.gotinder.com/";

		public string AuthToken { get; set; }

		public TinderInformation Information { get; private set; }

		public TinderClient()
		{
		}

		public TinderClient(string authToken) : this()
		{
			AuthToken = authToken;
		}

		NameValueCollection GetHeaders()
		{
			var headers = new NameValueCollection();

			headers.Add("User-Agent", "Tinder Android Version 4.5.5");
			headers.Add("os_version", "23");
			headers.Add("platform", "android");
			headers.Add("app-version", "854");
			headers.Add("Accept-Language", "en");
			headers.Add("Content-Type", "application/json");

			if (AuthToken != string.Empty)
				headers.Add("X-Auth-Token", AuthToken);

			return headers;
		}

		async Task<string> GetRequest(string path)
		{
			var client = new WebClient();

			client.Headers.Clear();
			client.Headers.Add(GetHeaders());

			return await client.DownloadStringTaskAsync(TINDER_HOST + path);
		}

		async Task<string> PostRequest(string path, object data)
		{
			var client = new WebClient();

			client.Headers.Clear();
			client.Headers.Add(GetHeaders());

			return await client.UploadStringTaskAsync(TINDER_HOST + path, JsonConvert.SerializeObject(data));
		}

		/// <summary>
		/// Login the specified fbId and fbToken.
		/// 
		/// http://findmyfbid.com/
		/// https://www.facebook.com/v2.6/dialog/oauth?redirect_uri=fb464891386855067%3A%2F%2Fauthorize%2F&state=%7B%22challenge%22%3A%22q1WMwhvSfbWHvd8xz5PT6lk6eoA%253D%22%2C%220_auth_logger_id%22%3A%2254783C22-558A-4E54-A1EE-BB9E357CC11F%22%2C%22com.facebook.sdk_client_state%22%3Atrue%2C%223_method%22%3A%22sfvc_auth%22%7D&scope=user_birthday%2Cuser_photos%2Cuser_education_history%2Cemail%2Cuser_relationship_details%2Cuser_friends%2Cuser_work_history%2Cuser_likes&response_type=token%2Csigned_request&default_audience=friends&return_scopes=true&auth_type=rerequest&client_id=464891386855067&ret=login&sdk=ios&logger_id=54783C22-558A-4E54-A1EE-BB9E357CC11F#_=_
		/// 
		/// </summary>
		/// <param name="fbId">Fb identifier.</param>
		/// <param name="fbToken">Fb token.</param>
		public async Task<bool> Login(string fbId, string fbToken)
		{
			var data = JsonConvert.DeserializeObject<TinderInformation>(
				await PostRequest("auth", new
				{
					facebook_token = fbToken,
					facebook_id = fbId,
					locale = "en"
				}));

			AuthToken = data.Token;
			Information = data;

			return AuthToken != string.Empty;
		}

		public async Task<TinderUpdate> GetUpdates(DateTime lastActivity)
		{
			string timeStamp = lastActivity.ToUniversalTime().ToString();

			var data = JsonConvert.DeserializeObject<TinderUpdate>(
				await PostRequest("updates", new
				{
					last_activity_date = timeStamp
				}));

			return data;
		}

		public async Task<TinderRecommendation> GetRecommendations()
		{
			var data = await GetRequest("user/recs");
			return JsonConvert.DeserializeObject<TinderRecommendation>(
				data);
		}

		public async Task<TinderMatchResult> Rate(string userId, bool like, bool superLike)
		{
			string requestString = (like ? "like" : "pass")
										   + (superLike ? "/super" : "")
										   + "/"
				+ userId;

			return JsonConvert.DeserializeObject<TinderMatchResult>(await GetRequest(requestString));
		}

		public Task<TinderMatchResult> SuperLike(string userId)
		{
			return Rate(userId, true, true);
		}

		public Task<TinderMatchResult> Like(string userId)
		{
			return Rate(userId, true, false);
		}

		public Task<TinderMatchResult> Pass(string userId)
		{
			return Rate(userId, false, false);
		}

		public async Task<TinderMessageResult> SendMessage(string matchId, string message)
		{
			var data = JsonConvert.DeserializeObject<TinderMessageResult>(
				await PostRequest("user/matches/" + matchId, new
				{
					message
				}));

			return data;
		}

		public async Task<TinderUserResult> GetUser(string userId)
		{
			var data = JsonConvert.DeserializeObject<TinderUserResult>(
				await GetRequest("user/" + userId));
			return data;
		}

		public async Task<bool> UpdatePosition(double longitude, double latitude)
		{
			var data = JsonConvert.DeserializeObject<TinderUserResult>(
				await PostRequest("user/ping", new
				{
					lon = longitude,
					lat = latitude
				}));

			return data.Status == 200;
		}
	}
}

