﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Signalizer.Entities.Models;

namespace Signalizer.Client.Authentication
{
    public class CustomAuthStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider, ICustomAuthStateProvider
    {
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var sessionModel = await localStorage.GetItemAsync<LoginResponseModel>("sessionState");
            var identity = sessionModel == null ? new ClaimsIdentity() : GetClaimsIdentity(sessionModel.Token);
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(LoginResponseModel model)
        {
            await localStorage.SetItemAsync("sessionState", model);
            var identity = GetClaimsIdentity(model.Token);
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private ClaimsIdentity GetClaimsIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            return new ClaimsIdentity(claims, "jwt");
        }

        public async Task MarkUserAsLoggedOut()
        {
            await localStorage.RemoveItemAsync("sessionState");
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }
}