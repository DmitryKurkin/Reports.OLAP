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
        /// <param name="filePath">The path to the file</param>
        /// <param name="cubeFiles">Available cube configuration files</param>
        public DataSetConfigurationFile(string filePath, IEnumerable<CubeConfigurationFile> cubeFiles)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("The path cannot be empty", nameof(filePath));
            if (cubeFiles == null) throw new ArgumentNullException(nameof(cubeFiles));

            FilePath = filePath;
            CubeFiles = cubeFiles.ToList();
        }

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