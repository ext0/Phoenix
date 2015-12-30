using PhoenixServer.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeScraper.Skype
{
    public static class SkypeAggregation
    {
        public static SkypeData doWork(byte[] db)
        {
            List<SkypeConversation> conversations = SkypeOperations.processSQLDataProp<SkypeConversation>("Conversations", Path.GetRandomFileName(), db, true);
            List<SkypeMessage> messages = SkypeOperations.processSQLDataProp<SkypeMessage>("Messages", Path.GetRandomFileName(), db, true);
            List<SkypeContact> contacts = SkypeOperations.processSQLDataProp<SkypeContact>("Contacts", Path.GetRandomFileName(), db, true);
            List<SkypeAccount> accounts = SkypeOperations.processSQLDataProp<SkypeAccount>("Accounts", Path.GetRandomFileName(), db, true);
            initializeData(ref conversations, ref messages);
            return new SkypeData
            {
                conversations = conversations,
                messages = messages,
                contacts = contacts,
                accounts = accounts
            };
        }
        public static void initializeData(ref List<SkypeConversation> conversations, ref List<SkypeMessage> messages)
        {
            foreach (SkypeMessage message in messages)
            {
                foreach (SkypeConversation conversation in conversations)
                {
                    if ((conversation.identity == null) || (message.identity == null))
                        continue;
                    if (conversation.identity.Equals(message.identity))
                    {
                        conversation.messages.Add(message);
                        break;
                    }
                }
            }
            conversations.Sort();
            foreach (SkypeConversation conversation in conversations)
            {
                conversation.messages.Sort();
            }
        }
    }
    [Serializable()]
    public class SkypeData
    {
        public List<SkypeConversation> conversations { get; set; }
        public List<SkypeMessage> messages { get; set; }
        public List<SkypeContact> contacts { get; set; }
        public List<SkypeAccount> accounts { get; set; }
    }
    [Serializable()]
    public class SkypeConversation : IComparable<SkypeConversation>
    {
        [SQLite("identity")]
        public String identity { get; set; }
        [SQLite("type")]
        public int type { get; set; }
        [SQLite("displayname")]
        public String displayName { get; set; }
        [SQLite("last_activity_timestamp")]
        public int lastActivityTimestamp { get; set; }
        public List<SkypeMessage> messages = new List<SkypeMessage>();

        public int CompareTo(SkypeConversation other) //compares like skype conversation list works, high index = newer
        {
            return other.lastActivityTimestamp - lastActivityTimestamp;
        }
    }
    [Serializable()]
    public class SkypeContact
    {
        [SQLite("skypename")]
        public String skypeName { get; set; }
        [SQLite("fullname")]
        public String fullName { get; set; }
        [SQLite("gender")]
        public int gender { get; set; }
        public String genderString
        {
            get
            {
                return (gender == 1) ? "Male" : "Female";
            }
            set
            {
                genderString = value;
            }
        }
        [SQLite("phone_home")]
        public String phoneHome { get; set; }
        [SQLite("phone_office")]
        public String phoneOffice { get; set; }
        [SQLite("phone_mobile")]
        public String phoneMobile { get; set; }
        [SQLite("about")]
        public String about { get; set; }
        [SQLite("mood_text")]
        public String moodText { get; set; }
        [SQLite("isblocked")]
        public bool isBlocked { get; set; }
        [SQLite("birthday")]
        public String birthday { get; set; }
    }
    [Serializable()]
    public class SkypeMessage : IComparable<SkypeMessage>
    {
        [SQLite("chatname")]
        public String identity { get; set; }
        [SQLite("author")]
        public String author { get; set; }
        [SQLite("from_dispname")]
        public String authorDisplayName { get; set; }
        [SQLite("dialog_partner")]
        public String dialogPartner { get; set; }
        [SQLite("timestamp")]
        public int timestamp { get; set; }
        [SQLite("body_xml")]
        public String body { get; set; }
        [SQLite("chatmsg_type")]
        public int msgType { get; set; }
        [SQLite("body_is_rawxml")]
        public bool rawXml { get; set; }

        public int CompareTo(SkypeMessage other)
        {
            return timestamp - other.timestamp;
        }
    }
    [Serializable()]
    public class SkypeAccount
    {
        [SQLite("emails")]
        public String email { get; set; }
        [SQLite("about")]
        public String about { get; set; }
        [SQLite("mood_text")]
        public String moodText { get; set; }
        [SQLite("city")]
        public String city { get; set; }
        [SQLite("province")]
        public String province { get; set; }
        [SQLite("country")]
        public String country { get; set; }
        [SQLite("gender")]
        public int gender { get; set; }
        public String genderString
        {
            get
            {
                return (gender == 1) ? "Male" : "Female";
            }
            set
            {
                genderString = value;
            }
        }
        [SQLite("skypename")]
        public String skypeName { get; set; }
        [SQLite("fullname")]
        public String fullName { get; set; }
        [SQLite("birthday")]
        public String birthday { get; set; }
    }
}
