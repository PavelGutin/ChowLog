using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/Account/Logout", async Task<IResult> (
            ClaimsPrincipal user,
            SignInManager<IdentityUser> signInManager,
            [FromForm] string returnUrl) =>
        {
            await signInManager.SignOutAsync();
            return Results.LocalRedirect($"~/{returnUrl}");
        });

        endpoints.MapPost("/Account/PasswordSignIn", async Task<IResult> (
            HttpContext context,
            [FromForm] string email,
            [FromForm] string password,
            [FromForm] bool rememberMe,
            [FromForm] string returnUrl,
            SignInManager<IdentityUser> signInManager) =>
        {
            var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return Results.LocalRedirect($"~/{returnUrl}");
            }

            var routeValues = new { returnUrl, email };
            context.Response.Headers.Add("error", "Invalid login attempt.");
            return Results.RedirectToRoute("Account/Login", routeValues);
        });

        endpoints.MapPost("/Account/Manage/LinkExternalLogin", async Task<IResult> (
            HttpContext context,
            [FromForm] string provider,
            [FromForm] string returnUrl,
            SignInManager<IdentityUser> signInManager) =>
        {
            await context.SignOutAsync(IdentityConstants.ExternalScheme);

            var redirectUrl = UriHelper.BuildRelative(
                context.Request.PathBase,
                "/Account/Manage/ExternalLogins",
                QueryString.Create("handler", "LinkLoginCallback"));

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, signInManager.UserManager.GetUserId(context.User));
            return Results.Challenge(properties, new[] { provider });
        }).RequireAuthorization();

        return endpoints;
    }
}
