namespace API.Services
{
    public interface ILockoutService
    {
        Task<bool> IsUserLockedOutAsync(string username);
        Task LockoutUserAsync(string username, int lockoutDurationMinutes);
        Task ResetFailedAttemptsAsync(string username);
        Task IncrementFailedAttemptsAsync(string username);
    }

}
