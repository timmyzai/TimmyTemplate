public static class UserContext
{
    private static readonly AsyncLocal<Guid?> _currentUserId = new();

    public static Guid? CurrentUserId
    {
        get => _currentUserId.Value;
        set => _currentUserId.Value = value;
    }
}
