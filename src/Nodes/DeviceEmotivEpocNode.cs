#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Emotiv;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.EmotivEpoc
{
	#region PluginInfo
	[PluginInfo(Name = "EmotivEpoc",
				Category = "Device",
				Help = "Get information from Emotiv Epoc device",
				Tags = "Emotiv, Epoc, EEG",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class DeviceEmotivEpocNode : IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification
	{
		#region enums
		public enum TEmotivConnectionMode {
			EmoEngine,
			EmotivControlPanel,
			EmoComposer
		}
		#endregion enums
		
		
		#region fields & pins
		[Input("Connect", DefaultBoolean = false, IsToggle = true, IsSingle = true)]
		public IDiffSpread<bool> FConnect;
		
		[Input("Server", DefaultString = "127.0.0.1", IsSingle = true)]
		public IDiffSpread<string> FServer;
		
		[Input("Connection Mode", IsSingle = true, DefaultEnumEntry = "EmoEngine")]
		public IDiffSpread<TEmotivConnectionMode> ConnectionMode;

        [Output("Device", IsSingle = true)]
        public ISpread<EmoEngine> FEmoEngine;

        [Output("Headset On", IsToggle = true)]
        public ISpread<bool> FHeadsetOn;

        [Output("Contact Quality")]
        public ISpread<EdkDll.EE_EEG_ContactQuality_t> FCQ;

        [Output("Signal Strength")]
        public ISpread<EdkDll.EE_SignalStrength_t> FSignalStrength;

        [Output("Battery Level")]
        public ISpread<Int32> FBatteryCharge;

        [Output("Battery Max Level")]
        public ISpread<Int32> FBatteryMaxCharge;

		[Output("Connected", IsToggle = true, IsSingle = true)]
		public ISpread<bool> FConnected;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		private EmoEngine mEngine;
		private bool mIsConnected = false;
//		private ISpread<double>
		
		//Contructor
		public DeviceEmotivEpocNode() {
			mEngine = EmoEngine.Instance;
			
			//Register event handler
            mEngine.EmoEngineConnected += new EmoEngine.EmoEngineConnectedEventHandler(EmoEngineConnectedCB);
            mEngine.EmoEngineDisconnected += new EmoEngine.EmoEngineDisconnectedEventHandler(EmoEngineDisconnectedCB);
		}
		
		
		//I/O pin that needs to be setup only once after constructor
		public void OnImportsSatisfied() {
            ////Set the legends as they won't change dynamically
            //ExpressivLegend();
            //AffectivLegend();
		}
		
		
		//Destructor
		public void Dispose() {
			mEngine.Disconnect();
		}
		
		
		//Create the connection
		protected void Connect(TEmotivConnectionMode iMode, string iServer) {
			switch(iMode) {
				case TEmotivConnectionMode.EmoEngine:
				mEngine.Connect();
				break;
				
				case TEmotivConnectionMode.EmotivControlPanel:
				mEngine.RemoteConnect(iServer, 3008);
				break;
				
				case TEmotivConnectionMode.EmoComposer:
				FLogger.Log(LogType.Debug, "Connecting...");
				mEngine.RemoteConnect(iServer, 1726);
				break;
			}
		}


        //Handling internal connection status
        protected void EmoEngineConnectedCB(object sender, EmoEngineEventArgs e)
        {
            mIsConnected = true;
			FLogger.Log(LogType.Debug, "Connected!");
        }

        protected void EmoEngineDisconnectedCB(object sender, EmoEngineEventArgs e)
        {
            mIsConnected = false;
            FLogger.Log(LogType.Debug, "Disconnected!");
        }

        protected void EmoEngineEmoStateUpdatedCB(object sender, EmoEngineEventArgs e)
        {
            //Update connexion status
        }

		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FConnect.IsChanged) {
				if(FConnect[0] && !mIsConnected)
					Connect(ConnectionMode[0], FServer[0]);
				else {
					mEngine.Disconnect();
				}
			}
			
			if((FServer.IsChanged || ConnectionMode.IsChanged) && mIsConnected) {
				mEngine.Disconnect();
				Connect(ConnectionMode[0], FServer[0]);
			}
			
			if(mIsConnected) {
				//Process events
				mEngine.ProcessEvents(1000);
			}

            //Fill the output pins
            FEmoEngine.SliceCount = 1;
            FEmoEngine[0] = mEngine;
			
			FConnected.SliceCount = 1;
			FConnected[0] = mIsConnected;
		}
		
		
        ////Expressiv legend values
        //protected void ExpressivLegend() {
        //    FExpressivLegend.SliceCount = 10;
        //    FExpressivLegend[0] = "Blink";
        //    FExpressivLegend[1] = "Right Wink";
        //    FExpressivLegend[2] = "Left Wink";
        //    FExpressivLegend[3] = "Eyelid Right";
        //    FExpressivLegend[4] = "Eyelid Left";
        //    FExpressivLegend[5] = "Eyes Pos X";
        //    FExpressivLegend[6] = "Eyes Pos Y";
        //    FExpressivLegend[7] = "Eyebrow Extent";
        //    FExpressivLegend[8] = "Smile";
        //    FExpressivLegend[9] = "Clench";
        //}
		
		
        ////Affectiv legend values
        //protected void AffectivLegend() {
        //    FAffectivLegend.SliceCount = 5;
        //    FAffectivLegend[0] = "Engagement/Boredom";
        //    FAffectivLegend[1] = "Frustration";
        //    FAffectivLegend[2] = "Meditation";
        //    FAffectivLegend[3] = "Instantaneous Excitement";
        //    FAffectivLegend[4] = "Long Term Excitement";
        //}
	}
}
