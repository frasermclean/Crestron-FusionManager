using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;

namespace FM.Fusion
{
    public class FusionManager
    {
        #region Class variables
        CrestronControlSystem cs;
        FusionRoom room;
        #endregion

        #region Constants
        const int SensorAssetNumber = 1;
        #endregion

        #region Constructor
        public FusionManager(CrestronControlSystem cs, uint ipid, string roomName)
        {
            try
            {
                this.cs = cs;

                Trace("Constructor running.");

                // create new FusionRoom object
                string roomID = new Guid().ToString();
                room = new FusionRoom(ipid, cs, roomName, roomID);

                // add assets
                AddAssets();

                // add event handlers
                room.OnlineStatusChange += new OnlineStatusChangeEventHandler(OnlineStatusChange);
                room.FusionStateChange += new FusionStateEventHandler(FusionStateChange);
                room.FusionAssetStateChange += new FusionAssetStateEventHandler(AssetStateChange);

                // generate rvi file
                FusionRVI.GenerateFileForAllFusionDevices();

                // attempt to register
                eDeviceRegistrationUnRegistrationResponse response = room.Register();
                if (response == eDeviceRegistrationUnRegistrationResponse.Success)
                    Trace("Room registered successfully.");
                else
                    Trace("Room failed to register: " + response);
            }
            catch (Exception e)
            {
                string traceMessage = "Exception caught in constructor: " + e.Message + e.StackTrace;
                Trace(traceMessage);
                ErrorLog.Error(traceMessage);
            }
        }
        #endregion

        #region Properties
        public bool TraceEnabled { get; set; }
        public string TraceName { get; set; }
        public bool Online
        {
            get { return room.IsOnline; }
        }
        #endregion

        #region Public methods
        public void SetRoomOccupancy(bool value)
        {
            try
            {
                Trace("SetRoomOccupancy() setting occupancy to: " + value);

                FusionOccupancySensor sensorAsset = (FusionOccupancySensor)room.UserConfigurableAssetDetails[SensorAssetNumber].Asset;

                if (sensorAsset != null)                    
                    sensorAsset.RoomOccupied.InputSig.BoolValue = value;
                else
                    Trace("SetRoomOccupancy() Sensor asset is null.");
            }
            catch (Exception e)
            {
                Trace("SetRoomOccupancy() exception caught: " + e.Message);
            }
        }
        #endregion

        #region Private methods
        void Trace(string message)
        {
            if (TraceEnabled)
            {
                string line = String.Format("[{0} {1}] {2}", cs.ProgramNumber, this.GetType().Name, message.Trim());
                CrestronConsole.PrintLine(line);
            }
        }
        bool AddAssets()
        {
            try
            {
                // add sensor
                string sensorID = new Guid().ToString();
                room.AddAsset(eAssetType.OccupancySensor, SensorAssetNumber, "Occupancy Sensor", "Occupancy Sensor", sensorID);                

                Trace("AddAssets() added all assets successfully.");
                return true;
            }
            catch (Exception e)
            {
                Trace("AddAssets() exception caught: " + e.Message);
                return false;
            }
        }
        #endregion

        #region Event handlers
        void OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            Trace("OnlineStatusChange() online: " + args.DeviceOnLine);
        }
        void FusionStateChange(FusionBase device, FusionStateEventArgs args)
        {
            switch (args.EventId)
            {
                case FusionEventIds.SystemPowerOnReceivedEventId:
                    Trace("FusionStateChange() system power on event received.");
                    break;
                case FusionEventIds.SystemPowerOffReceivedEventId:
                    Trace("FusionStateChange() system power off event received.");
                    break;
                default:
                    Trace("FusionStateChange() unhandled event received: " + args.EventId);
                    break;
            }
        }
        void AssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
        {
            Trace("AssetStateChange() event ID: " + args.EventId);
        }
        #endregion
    }
}