using Sandbox.ModAPI;
using Sandbox.Common.ObjectBuilders;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.ModAPI;
using Scripts.KSWH;
using System.IO;

namespace Scripts.jukes
{
    //[Sandbox.Common.MySessionComponentDescriptor(Sandbox.Common.MyUpdateOrder.AfterSimulation)]
    [VRage.Game.Components.MySessionComponentDescriptor(VRage.Game.Components.MyUpdateOrder.AfterSimulation)]
    //class autoclean : Sandbox.Common.MySessionComponentBase
    class autoclean : VRage.Game.Components.MySessionComponentBase
    {
        class MissionData
        {
            public List<IMyIdentity> players = new List<IMyIdentity>();
            public List<IMyCubeGrid> CheckedShips = new List<IMyCubeGrid>();
            public DateTime StartTime;
        }
        private string Stoptag = ".";
        private static bool Log = true;
        public static MessageHandler MsgHandle;
        public static MyLogger Logger;
        private MissionData m_data;
        private bool Clean_Loaded;
        private HashSet<IMyEntity> m_entitiesCache = new HashSet<IMyEntity>();

        private static int cycle = 20*60*60;
        private static int cyclestop = 20*60*60;
        private static int warning = cycle -(5*60*60);
        private static int warning2 = (cycle * 2) - (5 * 60 * 60);
        private int count = cycle;
        private IUpdatableAfterSimulation[] updatables;

        public override void UpdateAfterSimulation()
        {
            try
            {
                base.UpdateAfterSimulation();

                if (!Clean_Loaded)
                    Init();

                if (!MyAPIGateway.Multiplayer.IsServer)
                    return;

                if (MyAPIGateway.Session == null)
                    return;

                if (!Clean_Loaded)
                    return;
                if (count % warning == 0)
                {
                    MsgHandle.SendNotificationAll("Server: Removing trash, ships with no beacon or antenna in 5 minutes.", 10000);
                    MsgHandle.SendNotificationAll("Server: 25 minute warning till NPC ships deleted.", 10000);
                }
                if (count % warning2 == 0)
                {
                    MsgHandle.SendNotificationAll("Server: Removing trash, ships with no beacon or antenna in 5 minutes.", 20000);
                    MsgHandle.SendNotificationAll("Server: Stopping ships / Deleting NPCS's in 5 minutes.", 20000);
                    MsgHandle.SendNotificationAll("Use ignore tag '.' (no quotes) in ship name to be skipped.", 20000);
                    MsgHandle.SendNotificationAll("Example: .Myshipname", 20000);
                }
                if (count % cycle == 0)
                {
                    MsgHandle.SendNotificationAll("Server: Automated cleanup launched.", 10000);

                    RemoveTrash();
                    //count = 1;
                }
                if (count % cyclestop == 0)
                {
                    StopallShips();
                    //if (count % cyclestop == 0)
                    if (updatables != null)
                    {
                        //for (var actionIndex = 0; actionIndex < updatables.Length; actionIndex++)
                        //updatables[actionIndex].UpdateAfterSimulation();
                        updatables[count].UpdateAfterSimulation();
                    }
                    MsgHandle.SendNotificationAll("Server: Auto-stopped / deleted NPC ships!", 20000);
                    MsgHandle.SendNotificationAll("Use ignore tag '.' (no quotes) in ship name to be skipped.", 20000);
                    MsgHandle.SendNotificationAll("Example: .Myshipname", 20000);
                    count = 1;
                }
                /*
                if (count % cyclestop == 0)
                {
                    StopallShips();
                    RemoveNPCs();
                    //Talk("cyclestop triggered!","");
                    MsgHandle.SendNotificationAll("Server: Auto-stopped / deleted NPC ships!", 20000);
                    MsgHandle.SendNotificationAll("Use ignore tag '.' (no quotes) in ship name to be skipped.", 20000);
                    MsgHandle.SendNotificationAll("Example: .Myshipname", 20000);
                    count = 1;
                }
                */
            }
            catch
            { }
            count++;
            }

