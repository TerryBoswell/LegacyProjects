// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTypeConverter.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Scribe.Connector.Cdk.Sample.RS_Source
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
                case OledbDataType.Bit:
                    systemDataType = typeof(Boolean);
                    break;
                case OledbDataType.DateTime:
                case OledbDataType.SmallDateTime:
                case OledbDataType.DbDate:
                case OledbDataType.DbFileTime:
                case OledbDataType.DbTime:
                case OledbDataType.FileTime:
                    systemDataType = typeof(DateTime);
                    break;
                case OledbDataType.UniqueIdentifier:
                    systemDataType = typeof(Guid);
                    break;

                case OledbDataType.TinyInt:
                case OledbDataType.UnsignedTinyInt:
                    systemDataType = typeof(byte);
                    break;
                case OledbDataType.BigInt:
                    systemDataType = typeof(Int64);
                    break;
                case OledbDataType.UnsignedBigInt:
                    systemDataType = typeof(UInt64);
                    break;
                case OledbDataType.Binary:
                case OledbDataType.VarBinary:
                case OledbDataType.Image:
                    systemDataType = typeof(byte[]);
                    break;
                case OledbDataType.SmallInt:
                case OledbDataType.Integer:
                    systemDataType = typeof(Int32);
                    break;
                case OledbDataType.UnsignedSmallInt:
                case OledbDataType.UnsignedInteger:
                    systemDataType = typeof(UInt32);
                    break;
                case OledbDataType.Variant:
                case OledbDataType.Chapter:
                case OledbDataType.Dispatch:
                case OledbDataType.PropVariant:
                    systemDataType = typeof(object);
                    break;
                case OledbDataType.Real:
                    systemDataType = typeof(Single);
                    break;
                case OledbDataType.Money:
                case OledbDataType.Float:
                    systemDataType = typeof(double);
                    break;
                case OledbDataType.Decimal:
                case OledbDataType.Decimal2:
                    systemDataType = typeof(decimal);
                    break;
                case OledbDataType.VarChar:
                case OledbDataType.NVarChar:
                case OledbDataType.NChar:
                case OledbDataType.Text:
                case OledbDataType.NText:
                case OledbDataType.Char:
                case OledbDataType.Bstr:
                    systemDataType = typeof(string);
                    break;
                case OledbDataType.UserDefined:
                case OledbDataType.VarNumeric:
                default:
                    systemDataType = Enum.GetName(typeof(OledbDataType), OledbDataType.Unsupported);
                    break;
            }

            return systemDataType.ToString();
        }
    }
}
