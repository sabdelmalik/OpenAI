using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;


namespace Testing_Serilog
{
    public static class LoggerExtensions
    {
        public static void LogException1(
        this ILogger logger,
        Exception ex,
        string message = "",
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        {
            logger.LogError(ex,
                "{Message} | {Member} | {File}:{Line}",
                message, member, GetTailElements(file, 3), line);
        }

        public static void LogInformation1(
            this ILogger logger,
            string message = "",
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            logger.LogInformation(
                "{Message} | {Member} | {File}:{Line}",
                message, member, GetTailElements(file, 3), line);
        }

        public static string GetTailElements(string path, int n)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;

            // Split using path separator, removing empty entries for robust parsing
            var parts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            // Take the last n parts, or all parts if n is larger than path depth
            var tailParts = parts.TakeLast(Math.Min(n, parts.Length));

            return string.Join(Path.DirectorySeparatorChar.ToString(), tailParts);
        }

    }
}
