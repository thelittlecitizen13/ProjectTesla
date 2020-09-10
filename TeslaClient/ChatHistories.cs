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
        public Dictionary<string, int> HistoryStatus { get; private set; }


        public ChatHistories()
        {
            Histories = new Dictionary<string, Queue<IMessage>>();
            HistoryStatus = new Dictionary<string, int>();
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
            increaseMemberHistoryStatus(msg.Source.UID);
        }
        public IMessage GetLastMessage(IMemberData currentChatMember)
        {
            if (currentChatMember == null || Histories.Count == 0)
                return null;
            if (Histories.ContainsKey(currentChatMember.UID) && Histories[currentChatMember.UID].Count != 0)
            {
                decreaseMemberHistoryStatus(currentChatMember.UID);
                return Histories[currentChatMember.UID].Dequeue();
            }
            return null;
        }
        private void increaseMemberHistoryStatus(string memberUID)
        {
            if (HistoryStatus.ContainsKey(memberUID))
                HistoryStatus[memberUID]++;
            else
                HistoryStatus.Add(memberUID, 1);
        }
        private void decreaseMemberHistoryStatus(string memberUID)
        {
            if (HistoryStatus.ContainsKey(memberUID))
            {
                HistoryStatus[memberUID]--;
                if (HistoryStatus[memberUID] == 0)
                    HistoryStatus.Remove(memberUID);
            }
            else
                return;
        }

    }
}
