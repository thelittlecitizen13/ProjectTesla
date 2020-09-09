using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class ChatHistories
    {
        public Dictionary<string, Queue<IMessage>>Histories { get; private set; }
        
        public ChatHistories()
        {
            Histories = new Dictionary<string, Queue<IMessage>>();
        }
        public void AddToChatHistory(IMessage msg)
        {
            if (Histories.ContainsKey(msg.Source.UID))
            {
                Histories[msg.Source.UID].Enqueue(msg);
            }
            else
            {
                Queue<IMessage> newMemberHistory = new Queue<IMessage>();
                newMemberHistory.Enqueue(msg);
                Histories.Add(msg.Source.UID, newMemberHistory);
            }    
        }
        public IMessage GetLastMessage(IMemberData currentChatMember)
        {
            if (currentChatMember == null || Histories.Count == 0)
                return null;
            if (Histories.ContainsKey(currentChatMember.UID) && Histories[currentChatMember.UID].Count != 0)
            {
                return Histories[currentChatMember.UID].Dequeue();
            }
            return null;
        }
    }
}
