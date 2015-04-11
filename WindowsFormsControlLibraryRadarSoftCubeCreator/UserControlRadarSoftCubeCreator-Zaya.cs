namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using RadarSoft.WinForms.Desktop;
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.Odbc;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public partial class UserControlRadarSoftCubeCreator : UserControl
    {
        public event EventHandler CubeCreated;

        public UserControlRadarSoftCubeCreator()
        {
            InitializeComponent();
        }

        public TOLAPCube Cube { get; private set; }

        protected virtual void OnCubeCreated()
        {
            var handler = CubeCreated;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                textBoxCubeFilePath.Text = openFileDialog.FileName;
            }
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            if (Cube != null)
            {
                Cube.Dispose();
            }

            var cubeFilePath = textBoxCubeFilePath.Text;

            try
            {
                var fileContents = File.ReadAllText(cubeFilePath);
                var doc = XElement.Parse(fileContents);
                var ns = doc.Name.Namespace;

                var versionMajor = int.Parse(doc.Attribute("versionMajor").Value, new CultureInfo("en-US"));
                if (versionMajor != 0)
                {
                    throw new InvalidDataException("The major version is invalid");
                }

                var versionMinor = int.Parse(doc.Attribute("versionMinor").Value, new CultureInfo("en-US"));
                if (versionMinor != 4)
                {
                    throw new InvalidDataException("The minor version is invalid");
                }

                var connectionString = doc.Attribute("connectionString").Value;
                var selectCommand = doc.Attribute("selectCommand").Value;

                var dataSet = new DataSet();

                DbDataAdapter dataAdapter;
                using (var conn = CreateAdapter(connectionString, selectCommand, out dataAdapter))
                using (dataAdapter)
                {
                    var rows = dataAdapter.Fill(dataSet);
                    MessageBox.Show(
                        string.Format("{0} rows found", rows),
                        "Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                Cube = new TOLAPCube();
                Cube.DataSet = dataSet;

                foreach (var m in doc.Descendants(ns + "Measure"))
                {
                    Cube.AddMeasure(
                        dataSet.Tables[0],
                        m.Attribute("sourceField").Value,
                        m.Attribute("displayName").Value);
                }

                foreach (var d in doc.Descendants(ns + "Dimention"))
                {
                    var dimentionName = d.Attribute("displayName").Value;
                    var mlhAttribute = d.Attribute("multilevelHierarchyName");

                    var dimention = Cube.AddDimension(dimentionName);

                    foreach (var h in d.Elements(ns + "Hierarchy"))
                    {
                        var hierarchy = Cube.AddHierarchy(
                            dimention.DisplayName,
                            dataSet.Tables[0],
                            h.Attribute("sourceField").Value,
                            null,
                            h.Attribute("displayName").Value);

                        if (mlhAttribute != null)
                        {
                            Cube.MakeUpCompositeHierarchy(dimentionName, mlhAttribute.Value, hierarchy);
                        }
                    }
                }

                OnCubeCreated();
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DbConnection CreateAdapter(
            string connectionString,
            string selectCmd,
            out DbDataAdapter dataAdapter)
        {
            var connection = new OdbcConnection(connectionString);
            connection.Open();

            dataAdapter = new OdbcDataAdapter(selectCmd, connection);

            return connection;
        }

        private bool GetRelation(
            DbConnection connection,
            string parentTable,
            string childTable,
            out string primaryKeyField,
            out string foreignKeyField)
        {
            var result = false;
            primaryKeyField = null;
            foreignKeyField = null;

            var selectCmdText = string.Format(
                "SELECT FKCOLNAMES, PKCOLNAMES FROM SYSIBM.SYSRELS WHERE TBNAME = '{0}' AND REFTBNAME = '{1}'",
                childTable,
                parentTable);
            using (var selectCmd = new OdbcCommand(selectCmdText, (OdbcConnection)connection))
            {
                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        primaryKeyField = (string)reader["PKCOLNAMES"];
                        foreignKeyField = (string)reader["FKCOLNAMES"];
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    public class FieldDescriptor
    {
        public string Name { get; set; }

        public string Alias { get; set; }

        public string References { get; set; }

        public override string ToString()
        {
            return string.Format("Field: {0} [{1}]", Name, Alias ?? "none");
        }
    }

    public class TableDescriptor
    {
        public string Name { get; set; }
    }
}