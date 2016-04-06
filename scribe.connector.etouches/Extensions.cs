﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches
{
    public static class Extensions
    {
        public enum Actions
        {
            Event,
            Attendee,
            RegSession,
            Speaker,
            Session,
            SessionTrack,
            Meeting,
            FinanacialTransaction,
            EventMeta,
            SessionMeta,
            AttendeeMeta,
            RegSessionMeta,
            SpeakerMeta,
            SessionTrackMeta,
            MeetingMeta,
            FinancialTransactionMeta,
            EventCreate
        }

        public static string Name(this Actions a)
        {
            switch (a)
            {
                case Actions.MeetingMeta:
                    return "meetingmetadata.json";
                case Actions.SessionTrackMeta:
                    return "sessiontrackmetadata.json";
                case Actions.SpeakerMeta:
                    return "speakermetadata.json";
                case Actions.RegSessionMeta:
                    return "regsessionmetadata.json";
                case Actions.AttendeeMeta:
                    return "attendeemetadata.json";
                case Actions.SessionMeta:
                    return "sessionmetadata.json";
                case Actions.EventMeta:
                    return "eventmetadata.json";
                case Actions.FinancialTransactionMeta:
                    return "financialtransactionmetadata.json";
                case Actions.Event:
                   return "eventlist.json";
                case Actions.Attendee:
                    return "attendeelist.json";
                case Actions.RegSession:
                    return "regsessionlist.json";
                case Actions.Speaker:
                    return "speakerlist.json";
                case Actions.Session:
                    return "sessionlist.json";
                case Actions.SessionTrack:
                    return "sessiontracklist.json";
                case Actions.Meeting:
                    return "meetinglist.json";
                case Actions.FinanacialTransaction:
                    return "financialtransactionlist.json";
                case Actions.EventCreate:
                    return "createEvent.json";
                default:
                    throw new ApplicationException("Unknown List");
            }
        }
    }
}
