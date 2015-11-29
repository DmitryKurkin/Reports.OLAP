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
        /// <param name="filePath">The path to the file</param>
        public CubeConfigurationFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The path cannot be empty", nameof(filePath));

            FilePath = filePath;
        }

        /// <summary>
        /// Gets the path to the file
        /// </summary>
        public string FilePath { get; }
    }
}