namespace PointAtlas.Core.Interfaces;

public interface ICurrentUserService
{
    string? GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
    bool IsInRole(string role);
}
