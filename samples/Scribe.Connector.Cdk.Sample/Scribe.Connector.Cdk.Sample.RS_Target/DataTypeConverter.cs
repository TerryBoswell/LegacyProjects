// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTypeConverter.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Scribe.Connector.Cdk.Sample.RS_Target
{
    using System;

    /// <summary>
    /// Class to handle converting datatypes from .Net and the OleDb connection
    /// </summary>
    internal class DataTypeConverter
    {
        /// <summary>
        /// Enumerated value of the DataType
        /// </summary>
        private enum OledbDataType
        {
            BigInt = 20,
            UnsignedBigInt = 21,
            Binary = 128,
            Bit = 11,
            Bstr = 8,
            Char = 129,
            Chapter = 136,
            Money = 6,
            DateTime = 7,
            DbDate = 133,
            DbFileTime = 137,
            DbTime = 134,
            FileTime = 64,
            Dispatch = 9,
            SmallDateTime = 135,
            Float = 5,
            UniqueIdentifier = 72,
            Integer = 3,
            UnsignedInteger = 19,
            Image = 205,
            Text = 201,
            NText = 203,
            Decimal = 131,
            Decimal2 = 14,
            Real = 4,
            SmallInt = 2,
            UnsignedSmallInt = 18,
            VarBinary = 204,
            VarChar = 200,
            VarNumeric = 139,
            Variant = 12,
            PropVariant = 138,
            NVarChar = 202,
            NChar = 130,
            UnsignedTinyInt = 17,
            TinyInt = 16,
            UserDefined = 132,
            Unsupported
        }

        /// <summary>
        /// Convert the ole db DATA_TYPE value into a .Net object typ
        /// </summary>
        /// <param name="oleDbDataType">data type object returned from the ole db connection</param>
        /// <returns>Converted Data Type in string format ex: "int = System.Int32"</returns>
        public static object OleDbToSystem(object oleDbDataType)
        {
            object systemDataType = null;

            switch ((OledbDataType)Convert.ToInt32(oleDbDataType))
            {
                case OledbDataType.DateTime:
                case OledbDataType.SmallDateTime:
                case OledbDataType.DbDate:
                case OledbDataType.DbFileTime:
                case OledbDataType.DbTime:
                case OledbDataType.FileTime:
                    systemDataType = typeof(DateTime);
                    break;
                case OledbDataType.UnsignedBigInt:
                case OledbDataType.BigInt:
                    systemDataType = typeof(Int64);
                    break;
                default:
                    systemDataType = typeof(string);
                    break;
            }

            return systemDataType.ToString();
        }

        /// <summary>
        /// Convert System type into database type for column creation
        /// </summary>
        /// <param name="systemType"></param>
        /// <returns></returns>
        public static string SystemToOleDb(string systemType)
        {
            string oleDbType = string.Empty;

            switch (systemType)
            {
                case "System.DateTime":
                    oleDbType = "datetime";
                    break;
                case "System.UInt64":
                case "System.Int64":
                    oleDbType = "bigint";
                    break;
                default:
                    oleDbType = "nvarchar";
                    break;
            }

            return oleDbType;
        }
    }
}
