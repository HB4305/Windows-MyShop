namespace MyShop.Services;

/// <summary>
/// Singleton lưu trữ thông tin user đang đăng nhập trong phiên làm việc.
/// </summary>
public class CurrentUserService
{
    public int? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string? UserRole { get; private set; }

    /// <summary>True nếu đã đăng nhập.</summary>
    public bool IsLoggedIn => UserId.HasValue;

    /// <summary>True nếu role là "owner".</summary>
    public bool IsOwner => UserRole == "owner";

    /// <summary>True nếu role là "sale".</summary>
    public bool IsSale => UserRole == "sale";

    /// <summary>Lưu thông tin user sau khi đăng nhập.</summary>
    public void SetUser(int id, string email, string role)
    {
        UserId = id;
        UserEmail = email;
        UserRole = role?.ToLowerInvariant();
    }

    /// <summary>Xóa toàn bộ thông tin user khi logout.</summary>
    public void Clear()
    {
        UserId = null;
        UserEmail = null;
        UserRole = null;
    }
}
