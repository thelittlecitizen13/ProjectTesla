using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaCommon
{
    public class ClientData
    {
        public string UID { get; set; }
        public string ClientName { get; set; }
        public ClientData(string clientName)
        {
            ClientName = clientName;
            UID = Guid.NewGuid().ToString();
        }
    }
}
