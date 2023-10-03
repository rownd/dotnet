public class RowndException : Exception
{
    public RowndException(string message) : base(message) {}
    public RowndException(string message, Exception innerException) : base(message, innerException) {}
}