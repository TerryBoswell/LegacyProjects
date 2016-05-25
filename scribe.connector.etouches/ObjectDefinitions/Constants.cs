using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches.ObjectDefinitions
{
    public static class Constants
    {
        #region Actions
        public enum QueryAction
        {
            Query,
            Create,
            Update,
            Delete
        }

        #endregion
        #region Descriptors
        public static string Attendee_Name = "Attendee";
        public static string Attendee_FullName = "Attendee";
        public static string Attendee_Description = "Attendee";
        public static string Attendee_PK = "attendeeid";
        public static string RegSession_Name = "RegSession";
        public static string RegSession_FullName = "RegSession";
        public static string RegSession_Description = "RegSession";
        public static string RegSession_PK = "regsessionid";

        public const string Event = "Event";
        public static string Event_Name = "Event";
        public static string Event_FullName = "Event";
        public static string Event_Description = "A single Event";
        public static string Event_PK = "eventid";
        public const string Event_NameProperty = "eventname";

        public static string FinancialTranstion_Name = "FinancialTransaction";
        public static string FinancialTranstion_Description = "eSocial Financial Transactions: Charges,Payments, Credits, Taxes";
        public static string FinancialTranstion_FullName = "FinancialTransaction";
        public static string FinancialTranstion_PK = "financialtranstionid";


        public static string Meeting_Name = "Meeting";
        public static string Meeting_Description = "eSocial meeting for an event";
        public static string Meeting_FullName = "Meeting";
        public static string Meeting_PK = "meetingid";

        public static string Session_Name = "Session";
        public static string Session_Description = "A single Session";
        public static string Session_FullName = "Session";
        public static string Session_PK = "sessionid";
         //TODO:Check with Shane to determine when Session will have the sessionid property
        public const string Session_tempPk = "questionid";

        public static string Speaker_Name = "Speaker";
        public static string Speaker_Description = "A single Speaker";
        public static string Speaker_FullName = "Speaker";
        public static string Speaker_PK = "speakerid";

        public static string SessionTrack_Name = "SessionTrack";
        public static string SessionTrack_FullName = "SessionTrack";
        public static string SessionTrack_Description = "A single Session Track";
        public static string SessionTrack_PK = "sessiontrackid";
        #endregion

        public static string ConnectorTitle = "eTouch Connector";

        public static string LastModifiedParameter = "lastmodified";

        //public static string AttendeeLastModifiedParameter = "attendees_lastmodified";

        public static string BuildChildRelationship(string parent, string child)
        {
            return $"{child}_{parent}";
        }

        public static string BuildParentRelationship(string parent, string child)
        {
            return $"{parent}_{child}";
        }
    }
}
