namespace Reporting.BusinessLogic
{
    using System;

    /// <summary>
    /// Represents a cube configuration file
    /// </summary>
    public class CubeConfigurationFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CubeConfigurationFile"/> class
        /// </summary>
        /// <param name="displayName">The display name of the config</param>
        /// <param name="filePath">The path to the file</param>
        public CubeConfigurationFile(string displayName, string filePath)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("The display name be empty", nameof(displayName));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The path cannot be empty", nameof(filePath));

            DisplayName = displayName;
            FilePath = filePath;
        }

        /// <summary>
        /// Gets the display name of the config
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the path to the file
        /// </summary>
        public string FilePath { get; }
    }
}