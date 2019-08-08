using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using FM.Fusion;

namespace TestSystem
{
    public class ControlSystem : CrestronControlSystem
    {
        #region System components
        FusionManager fusionManager;
        #endregion

        #region Class variables
        #endregion

        #region Constants
        const int FusionIpId = 0x05;
        #endregion

        #region Properties
        public bool TraceEnabled { get; set; }
        public string TraceName { get; set; }
        #endregion

        #region Constructor
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }
        public override void InitializeSystem()
        {
            try
            {
                // trace defaults
                this.TraceEnabled = true;
                this.TraceName = this.GetType().Name;

                // fusion manager
                fusionManager = new FusionManager(this, FusionIpId, "FusionManager Test Room");
                fusionManager.TraceEnabled = true;

                // add console commands
                CrestronConsole.AddNewConsoleCommand(ConsoleOccupancyHandler, "occ", "Occupancy set (true/false)", ConsoleAccessLevelEnum.AccessProgrammer);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error in InitializeSystem: {0}", e.Message);
            }
        }
        #endregion

        #region Private methods
        void Trace(string message)
        {
            if (TraceEnabled)
                CrestronConsole.PrintLine(String.Format("[{0}] {1}", TraceName, message.Trim()));
        }
        #endregion

        #region Console command handlers
        void ConsoleOccupancyHandler(string input)
        {
            bool value = Boolean.Parse(input);
            fusionManager.SetRoomOccupancy(value);
        }
        #endregion
    }
}