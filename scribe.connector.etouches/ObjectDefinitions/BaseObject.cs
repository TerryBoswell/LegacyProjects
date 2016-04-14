using Newtonsoft.Json.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    abstract class BaseObject : ObjectDefinition
    {
        protected ScribeConnection Connection;
        protected Core.ConnectorApi.Query.Query Query;
        protected System.DateTime? ModifiedAfterDate = null;
        protected System.DateTime? AttendeeModifiedAfterDate = null;
        protected bool HasChildren;
        protected Dictionary<string, string> KeyPairs = new Dictionary<string, string>();
        protected List<string> ChildNames = new List<string>();

        public BaseObject(ScribeConnection connection, string name, string fullName, string description) :
            this(connection, name, fullName, description, Constants.QueryAction.Query)
        { }
        public BaseObject(ScribeConnection connection, string name, string fullName, string description,
            params Constants.QueryAction[] actions)
        {
            this.Connection = connection;
            FullName = fullName;
            Description = description;
            Hidden = false;
            Name = name;
            SupportedActionFullNames = new List<string>();
            if (actions == null || !actions.Any())
                SupportedActionFullNames.Add(Constants.QueryAction.Query.ToString());
            else
                foreach (var a in actions)
                    SupportedActionFullNames.Add(a.ToString());
        }



        virtual protected void SetPropertyDefinitions(JObject o)
        {
            PropertyDefinitions = new List<IPropertyDefinition>();

            //populate the rest of the properties
            JArray fields = (JArray)o["fields"];

            foreach (JObject field in fields)
            {
                var presentationType = ((string)field["presentationType"]);
                PropertyDefinitions.Add(new PropertyDefinition
                {  //TODO: Fill these in from the metadata results as soon as the API support them
                    Description = (string)field["description"],
                    FullName = (string)field["fullName"],
                    IsPrimaryKey = ((string)field["isPrimaryKey"] == "yes"),
                    Nullable = ((string)field["nullable"] == "yes"),
                    MaxOccurs = 1,
                    MinOccurs = 0,
                    Name = (string)field["fullName"],
                    PresentationType = presentationType,
                    PropertyType = (string)field["presentationType"],
                    UsedInQuerySelect = true,
                    UsedInLookupCondition = true,
                    UsedInQueryConstraint = true,
                    UsedInActionInput = true,
                    UsedInActionOutput = true
                });

            }
        }

        /// <summary>
        /// This method will set any parameters that can be passed directly
        /// to the restful web calls vs querying in memory
        /// </summary>
        /// <param name="query"></param>
        virtual protected void SetQuery(Core.ConnectorApi.Query.Query query)
        {
            this.Query = query;
            if (query != null && query.Constraints != null && query.Constraints.ExpressionType == ExpressionType.Comparison)
            {
                ComparisonExpression lookupCondition = query.Constraints as ComparisonExpression;

                //We need to deal with the equality operator since the api supports exact match on key pair values
                if (lookupCondition.Operator == ComparisonOperator.Equal 
                    && lookupCondition.RightValue.Value != null)
                {
                    var key = lookupCondition.LeftValue.Value.ToString().Replace($"{this.Name}.", string.Empty);
                    var value = lookupCondition.RightValue.Value.ToString();
                    this.KeyPairs.Add(key, value);
                }

                //We have to make a presumption that we will key off the name last modified from our meta data
                if (lookupCondition.Operator == ComparisonOperator.Greater && lookupCondition.LeftValue.Value.ToString().Contains($"{Constants.LastModifiedParameter}")
                    && lookupCondition.RightValue.Value != null)
                {
                    System.DateTime d;
                    if (System.DateTime.TryParse(lookupCondition.RightValue.Value.ToString(), out d))
                        this.ModifiedAfterDate = d;
                }


                //if (lookupCondition.Operator == ComparisonOperator.Greater && lookupCondition.LeftValue.Value.ToString().Contains($".{Constants.AttendeeLastModifiedParameter}")
                //    && lookupCondition.RightValue.Value != null)
                //{
                //    System.DateTime d;
                //    if (System.DateTime.TryParse(lookupCondition.RightValue.Value.ToString(), out d))
                //        this.AttendeeModifiedAfterDate = d;
                //}
                
            }

            if (query != null && query.RootEntity != null && query.RootEntity.ChildList != null && query.RootEntity.ChildList.Count > 0)
            {
                this.HasChildren = true;
                query.RootEntity.ChildList.ForEach(c => this.ChildNames.Add(c.Name));
            } 
        }

        virtual protected IEnumerable<DataEntity> GetDataEntites(System.Data.DataSet ds, Core.ConnectorApi.Query.Query query)
        {
            var table = ds.Tables["ResultSet"];
            if (table.Rows.Count == 0)
                return new List<DataEntity>();
            var filteredRows = table.Select(query.ToSelectExpression());
            return filteredRows.ToDataEntities(query.RootEntity.ObjectDefinitionFullName);
        }
        
    }
}
