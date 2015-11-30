namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using System;
    using System.Text;
    using System.Windows.Forms;

    using Reporting.BusinessLogic;

    /// <summary>
    /// Provides the implementation of the <see cref="IExternalFilterProvider"/> interface that is able to build a filter for the TIMESTAMP data format only
    /// </summary>
    internal class PrimitiveExternalFilterProvider : IExternalFilterProvider, IDisposable
    {
        private readonly TimestampFilterForm _filterForm = new TimestampFilterForm();

        public void Reset()
        {
            _filterForm._checkBoxDontAskAgain.Checked = false;
            _filterForm._dateTimePickerStart.Value = DateTime.Now;
            _filterForm._dateTimePickerEnd.Value = DateTime.Now;
        }

        public string GetFilter(string tableName, string fieldName, string fieldAlias)
        {
            var filter = new StringBuilder();

            _filterForm.Text = string.Format("Table: {0} | Field: {1} | Alias: {2}", tableName, fieldName, fieldAlias);

            if (_filterForm._checkBoxDontAskAgain.Checked ||
                _filterForm.ShowDialog() == DialogResult.OK)
            {
                var startDate = _filterForm._dateTimePickerStart.Value;
                var endDate = _filterForm._dateTimePickerEnd.Value;

                filter.AppendFormat(
                    "TIMESTAMP('{0}') < TIMESTAMP({1}.{2}) AND TIMESTAMP({1}.{2}) < TIMESTAMP('{3}')",
                    startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    tableName,
                    fieldName,
                    endDate.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return filter.ToString();
        }

        public void Dispose()
        {
            _filterForm.Dispose();
        }
    }
}