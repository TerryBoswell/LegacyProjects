﻿using EasyHttp.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scribe.Core.ConnectorApi.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Collections.Concurrent;

namespace Scribe.Connector.etouches
{
    public static class DataUtility
    {
        private static ConcurrentDictionary<string, ConcurrentBag<string>> DatasetKeys =
                                new ConcurrentDictionary<string, ConcurrentBag<string>>();
        private static string Generatekey(Guid connectionKey, string action, string accountId, string eventId = null,
            string aQuery = null, Dictionary<string, string> keypairs = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null)
        {
            var key = $"{connectionKey}-{action}-{accountId}-{eventId}-{aQuery}-{modifiedAfter}-{modifiedBefore}";
            if (keypairs != null && keypairs.Any())
                foreach (var kp in keypairs)
                    key = $"{key}-{kp.Key}-{kp.Value}";
            //Each Time we generate a key for all the parameters except key pairs we will store it in the DataSetKeys
            //if (keypairs == null)
            //    StoreGeneratedKey(connectionKey, action, key);
            return key;
        }

        public static JObject GetJObject(string baseUrl, Extensions.Actions action, string accesstoken, string accountId = null,
            string eventId = null)
        {
            var strAction = action.Name();
            HttpResponse result = DoHttpGetInternal(baseUrl, strAction, accesstoken, accountId, eventId);
            if (result == null)
            {
                Logger.WriteError("Result of get was null in GetJObject");
                throw new ApplicationException("Result of get was null");
            }
            var res = result.RawText;
            var json = JObject.Parse(res);
            if (json != null)
                Logger.WriteDebug($"The action {strAction} successfully returned in GetJobject");
            return json;
        }


        public static DataSet GetDataset(ScribeConnection connection, Extensions.Actions action, 
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null,
            Dictionary<string, string> keypairs = null)
        {
            var strAction = action.Name();
            var isKeyPairs = keypairs != null;
            var key = Generatekey(connection.ConnectionKey, strAction, connection.AccountId, connection.EventId, null, keypairs);
            DataSet ds = ConnectorCache.GetCachedData<DataSet>(key);
            if (ds != null)
                   return ds;

            //If we do not have key pairs, will load all pages for the dataset
            ds = GetCompleteDatasetIteratively(connection, strAction, connection.AccountId, eventId, modifiedAfter, modifiedBefore, additionCondition, keypairs);

            if (ds != null)
                ConnectorCache.StoreData(key, ds, connection.TTL);

            if (!isKeyPairs)
                StoreGeneratedKey(connection.ConnectionKey, strAction, key);

            return ds;
        }

        private static void StoreGeneratedKey(Guid connectionKey, string action, string key)
        {
            //We should only have one record per dataset key
            //The dataset key represents an action and a connection key
            var dsKey = $"{connectionKey}-{action}";

            ConcurrentBag<string> bag = new ConcurrentBag<string>();
            bag.Add(key);
            DatasetKeys.AddOrUpdate(dsKey, bag, (s, i) =>
            {
                if (!i.Contains(key))
                    i.Add(key);
                return i;
            });
        }

        //private static DataSet GetKeyPairValueFromMemory(Guid connectionKey, string action,
        //    string accountId, string eventId,
        //    Dictionary<string, string> keypairs = null)
        //{
        //    var dsKey = $"{connectionKey}-{action}";
        //    ConcurrentBag<string> bag;
        //    if (DatasetKeys.TryGetValue(dsKey, out bag))
        //    {
        //        foreach (var key in bag.AsEnumerable())
        //        {
        //            var ds = ConnectorCache.GetCachedData<DataSet>(key);
        //        }
        //    }
        //}


        private static DataSet GetCompleteDatasetIteratively(ScribeConnection connection, string action, string accountId = null,
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null,
            Dictionary<string, string> keypairs = null)
        {
            var hasMoreRecords = true;
            DataSet returnSet = new DataSet();
            int pageNumber = 1;
            while (hasMoreRecords)
            {
                var ds = GetDatasetIteratively(connection, action, accountId, eventId, null, null, null, null, pageNumber);
                //While there are 
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    returnSet.copyDataTable(ds);
                    if (ds.Tables[0].Rows.Count < connection.PageSize)
                        hasMoreRecords = false;
                }
                else
                    hasMoreRecords = false;
                pageNumber++;
            }
            return returnSet;
        }


        private static void copyDataTable(this DataSet copyToDs, DataSet copyFromDs)
        {

            if (copyToDs.Tables.Count == 0)
            {
                copyToDs.Merge(copyFromDs);
                return;
            }

            var copyTo = copyToDs.Tables[0];
            var copyFrom = copyFromDs.Tables[0];

            for(int j = 0; j < copyFrom.Rows.Count; j++)
            {
                var row = copyFrom.Rows[j];
                var newRow = copyTo.NewRow();
                newRow.BeginEdit();
                for (int i = 0; i < copyTo.Columns.Count; i++)
                {
                    var origColumn = copyTo.Columns[i];
                    var newColumn = copyFrom.Columns[i];
                    if (row[i] != DBNull.Value)
                        newRow[i] = Convert.ChangeType(row[i], origColumn.DataType);


                }
                newRow.EndEdit();
                copyTo.Rows.Add(newRow);
            }
        }

        //private static DataSet GetDatasetIteratively(ScribeConnection connection, string action, string accountId = null,
        //    string eventId = null, Dictionary<string, string> keypairs = null)
        //{
        //    return GetDatasetIteratively(connection, action, accountId, eventId, null, null, null, keypairs);
        //}


