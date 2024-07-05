namespace Boa.Identity;

/// <summary>
/// Represents the result of a sign-in operation.
/// </summary>
public class Send2FATokenResult
{
    private static readonly Send2FATokenResult _success = new() { Succeeded = true };
    private static readonly Send2FATokenResult _failed = new();
    private static readonly Send2FATokenResult _retry = new() { CanRetry = true };
    private static readonly Send2FATokenResult _notNeeded = new() { IsNotNeeded = true };

    /// <summary>
    /// Returns a flag indication whether the token was send successfully.
    /// </summary>
    /// <value>True if the token was send successfully, otherwise false.</value>
    public bool Succeeded { get; protected set; }

    /// <summary>
    /// Returns a flag indication whether the failure is blocking or another try is possible.
    /// </summary>
    /// <value>True if another try is possible, otherwise false.</value>
    public bool CanRetry { get; protected set; }

    /// <summary>
    /// Returns a flag indication whether the service needs a method to send tokens.
    /// </summary>
    /// <value>True if the service doesn't need a method to send tokens, otherwise false.</value>
    public bool IsNotNeeded { get; protected set; }

    /// <summary>
    /// Returns a flag indication whether the service has to wait before trying to send token again.
    /// </summary>
    /// <value>True if the service has to wait before trying to send token again, otherwise false.</value>
    public bool HasToWait { get; protected set; }

    /// <summary>
    /// Returns the number of seconds to wait before trying to send a token again.
    /// </summary>
    /// <value>The number of seconds to wait before trying to send a token again.</value>
    public int WaitingTime { get; protected set; }

    /// <summary>
    /// Returns a <see cref="Send2FATokenResult"/> that represents a successful submission.
    /// </summary>
    /// <returns>A <see cref="Send2FATokenResult"/> that represents a successful submission.</returns>
    public static Send2FATokenResult Success => _success;

    /// <summary>
    /// Returns a <see cref="Send2FATokenResult"/> that represents a failed submission.
    /// </summary>
    /// <returns>A <see cref="Send2FATokenResult"/> that represents a failed submission.</returns>
    public static Send2FATokenResult Failed => _failed;

    /// <summary>
    /// Returns a <see cref="Send2FATokenResult"/> that represents a failed submission, but it's
    /// possible to make another try.
    /// </summary>
    /// <returns>A <see cref="Send2FATokenResult"/> that represents a failed submission, but it's
    /// possible to make another try.</returns>
    public static Send2FATokenResult Retry => _retry;

    /// <summary>
    /// Returns a <see cref="Send2FATokenResult"/> that represents a submission that failed because the
    /// service doesn't need a method to send tokens.
    /// </summary>
    /// <returns>A <see cref="Send2FATokenResult"/> that represents a submission that failed because the
    /// service doesn't need a method to send tokens.</returns>
    public static Send2FATokenResult NotNeeded => _notNeeded;

    /// <summary>
    /// Creates a <see cref="Send2FATokenResult"/> that represents a submission that failed because the
    /// request was made too close to the previous one.
    /// </summary>
    /// <param name="seconds">seconds to wait before the next request</param>
    /// <returns>A <see cref="Send2FATokenResult"/> that represents a submission that failed because the
    /// request was made too close to the previous one.</returns>
    public static Send2FATokenResult Wait(int seconds) => new() { HasToWait = true, WaitingTime = seconds > 0 ? seconds : 1 };

    /// <summary>
    /// Converts the value of the current <see cref="Send2FATokenResult"/> object to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of value of the current <see cref="Send2FATokenResult"/> object.</returns>
    public override string ToString()
    {
        return IsNotNeeded ? "NotNeeded" :
               CanRetry ? "Retry" :
               HasToWait ? $"Wait {WaitingTime} sec" :
               Succeeded ? "Succeeded" : "Failed";
    }
}
