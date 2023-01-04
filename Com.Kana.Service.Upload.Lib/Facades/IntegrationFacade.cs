﻿using Com.Kana.Service.Upload.Lib.Helpers;
using Com.Kana.Service.Upload.Lib.Interfaces;
using Com.Kana.Service.Upload.Lib.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Com.Kana.Service.Upload.Lib.Facades
{
    public class IntegrationFacade : IIntegrationFacade
    {
        private readonly IServiceProvider serviceProvider;

        public IntegrationFacade(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<AccurateTokenViewModel> RetrieveToken(string code)
        {
            var AccurateToken = await RequestTokenAsync(code);

            if(AccurateToken != null)
            {
                AuthCredential.AccessToken = AccurateToken.access_token;
                AuthCredential.RefreshToken = AccurateToken.refresh_token;
            }

            return AccurateToken;
        }

        public async Task<AccurateTokenViewModel> RefreshToken()
        {
            var refresh_token = AuthCredential.RefreshToken;

            var AccurateToken = await RenewTokenAsync(refresh_token);

            if (AccurateToken != null)
            {
                AuthCredential.AccessToken = AccurateToken.access_token;
                AuthCredential.RefreshToken = AccurateToken.refresh_token;
            }

            return AccurateToken;
        }

        private async Task<AccurateTokenViewModel> RequestTokenAsync(string code)
        {
            IAuthorizationClientService httpClient = (IAuthorizationClientService)serviceProvider.GetService(typeof(IAuthorizationClientService));
            var send = new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", APIEndpoint.Upload + "integration/authcallback"),
            };

            var content = new FormUrlEncodedContent(send);
            var response = await httpClient.SendAsync(HttpMethod.Post, APIEndpoint.Accurate + "oauth/token", content);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                AccurateTokenViewModel AccuToken = JsonConvert.DeserializeObject<AccurateTokenViewModel>(data);
                return AccuToken;
            }
            else
            {
                return null;
            }
        }

        private async Task<AccurateTokenViewModel> RenewTokenAsync(string refToken)
        {
            IAuthorizationClientService httpClient = (IAuthorizationClientService)serviceProvider.GetService(typeof(IAuthorizationClientService));
            var send = new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("redirect_uri", APIEndpoint.Upload + "integration/authcallback"),
                new KeyValuePair<string, string>("refresh_token", refToken),
            };

            var content = new FormUrlEncodedContent(send);
            var response = await httpClient.SendAsync(HttpMethod.Post, APIEndpoint.Accurate + "oauth/token", content);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                AccurateTokenViewModel AccuToken = JsonConvert.DeserializeObject<AccurateTokenViewModel>(data);
                return AccuToken;
            }
            else
            {
                return null;
            }
        }

        public async Task<AccurateSessionViewModel> OpenDb()
        {

            ISessionClientService httpClient = (ISessionClientService)serviceProvider.GetService(typeof(ISessionClientService));

            var url = "https://account.accurate.id/api/open-db.do?id=703743";

            var response = await httpClient.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var message = JsonConvert.DeserializeObject<AccurateResponseViewModel>(data);
            if (response.IsSuccessStatusCode && message.s)
            {
                AccurateSessionViewModel AccuSession = JsonConvert.DeserializeObject<AccurateSessionViewModel>(data);

                AuthCredential.Session = AccuSession.session;
                AuthCredential.Host = AccuSession.host;

                return AccuSession;
            }
            else
            {
                return null;
            }
        }

    }
}
