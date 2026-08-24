using Tazora.Models;

namespace Tazora.Services;

public class AppSession
{
    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated =>
        CurrentUser is not null;

    public void Start(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }
}