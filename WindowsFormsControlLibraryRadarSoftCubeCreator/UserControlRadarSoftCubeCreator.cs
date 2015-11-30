namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;

    using Reporting.BusinessLogic;

    using RadarSoft.Common;
    using RadarSoft.WinForms;
    using RadarSoft.WinForms.Desktop;

    public partial class UserControlRadarSoftCubeCreator : UserControl
    {
        public event EventHandler CubeCreated;

        private readonly IDatabaseBridge _dbBridge;

        public UserControlRadarSoftCubeCreator()
        {
            InitializeComponent();

            _dbBridge = new OdbcDatabaseBridge();

            DataSetDescriptorBuilder.FilterProvider = new PrimitiveExternalFilterProvider();
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

                DataSetDescriptorBuilder.FilterProvider.Reset();
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
                CreateCube(cubeFilePath);

                OnCubeCreated();
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (Cube != null)
            {
                Cube.ShowEditor();

                OnCubeCreated();
            }
        }

        private void CreateCube(string cubeFilePath)
        {
            var contents = File.ReadAllText(cubeFilePath);

            var dataSetDesc = DataSetDescriptorBuilder.Build(contents);

            var dataSet = DataSetBuilder.BuilDataSet(dataSetDesc, _dbBridge);

            var doc = XElement.Parse(contents);

            Cube = OlapCubeBuilder.BuildCube(doc, dataSetDesc, dataSet);

            if (checkBoxUseEditor.Checked)
            {
                Cube.ShowEditor();
            }
        }
    }
}