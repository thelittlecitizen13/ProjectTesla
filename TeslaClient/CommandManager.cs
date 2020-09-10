using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TeslaCommon;

namespace TeslaClient
{
    public class CommandManager
    {
        private TeslaClient _teslaClient;
        private GroupCommandsHandler _groupCommandsHandler;
        private PicturesCommandHandler _picturesCommandHandler;
        public CommandManager(TeslaClient teslaClient)
        {
            _teslaClient = teslaClient;
            _groupCommandsHandler = new GroupCommandsHandler(_teslaClient);
            _picturesCommandHandler = new PicturesCommandHandler(_teslaClient);
        }
        //public bool IsCommandValid()
        //{

        //}
        public IMessage GenerateMessageFromCommand(string[] args, IMemberData source, IMemberData destination)
        {
            try
            {
                args = args.Select(str => str.ToLower()).ToArray();
                switch (args[0])
                {
                    case "/group":
                        return _groupCommandsHandler.GenerateGroupMessage(args);
                    case "/picture":
                        return _picturesCommandHandler.GeneratePictureMessage(args, source, destination);
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }
        

        public string GetCommandHelp()
        {
            return GetBasicCommandsHelp() + _groupCommandsHandler.GetGroupCommandHelp();
        }
        public string GetChatCommands()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Type /picture to send a screen shot");
            sb.AppendLine("Type /picture;[path] to send an existing local picture");
            sb.AppendLine("Type /exit to exit chat");
            sb.AppendLine();
            return sb.ToString();
        }
        public string GetBasicCommandsHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Type /help to see available commands (like group controlling)");
            sb.AppendLine("Type /refresh to refresh contacts");
            sb.AppendLine("Type /notifications to disable/enable unseen messages notifications");
            sb.AppendLine("Type /exit to exit");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
