// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTypeConverter.cs" company="Scribe Software Corporation">
//   Copyright © 1996-2011 Scribe Software Corp. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Data;

namespace Scribe.Connector.Cdk.Sample.SYS
{
    using System;

    class DataTypeConverter
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
            Unsupported = 0
        }

        /// <summary>
        /// Converts the numeric value received from the ole db connection to the string representaion
        /// </summary>
        /// <param name="oleDbDataType">data type value returned from the ole db connection</param>
        /// <returns></returns>
        public static string NumericOleDbToString(object oleDbDataType)
        {
            var oleDbType = (OledbDataType)Convert.ToInt32(oleDbDataType);
            return oleDbType.ToString();
        }

        /// <summary>
        /// Convert the ole db DATA_TYPE value into a .Net object typ
        /// </summary>
        /// <param name="oleDbDataType">data type object returned from the ole db connection</param>
        /// <returns>Converted Data Type in string format ex: "int = System.Int32"</returns>
        public static object OleDbToSystem(object oleDbDataType)
        {
            object systemDataType = OledbDataType.Unsupported;

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
            }

            return systemDataType.ToString();
        }

        /// <summary>
        /// Converts internal system data to SQL data objects for putting data back into Salesforce.
        /// </summary>
        /// <param name="columnName">name of the column being processed</param>
        /// <param name="columnData">data to be processed</param>
        /// <param name="tableDefinition">data table contained column deifnitions</param>
        /// <returns>data object that can be used by Sql </returns>
        public static object ToSqlValue(string columnName, object columnData, DataTable tableDefinition)
        {
            string dataType = string.Empty;
            object dataValue;

            // retrieve the data type from the table definition
            foreach (DataRow row in tableDefinition.Rows)
            {
                if (row["COLUMN_NAME"].ToString() == columnName)
                {
                    dataType = row["DATA_TYPE"].ToString();
                }
            }

            //verify that a data type was found
            if (string.IsNullOrWhiteSpace(dataType))
            {
                throw new ArgumentException(string.Format(ErrorCodes.InvalidQueryColumn.Description, columnName,
                                                          tableDefinition.TableName));
            }

            OledbDataType internalSqlDataType;

            //attempt a retrieval of the data type that was found
            if (Enum.TryParse(dataType, out internalSqlDataType) == false)
            {
                internalSqlDataType = OledbDataType.Unsupported;
            }

            try
            {
                //convert data using the appropriate convertion method
                switch (internalSqlDataType)
                {
                    case OledbDataType.BigInt:
                        dataValue = Convert.ToInt64(columnData);
                        break;
                    case OledbDataType.UnsignedBigInt:
                        dataValue = Convert.ToUInt64(columnData);
                        break;
                    case OledbDataType.Integer:
                        dataValue = Convert.ToInt32(columnData);
                        break;
                    case OledbDataType.UnsignedInteger:
                        dataValue = Convert.ToUInt32(columnData);
                        break;
                    case OledbDataType.SmallInt:
                        dataValue = Convert.ToInt16(ConvertBooleanToStandardValue(columnData));
                        break;
                    case OledbDataType.UnsignedSmallInt:
                        dataValue = Convert.ToUInt16(columnData);
                        break;
                    case OledbDataType.TinyInt:
                    case OledbDataType.UnsignedTinyInt:
                        dataValue = Convert.ToByte(ConvertBooleanToStandardValue(columnData));
                        break;
                    case OledbDataType.Bit:
                        dataValue = Convert.ToBoolean(ConvertStandardValueToBoolean(columnData));
                        break;
                    case OledbDataType.UniqueIdentifier:
                        dataValue = Guid.Parse(columnData.ToString());
                        break;
                    case OledbDataType.DateTime:
                    case OledbDataType.SmallDateTime:
                    case OledbDataType.DbDate:
                    case OledbDataType.DbFileTime:
                    case OledbDataType.FileTime:
                        dataValue = Convert.ToDateTime(columnData);
                        break;
                    case OledbDataType.DbTime:
                        dataValue = TimeSpan.Parse(columnData.ToString());
                        break;
                    case OledbDataType.Decimal:
                    case OledbDataType.Decimal2:

                        dataValue = Convert.ToDecimal(columnData);
                        break;
                    case OledbDataType.Float:
                    case OledbDataType.Money:
                        dataValue = Convert.ToDouble(columnData);
                        break;
                    case OledbDataType.Real:
                        dataValue = Convert.ToSingle(columnData);
                        break;
                    case OledbDataType.NText:
                    case OledbDataType.NVarChar:
                    case OledbDataType.VarChar:
                    case OledbDataType.NChar:
                    case OledbDataType.Text:
                    case OledbDataType.Char:
                    case OledbDataType.Bstr:
                        dataValue = columnData.ToString();
                        break;
                    case OledbDataType.Binary:
                    case OledbDataType.VarBinary:
                        dataValue = System.Text.Encoding.ASCII.GetBytes(columnData.ToString());
                        break;
                    case OledbDataType.Image:
                    case OledbDataType.Variant:
                    case OledbDataType.Chapter:
                    case OledbDataType.Dispatch:
                    case OledbDataType.PropVariant:
                        dataValue = columnData;
                        break;
                    default:
                        dataValue = Enum.GetName(typeof(OledbDataType), OledbDataType.Unsupported);
                        break;
                }
            }
            catch (FormatException formatException)
            {
                var additionalInfo = string.Format(ErrorCodes.InvalidQueryDataType.Description, columnData.GetType(), columnData);
                throw new FormatException(additionalInfo, formatException);
            }

            return dataValue;
        }

        /// <summary>
        /// Convert boolean values to valid bit values
        /// </summary>
        /// <param name="fieldValue">value to be added</param>
        /// <returns>converted value of it needed to be changed</returns>
        private static object ConvertBooleanToStandardValue(object fieldValue)
        {
            object value;

            //convert boolean values
            switch (fieldValue.ToString().ToLower())
            {
                case "true":
                    value = 1;
                    break;
                case "false":
                    value = 0;
                    break;
                default:
                    value = fieldValue;
                    break;
            }

            return value;
        }

        /// <summary>
        /// Convert binary values to valid boolean
        /// </summary>
        /// <param name="fieldValue">value to be added</param>
        /// <returns>converted value of it needed to be changed</returns>
        private static object ConvertStandardValueToBoolean(object fieldValue)
        {
            object value;

            //convert boolean values
            switch (fieldValue.ToString().ToLower())
            {
                case "1":
                    value = true;
                    break;
                case "0":
                    value = false;
                    break;
                default:
                    value = fieldValue;
                    break;
            }

            return value;
        }
    }
}
