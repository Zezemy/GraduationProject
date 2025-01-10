using Microsoft.AspNetCore.Components.Authorization;
using Signalizer.Entities.Models;

namespace Signalizer.Client.Authentication
{
    public interface ICustomAuthStateProvider
    {
        Task<AuthenticationState> GetAuthenticationStateAsync();
        Task MarkUserAsAuthenticated(LoginResponseModel model);
        Task MarkUserAsLoggedOut();
    }
}