using Sandbox.ModAPI;
using Scripts.KSWH;
using System;
using System.Linq;

using VRage.Game.ModAPI;

namespace Scripts.jukes
{
	/// <summary>
	/// Deleter of cubegrids that have Pirate majority owners.
	/// </summary>
	public class PirateDeleter : RepeatedDeleter<IMyCubeGrid, CubeGridDeletionContext>
	{
        private bool pirateGridDeletion_Enabled;
        private double pirateGridDeletion_PlayerDistanceThreshold;
        private bool pirateGridDeletion_MessageAdminsOnly;
        private static bool Log = true;
        private static MyLogger PDLLogger;

        public PirateDeleter(double interval, double playerDistanceThreshold, bool messageAdminsOnly)
			: base(interval, messageAdminsOnly, new CubeGridDeletionContext() { PlayerDistanceThreshold = playerDistanceThreshold })
		{
		//	BlockCountThreshold = blockCountThreshold;
		}

        protected override bool BeforeDelete(IMyCubeGrid entity, CubeGridDeletionContext context)
		{
            if (entity.BigOwners.Contains(144115188075855873))
            {
                Talk("Avast!", "NPC Identified!");
                Talk(entity.DisplayName, "Deleted");
                return true;

            }

			//context.CurrentEntitySlimBlocks.Clear();
			//entity.GetBlocksIncludingFromStaticallyAttachedCubeGrids(context.CurrentEntitySlimBlocks);

			//if (context.CurrentEntitySlimBlocks.Count > BlockCountThreshold)
				//return false;

			//if (context.CurrentEntitySlimBlocks.IsAttachedWheelGrid())
				//return false;

			return false;
		}

		protected override void AfterDeletion(CubeGridDeletionContext context)
		{
			if (context.EntitiesForDeletion.Count == 0)
				return;

			//ShowMessageFromServer("Deleted {0} grid(s) that had fewer than {1} blocks, no owner and no players within {2} m: {3}.",
				//context.EntitiesForDeletion.Count, BlockCountThreshold, context.PlayerDistanceThreshold, string.Join(", ", context.EntitiesForDeletionNames));
		}

        //public int BlockCountThreshold { get; private set; }
        public static void Talk(String who, String what)
        {
            if (!Log)
                return;
            PDLLogger.WriteLine(who + " :: " + what);
            MyAPIGateway.Utilities.ShowMessage(who, what);
        }


    }


}
