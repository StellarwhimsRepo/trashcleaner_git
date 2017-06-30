using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Scripts.KSWH;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Scripts.jukes
{


    public enum MessageType : ushort
    {
        notify = 1,
    }
    [ProtoBuf.ProtoContract]
    [Serializable]
    public struct GenericMessage
    {
        [ProtoBuf.ProtoMember(1)]
        public MessageType msgtyp;     // Message type
        [ProtoBuf.ProtoMember(2)]
        public String text;
        [ProtoBuf.ProtoMember(3)]
        public int time;
    }
    class MessageHandler{
        public const ushort messageid_zerty = 17842; 
        public bool Message_init;
        public static MyLogger Logger;

        public MessageHandler()
        {
            Logger = new MyLogger("messages.log");
            Logger.WriteLine("message handler started");
            if (!Message_init) init();
            
        }
        public void init()
        {
            try
            {
                MyAPIGateway.Multiplayer.RegisterMessageHandler(messageid_zerty, MessageHandler.ReceiveNetworkMessage);
                Logger.WriteLine("Messageclass registered");
                Message_init = true;
            }
            catch (Exception ex) { Logger.WriteLine(ex.ToString()); }
        }
        public void Close()
        {
            try
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(messageid_zerty, MessageHandler.ReceiveNetworkMessage);
                Message_init = true;
            }
            catch (Exception ex) { Logger.WriteLine(ex.ToString()); }
            if (Logger != null)
            {
                Logger.Close();
                Logger = null;
            }
        }
        public void SendNotificationAll(String Message, int time=2000)
        {
            Logger.WriteLine("message:  " + Message + " :: to all");
            var msg = new GenericMessage();
            msg.msgtyp = MessageType.notify;
            msg.text = Message;
            msg.time = time;

            var strmsg = MyAPIGateway.Utilities.SerializeToXML<GenericMessage>(msg);
            var bytemsg = Encoding.ASCII.GetBytes(strmsg);


            if (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE)
                MessageHandler.ReceiveNetworkMessage(bytemsg);
            else
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players, p => p != null);
                foreach (var player in players) MyAPIGateway.Multiplayer.SendMessageTo(messageid_zerty, bytemsg, player.SteamUserId);
            }
        }
        public void SendNotificationTo(IMyPlayer player, String Message, int time = 2000)
        {
            Logger.WriteLine("message:  " + Message + " :: to " + player.SteamUserId);
            var msg = new GenericMessage();
            msg.msgtyp = MessageType.notify;
            msg.text = Message;
            msg.time = time;

            var strmsg = MyAPIGateway.Utilities.SerializeToXML<GenericMessage>(msg);
            var bytemsg = Encoding.ASCII.GetBytes(strmsg);

            if (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE)
                MessageHandler.ReceiveNetworkMessage(bytemsg);
            else
                MyAPIGateway.Multiplayer.SendMessageTo(messageid_zerty, bytemsg, player.SteamUserId);
            
        }
        public static void ReceiveNetworkMessage( byte[] bmsg)
        {
            var strmsg = Encoding.Default.GetString(bmsg);
            var msg = MyAPIGateway.Utilities.SerializeFromXML<GenericMessage>(strmsg);
            if (msg.msgtyp == MessageType.notify)
            {
                Logger.WriteLine("message recived: "+msg.text);
                MyAPIGateway.Utilities.ShowNotification(msg.text, msg.time, MyFontEnum.Green );
            }
        }
    }
}

