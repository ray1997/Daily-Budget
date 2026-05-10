using DailyBudget.Api.Data;
using DailyBudget.Api.Dtos;
using DailyBudget.Api.Models;
using DailyBudget.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyBudget.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(BudgetDbContext db, OidcService oidc, TokenService tokens) : ControllerBase
{
    [HttpPost("oidc")]
    public async Task<ActionResult<AuthResponse>> Login(OidcLoginRequest request, CancellationToken cancellationToken)
    {
        var profile = await oidc.RedeemCodeAsync(request, cancellationToken);
        var user = await db.Users.SingleOrDefaultAsync(u => u.OidcSubject == profile.Subject, cancellationToken);
        if (user is null)
        {
            user = new AppUser { OidcSubject = profile.Subject };
            db.Users.Add(user);
        }

        user.Email = profile.Email ?? string.Empty;
        user.DisplayName = profile.Name ?? profile.PreferredUsername ?? profile.Email ?? "Budget user";
        await db.SaveChangesAsync(cancellationToken);

        var token = tokens.CreateToken(user);
        return Ok(new AuthResponse(token.Token, token.ExpiresAt, new UserDto(user.Id, user.Email, user.DisplayName)));
    }
}
