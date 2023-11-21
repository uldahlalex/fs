namespace api;

public static class ActionExtensions
{
    public static void TryCatch(this Action action, Action<Exception> exceptionHandler)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            exceptionHandler(ex);
        }
    }

    // Overload for Func<T>
    public static T TryCatch<T>(this Func<T> func, Action<Exception> exceptionHandler)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            exceptionHandler(ex);
            return default(T);
        }
    }
}
public static class ExceptionHandlers
{
    public static void TryExecute(Action action, Action<Exception> exceptionHandler)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            exceptionHandler(ex);
        }
    }

    public static void Wrap(Delegate action)
    {
        Console.WriteLine("Wrap");
    }

    // Overload for Func<T> if you need to return a value
    public static T TryExecute<T>(Func<T> func, Action<Exception> exceptionHandler)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            exceptionHandler(ex);
            return default(T);
        }
    }
}