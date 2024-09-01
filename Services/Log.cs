namespace SportWeb.Services
{
    public static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "{name}'s value is {value}")]
        public static partial void VariableLog(ILogger logger, string name, object value);
    }

}