        /// <summary>
        /// This method will load a dataset iteratively. It converts one row and column at a time
        /// it protects against properties that are collections ignoring them
        /// it will protect against unix representation of null dates
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="action"></param>
        /// <param name="accesstoken"></param>
        /// <param name="accountId"></param>
        /// <param name="eventId"></param>
        /// <param name="modifiedAfter"></param>
        /// <param name="modifiedBefore"></param>
        /// <returns></returns>
        private static DataSet GetDatasetIteratively(ScribeConnection connection, string action, string accountId = null,
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null,
            Dictionary<string, string> keypairs = null, int? pageNumber = null)
        {
            HttpResponse result = DoHttpGetInternal(connection.BaseUrl, action, connection.AccessToken,
                accountId, eventId, modifiedAfter, modifiedBefore, additionCondition, connection.PageSize, keypairs, pageNumber);

            //We are going to try to reconnnect one time
            if (!String.IsNullOrEmpty(result.RawText) && result.RawText.StartsWith("{\"status\":\"error\",\"msg\":\"Not authorized to access account"))
            {
                connection.ReConnnect();
                if (connection.IsConnected)
                    result = DoHttpGetInternal(connection.BaseUrl, action, connection.AccessToken,
                        accountId, eventId, modifiedAfter, modifiedBefore, additionCondition, connection.PageSize, keypairs, pageNumber);
            }

            if (String.IsNullOrEmpty(result.RawText))
            {
                Logger.WriteError("Result of the get was empty");
                throw new ApplicationException("Result of the get was empty");
            }
            DataSet ds = null;
            string plainJson = result.RawText;
            try
            {
                ds = ConvertDataSetIteratively(plainJson);
            }
            catch (Exception ex)
            {
                var msg = $"Error : {ex.Message} while deserializing {plainJson}";
                Logger.WriteError(msg);
                throw new ApplicationException(msg);
            }
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                var msg = $"The Action {action} return {ds.Tables[0].Rows.Count} records";
                Logger.WriteDebug(msg);
            }
            return ds;
        }
        public static DataSet ConvertDataSetIteratively(string json)
        {
            var jsonLinq = JObject.Parse(json);

            // Find the first array using Linq
            var srcArray = jsonLinq.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types
                    if (column.Value is JValue)
                    {
                        //We need to clean up columns of type date that are represented this way for nulls
                        if (column.Value.ToString().Equals("0000-00-00 00:00:00", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0000 - 00 - 00", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0000-00-00", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0001-01-01 00:00:00", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0001 - 01 - 01", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        if (column.Value.ToString().Equals("0001-01-01", StringComparison.OrdinalIgnoreCase))
                            column.Value = getDefaultDate();
                        cleanRow.Add(column.Name, column.Value);
                    }
                }

                trgArray.Add(cleanRow);
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(JsonConvert.DeserializeObject<DataTable>(trgArray.ToString()));
            ds.Tables[0].TableName = "ResultSet";
            return ds;
        }

        private static String getDefaultDate()
        {
            return String.Empty;
            //return System.DateTime.MinValue.ToString("yyyy-MM-dd");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="action"></param>
        /// <param name="accesstoken"></param>
        /// <param name="accountId"></param>
        /// <param name="eventId"></param>
        /// <param name="modifiedAfter">Using standard lastmodified-gt parameter</param>
        /// <param name="modifiedBefore">Using standard lastmodified-lt=</param>
        /// <param name="additionCondition">A custom Query parameter to pass that is not a common one</param>
        /// <returns></returns>
        private static HttpResponse DoHttpGetInternal(string baseUrl, string action, string accesstoken, string accountId = null,
            string eventId = null, DateTime? modifiedAfter = null, DateTime? modifiedBefore = null, string additionCondition = null,
            int? pageSize = null, Dictionary<string, string> keypairs = null, int? pageNumber = null)
        {
            var http = new HttpClient();
            //var uri = new UriBuilder(baseUrl);
            if (String.IsNullOrEmpty(action))
                throw new ApplicationException("An Action must be provided");
            if (String.IsNullOrEmpty(baseUrl))
                throw new ApplicationException("A base url must be provided");
            if (String.IsNullOrEmpty(accesstoken))
                throw new ApplicationException("An access token must be provided");
            if (String.IsNullOrEmpty(accountId) && !String.IsNullOrEmpty(eventId))
                throw new ApplicationException("An account id must be provided when an event id is provided");

            var path = string.Empty;
            if (String.IsNullOrEmpty(eventId))
                path = $"{baseUrl}/{action}/{accountId}?accesstoken={accesstoken}";
            else
                path = $"{baseUrl}/{action}/{accountId}/{eventId}?accesstoken={accesstoken}";

            if (modifiedBefore.HasValue)
            {
                var ltDate = modifiedBefore.Value.ToString("yyyy-MM-dd");
                path = $"{path}&lastmodified-lt='{ltDate}'";
            }
            if (modifiedAfter.HasValue)
            {
                var gtDate = modifiedAfter.Value.ToString("yyyy-MM-dd");
                path = $"{path}&lastmodified-gt='{gtDate}'";
            }

            if (!String.IsNullOrEmpty(additionCondition))
                path = $"{path}&{additionCondition}";

            if (pageSize.HasValue)
                path = $"{path}&pageSize={pageSize.Value}";
            if (pageNumber.HasValue)
                path = $"{path}&pageNumber={pageNumber.Value}";
            if (keypairs != null && keypairs.Any())
                foreach (var kp in keypairs)
                    path += $"&{kp.Key}={kp.Value}";
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var result = http.Get(path);
            sw.Stop();
            Logger.WriteDebug($"Execution of {path} took {sw.Elapsed.TotalSeconds} seconds");
            return result;
        }


    }
}