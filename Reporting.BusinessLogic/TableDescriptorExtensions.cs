namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Contains extension methods for the <see cref="TableDescriptor"/> class
    /// </summary>
    public static class TableDescriptorExtensions
    {
        /// <summary>
        /// Returns the PK of the specified <see cref="TableDescriptor"/> or a null if it does not have the PK
        /// </summary>
        /// <param name="tableDescriptor">The <see cref="TableDescriptor"/> whose PK to return</param>
        /// <returns>The PK of the specified <see cref="TableDescriptor"/> or a null if it does not have the PK</returns>
        public static FieldDescriptor GetPrimaryKey(this TableDescriptor tableDescriptor)
        {
            if (tableDescriptor == null) throw new ArgumentNullException(nameof(tableDescriptor));

            return tableDescriptor.Fields.Values.SingleOrDefault(fd => fd.IsPrimaryKey);
        }

        /// <summary>
        /// Returns FKs of the specified <see cref="TableDescriptor"/>
        /// </summary>
        /// <param name="tableDescriptor">The <see cref="TableDescriptor"/> whose FKs to return</param>
        /// <returns>FKs of the specified <see cref="TableDescriptor"/></returns>
        public static IEnumerable<FieldDescriptor> GetForeignKeys(this TableDescriptor tableDescriptor)
        {
            if (tableDescriptor == null) throw new ArgumentNullException(nameof(tableDescriptor));

            return tableDescriptor.Fields.Values.Where(fd => fd.References != null);
        }
    }
}