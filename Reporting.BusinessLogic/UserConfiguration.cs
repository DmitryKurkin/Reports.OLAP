namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents an XML-based user configuration
    /// </summary>
    public class UserConfiguration
    {
        /// <summary>
        /// Returns a new instance of the <see cref="UserConfiguration"/> class
        /// </summary>
        /// <returns>A new instance of the <see cref="UserConfiguration"/> class</returns>
        public static UserConfiguration Create()
        {
            return new UserConfiguration(new DataSetConfigurationFile[0]);
        }

        /// <summary>
        /// Returns a <see cref="UserConfiguration"/> loaded from the specified file
        /// </summary>
        /// <param name="path">The file from which to load a <see cref="UserConfiguration"/></param>
        /// <returns>A <see cref="UserConfiguration"/> loaded from the specified file</returns>
        public static UserConfiguration Load(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var contents = File.ReadAllText(path);

            var doc = XDocument.Parse(contents);

            var dataSetFiles = from dsc in doc.Root.Elements("DataSetConfig")
                select new DataSetConfigurationFile(
                    (string) dsc.Attribute("file"),
                    from cc in dsc.Elements("CubeConfig")
                    select new CubeConfigurationFile((string) cc.Attribute("file")));

            return new UserConfiguration(dataSetFiles);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfiguration"/> class
        /// </summary>
        /// <param name="dataSetFiles">Available data set configuration files</param>
        private UserConfiguration(IEnumerable<DataSetConfigurationFile> dataSetFiles)
        {
            if (dataSetFiles == null) throw new ArgumentNullException(nameof(dataSetFiles));

            DataSetFiles = dataSetFiles.ToList();
        }

        /// <summary>
        /// Gets available data set configuration files
        /// </summary>
        public IList<DataSetConfigurationFile> DataSetFiles { get; }

        /// <summary>
        /// Saves the configuration to the specified file
        /// </summary>
        /// <param name="path">The file to save the configuration to</param>
        public void Save(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var dataSetConfigTags = from dsf in DataSetFiles
                select new XElement(
                    "DataSetConfig",
                    new XAttribute("file", dsf.FilePath),
                    from cf in dsf.CubeFiles
                    select new XElement(
                        "CubeConfig",
                        new XAttribute("file", cf.FilePath)));

            new XDocument(new XElement("UserConfig", dataSetConfigTags.ToArray())).Save(path);
        }
    }
}