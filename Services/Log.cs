namespace SportWeb.Services
{
    public static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "VariableLog"),
            ]
        public static partial void VariableLog(ILogger logger);
    }
}
