using Microsoft.EntityFrameworkCore;

namespace MasayoshiDj.Authentication;

/// <summary>
/// DB context used (and model managed) by OpenIddict's EF integration.
/// </summary>
public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options);
