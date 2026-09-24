using System;

namespace ParamToolbox
{
    public static class PathHelper
    {
        /// <summary>
        /// Cleans up drag-and-drop terminal paths (trims whitespace, quotes and trailing slashes if needed)
        /// </summary>
        public static string CleanPath(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return input.Trim().Trim('\"', '\'').Trim();
        }
    }
}
