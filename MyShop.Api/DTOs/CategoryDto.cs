namespace MyShop.Api.DTOs;

// ===== CATEGORY DTOs =====

public record CategoryResponse(
    int Id,
    string Name,
    string? Description
);

public record CreateCategoryRequest(
    string Name,
    string? Description
);

public record UpdateCategoryRequest(
    string Name,
    string? Description
);
