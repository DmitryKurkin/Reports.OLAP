namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a data set configuration file with associated cube configuration files
    /// </summary>
    public class DataSetConfigurationFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSetConfigurationFile"/> class
        /// </summary>
        /// <param name="displayName">The display name of the config</param>
        /// <param name="filePath">The path to the file</param>
        /// <param name="cubeFiles">Available cube configuration files</param>
        public DataSetConfigurationFile(
            string displayName,
            string filePath,
            IEnumerable<CubeConfigurationFile> cubeFiles)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("The display name be empty", nameof(displayName));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The path cannot be empty", nameof(filePath));
            if (cubeFiles == null) throw new ArgumentNullException(nameof(cubeFiles));

            DisplayName = displayName;
            FilePath = filePath;
            CubeFiles = cubeFiles.ToList();
        }

        /// <summary>
        /// Gets the display name of the config
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the path to the file
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets available cube configuration files
        /// </summary>
        public IList<CubeConfigurationFile> CubeFiles { get; }
    }
}