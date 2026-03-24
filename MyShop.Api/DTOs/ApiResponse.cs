namespace MyShop.Api.DTOs;

// ===== SHARED API RESPONSE =====

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data
);

public record ApiErrorResponse(
    bool Success,
    string Message,
    string? Detail
);
