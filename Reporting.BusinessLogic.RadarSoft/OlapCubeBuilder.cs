namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using System.Xml.Linq;

    using RadarSoft.RadarCube.WPF;
    using RadarSoft.RadarCube.WPF.Common;
    using RadarSoft.RadarCube.WPF.Desktop;
    
    /// <summary>
    /// Contains a method that allows to build a <see cref="TOLAPCube"/>
    /// </summary>
    public class OlapCubeBuilder
    {
        /// <summary>
        /// Returns a new <see cref="TOLAPCube"/> built based on the specified parameters
        /// </summary>
        /// <param name="config">The cube constructor config document</param>
        /// <param name="dataSetDescriptor">The data set descriptor</param>
        /// <param name="dataSet">The data set</param>
        /// <returns>A new <see cref="TOLAPCube"/> built based on the specified parameters</returns>
        public static TOLAPCube BuildCube(XElement config, DataSetDescriptor dataSetDescriptor, DataSet dataSet)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (dataSetDescriptor == null) throw new ArgumentNullException(nameof(dataSetDescriptor));
            if (dataSet == null) throw new ArgumentNullException(nameof(dataSet));

            return new OlapCubeBuilder(config, dataSetDescriptor, dataSet).Build();
        }

        /// <summary>
        /// The cube constructor config document
        /// </summary>
        private readonly XElement _config;

        /// <summary>
        /// The data set descriptor
        /// </summary>
        private readonly DataSetDescriptor _dataSetDescriptor;

        /// <summary>
        /// The data set
        /// </summary>
        private readonly DataSet _dataSet;

        /// <summary>
        /// The OLAP cube being built
        /// </summary>
        private TOLAPCube _cube;

        /// <summary>
        /// Initializes a new instance of the <see cref="OlapCubeBuilder"/> class
        /// </summary>
        /// <param name="config">The cube constructor config document</param>
        /// <param name="dataSetDescriptor">The data set descriptor</param>
        /// <param name="dataSet">The data set</param>
        private OlapCubeBuilder(XElement config, DataSetDescriptor dataSetDescriptor, DataSet dataSet)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (dataSetDescriptor == null) throw new ArgumentNullException(nameof(dataSetDescriptor));
            if (dataSet == null) throw new ArgumentNullException(nameof(dataSet));

            _config = config;
            _dataSetDescriptor = dataSetDescriptor;
            _dataSet = dataSet;
        }

        /// <summary>
        /// Returns a new <see cref="TOLAPCube"/>
        /// </summary>
        /// <returns>A new <see cref="TOLAPCube"/></returns>
        public TOLAPCube Build()
        {
            _cube = new TOLAPCube {DataSet = _dataSet};

            var ns = _config.Name.Namespace;

            // add measures
            foreach (var m in _config.Descendants(ns + "Measure"))
            {
                var measure = _cube.AddMeasure(
                    _dataSet.Tables[(string) m.Attribute("sourceTable")],
                    (string) m.Attribute("sourceField"),
                    (string) m.Attribute("displayName"));

                var aggregateFunction = (string) m.Attribute("aggregateFunction");
                if (aggregateFunction != null)
                {
                    measure.AggregateFunction = (TFunction) Enum.Parse(typeof (TFunction), aggregateFunction, true);
                }

                var format = (string) m.Attribute("format");
                if (format != null)
                {
                    measure.DefaultFormat = format;
                }
            }

            // add dimentions
            foreach (var d in _config.Descendants(ns + "Dimention"))
            {
                var dimentionName = (string) d.Attribute("displayName");

                var dimention = _cube.AddDimension(dimentionName);

                // add flat hierarchies
                foreach (var h in d.Elements(ns + "Hierarchy"))
                {
                    CreateHierarchy(dimentionName, dimention, h);
                }

                // add multi-level hierarchies
                foreach (var m in d.Elements(ns + "Multilevel"))
                {
                    // add sub-flat hierarchies
                    foreach (var h in m.Elements(ns + "Hierarchy"))
                    {
                        var hierarchy = CreateHierarchy(dimentionName, dimention, h);

                        foreach (var a in h.Elements(ns + "Attribute"))
                        {
                            var displayName = (string) a.Attribute("displayName");
                            var sourceField = (string) a.Attribute("sourceField");

                            var att = new TInfoAttribute
                            {
                                DisplayMode = AttributeDispalyMode.AsColumn,
                                DisplayName = displayName,
                                SourceField = sourceField,
                                SourceFieldType =
                                    _dataSet.Tables[(string) h.Attribute("sourceTable")].Columns[sourceField].DataType
                            };
                            hierarchy.InfoAttributes.Add(att);
                        }

                        _cube.MakeUpCompositeHierarchy(dimentionName, (string) m.Attribute("name"), hierarchy);
                    }
                }
            }

            return _cube;
        }

        private TCubeHierarchy CreateHierarchy(
            string dimentionName,
            TCubeDimension dimention,
            XElement h)
        {
            TCubeHierarchy hierarchy;

            var sourceTable = (string) h.Attribute("sourceTable");
            var sourceField = (string) h.Attribute("sourceField");
            var displayName = (string) h.Attribute("displayName");

            if (_dataSet.Tables[sourceTable].Columns[sourceField].DataType == typeof (DateTime))
            {
                #region Add a BI hierarchy

                var hierarchyYear = _cube.AddBIHierarchy(
                    dimention.DisplayName,
                    _dataSet.Tables[sourceTable],
                    displayName + ": Year",
                    sourceField,
                    TBIMembersType.ltTimeYear);
                var hierarchyQuarter = _cube.AddBIHierarchy(
                    dimention.DisplayName,
                    _dataSet.Tables[sourceTable],
                    displayName + ": Quarter",
                    sourceField,
                    TBIMembersType.ltTimeQuarter);
                var hierarchyMonth = _cube.AddBIHierarchy(
                    dimention.DisplayName,
                    _dataSet.Tables[sourceTable],
                    displayName + ": Month",
                    sourceField,
                    TBIMembersType.ltTimeMonthLong);
                var hierarchyDay = _cube.AddBIHierarchy(
                    dimention.DisplayName,
                    _dataSet.Tables[sourceTable],
                    displayName + ": Day",
                    sourceField,
                    TBIMembersType.ltTimeDayOfMonth);

                hierarchy = _cube.MakeUpCompositeHierarchy(
                    dimentionName,
                    displayName + ": Year-Quarter-Month-Day",
                    new List<TCubeHierarchy> {hierarchyYear, hierarchyQuarter, hierarchyMonth, hierarchyDay});

                #endregion
            }
            else
            {
                #region Add either a Parent-Child or a simple hierarchy

                var isSelfReference = (string) h.Attribute("selfReference") == "true";

                if (isSelfReference)
                {
                    var parentField = _dataSetDescriptor.Tables[sourceTable].ForeignKeys.Single(
                        fd => fd.ForeignKeyReference.ParentTable.Name == sourceTable).AliasOrName;

                    hierarchy = _cube.AddHierarchy(
                        dimention.DisplayName,
                        _dataSet.Tables[sourceTable],
                        sourceField,
                        parentField,
                        displayName);

                    hierarchy.IDField = _dataSetDescriptor.Tables[sourceTable].PrimaryKey.AliasOrName;
                    hierarchy.IDFieldType = _dataSet.Tables[sourceTable].Columns[hierarchy.IDField].DataType;
                }
                else
                {
                    hierarchy = _cube.AddHierarchy(
                        dimention.DisplayName,
                        _dataSet.Tables[sourceTable],
                        sourceField,
                        null,
                        displayName);
                }

                #endregion
            }

            return hierarchy;
        }
    }
}