        private Configuration GetConfiguration()
        {
            var config = new Configuration();

            try
            {
                var fileName = string.Format("Config_{0}.xml", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));

                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(fileName, GetType()))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(fileName, GetType()))
                    {
                        config = MyAPIGateway.Utilities.SerializeFromXML<Configuration>(reader.ReadToEnd());
                    }
                }

                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, GetType()))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                }
            }
            //catch (Exception ex)
            catch
            {
                //Logger.WriteLine("Exception in MainLogic.GetConfiguration(), using the default settings: {0}", ".");
                //derp
            }

            return config;
        }


        private void RemoveTrash()
        {
            m_entitiesCache.Clear();
            MyAPIGateway.Entities.GetEntities(m_entitiesCache, (x) => x is IMyCubeGrid && !(x as IMyCubeGrid).IsStatic && x.Physics != null);
            if (m_entitiesCache.Count == 0) return;
            Talk("Scanning ship grids for trash ...", "Count to check:" + m_entitiesCache.Count);
            foreach (IMyEntity ent in m_entitiesCache)
            {
                Talk("Is this trash? - ", (ent as IMyCubeGrid).DisplayName);


                //if (m_data.CheckedShips.IndexOf((ent as IMyCubeGrid))> 0) return;
                if (((ent as IMyCubeGrid).IsTrash() || !HasPower(ent)))
               {
                   
                   DeleteEntity(ent);
                   //Close();
               }

            }
        }

        private void StopallShips()
        {
            var Ships= new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(Ships, (x) => x is IMyCubeGrid && x.Physics != null && x.Physics.LinearVelocity.Length() >1f  && !x.DisplayName.Contains(Stoptag));
            if (Ships.Count == 0) return;
            foreach (var s in Ships)
            {
                s.Physics.ClearSpeed();
            }
        }

        //private void GetPlayers()
        private void GetPirates()
        {
            m_data.players.Clear();
            //MyAPIGateway.Players.GetAllIdentites(m_data.players, (x) => x != null && x.IdentityId != null && !x.DisplayName.Contains("Space Pirates"));
            MyAPIGateway.Players.GetAllIdentites(m_data.players, (x) => x != null && x.IdentityId != null && x.IdentityId == 144115188075855873);  //pirates have always been 144115188075855873 identity
        }

        private bool checkOwners(IMyEntity ent)
        {
            foreach (var p in m_data.players)
            {
                if ((ent as IMyCubeGrid).BigOwners.Contains(p.IdentityId)) return true;
            }
            return  false;
        }

        private void RemoveNPCs()
        {
            //GetPlayers();
            GetPirates();
            var Ships = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(Ships, (x) => x is IMyCubeGrid && x.Physics != null && !x.DisplayName.Contains(Stoptag));
            if (Ships.Count == 0) return;
            foreach (var s in Ships)
            {
                //if (!checkOwners(s))
                if (checkOwners(s))
                {
                    Talk( "Avast!" , "NPC Identified!" );
                    DeleteEntity(s);
                    
                }
            }
        }

        private void DeleteEntity (IMyEntity ent )
        {
            if (ent == null) return;
            (ent as IMyCubeGrid).SyncObject.SendCloseRequest();
            Talk(ent.DisplayName, "Deleted"); 
        }

        private bool HasPower (IMyEntity grid)
        {
            if (grid  == null) return true;
            Talk(grid.DisplayName, "Checking for Beacons, Antenna or locked gear");  
            var blocksR=new List<IMySlimBlock>();
            //(grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_Reactor);
            //Talk(grid.DisplayName, "Found Reactors:" + blocksR.Count);
            //if (blocksR.Count > 0)
            //{
            //    foreach (var react in blocksR)
            //    {
            //        if ((react.GetObjectBuilder() as MyObjectBuilder_Reactor).Inventory.Items.Count > 0) return true;
            //   }
            //    blocksR.Clear();
                //Talk(grid.DisplayName, "Found Powered Reactors:" + blocksR.Count);
            //}
            //(grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_BatteryBlock);
            //if (blocksR.Count > 0)
            //{
            //    foreach (var react in blocksR)
            //   {
            //        if ((react.GetObjectBuilder() as MyObjectBuilder_BatteryBlock).CurrentStoredPower > 0f) return true;
            //    }
            //   blocksR.Clear();
                //Talk(grid.DisplayName, "Found Charged Battery!");
            //}
            //(grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_SolarPanel);
            //if (blocksR.Count > 0)
            //{
            //    blocksR.Clear();
            //    return true;
            //}
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_Beacon);
            if (blocksR.Count > 0)
            {
                blocksR.Clear();
                return true;

            }
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_RadioAntenna);
            if (blocksR.Count > 0)
            {
                blocksR.Clear();
                return true;

            }
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_LandingGear);
            if (blocksR.Count > 0)
            {
                foreach (var react in blocksR)
                {
                    if ((react.GetObjectBuilder() as MyObjectBuilder_LandingGear).IsLocked) return true;
                }
                blocksR.Clear();
                //Talk(grid.DisplayName, "Found Locked Gear!");
            }
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_ShipConnector);
            if (blocksR.Count > 0)
            {
                foreach (var react in blocksR)
                {
                    if ((react.GetObjectBuilder() as MyObjectBuilder_ShipConnector).Connected) return true;
                }
                blocksR.Clear();
                //Talk(grid.DisplayName, "Found Locked Connectors!");
            }
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_Wheel);
            if (blocksR.Count > 0)
            {
                blocksR.Clear();
                return true;

            }
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_MotorRotor);
            if (blocksR.Count > 0)
            {
                blocksR.Clear();
                return true;

            }
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_MotorAdvancedRotor);
            if (blocksR.Count > 0)
            {
                blocksR.Clear();
                return true;

            }
            (grid as IMyCubeGrid).GetBlocks(blocksR, (x) => (MyObjectBuilder_CubeBlock)x.GetObjectBuilder() is MyObjectBuilder_PistonTop);
            if (blocksR.Count > 0)
            {
                blocksR.Clear();
                return true;

            }

            //Talk(grid.DisplayName, "Found Solar Panels!");
            blocksR.Clear();
            return false;
        }

        private void Init()
        {
            Logger = new MyLogger("Log.log");
            MsgHandle = new MessageHandler();
            m_data = new MissionData();
            var updatables = new List<IUpdatableAfterSimulation>();
            var config = GetConfiguration();
            Clean_Loaded = true;
            if (!MyAPIGateway.Multiplayer.IsServer)  return;
            StopallShips();
            if (config.PirateGridDeletion_Enabled)
            updatables.Add(new PirateDeleter(
            config.PirateGridDeletion_Interval,
            //config.PirateGridDeletion_Enabled,
            config.PirateGridDeletion_PlayerDistanceThreshold,
            //config.PirateGridDeletion_BlockCountThreshold,
            config.PirateGridDeletion_MessageAdminsOnly));
            RemoveNPCs();
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            if (MsgHandle != null)
            {
                MsgHandle.Close();
                MsgHandle = null;
            }
            if (Logger != null)
            {
                Logger.Close();
                Logger = null;
            }
            if (updatables != null)
                foreach (var updatable in updatables)
                    updatable.Close();


        }

        public static void Talk(String who, String what){
            if (!Log)
                return;
            Logger.WriteLine(who + " :: " + what);
            MyAPIGateway.Utilities.ShowMessage(who, what);
        }
    }
}
