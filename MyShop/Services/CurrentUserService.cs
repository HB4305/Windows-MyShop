namespace MyShop.Services;

/// <summary>
/// Singleton storing the currently logged-in user information during the session.
/// </summary>
public class CurrentUserService
{
    public int? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string? UserRole { get; private set; }

    /// <summary>True if logged in.</summary>
    public bool IsLoggedIn => UserId.HasValue;

    /// <summary>True if the role is "owner".</summary>
    public bool IsOwner => UserRole == "owner";

    /// <summary>True if the role is "sale".</summary>
    public bool IsSale => UserRole == "sale";

    /// <summary>Saves user information after login.</summary>
    public void SetUser(int id, string email, string role)
    {
        UserId = id;
        UserEmail = email;
        UserRole = role?.ToLowerInvariant();
    }

    /// <summary>Clears all user information on logout.</summary>
    public void Clear()
    {
        UserId = null;
        UserEmail = null;
        UserRole = null;
    }
}
