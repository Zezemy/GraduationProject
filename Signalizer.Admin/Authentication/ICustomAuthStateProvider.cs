using Microsoft.AspNetCore.Components.Authorization;
using Signalizer.Entities.Models;

namespace Signalizer.Admin.Authentication
{
    public interface ICustomAuthStateProvider
    {
        Task<AuthenticationState> GetAuthenticationStateAsync();
        Task MarkUserAsAuthenticated(LoginResponseModel model);
        Task MarkUserAsLoggedOut();
    }
}