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

        [Output("EmoState", IsSingle = true)]
        public ISpread<EmoState> FEmoState;

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

        [Output("Time From Start")]
        public ISpread<Single> FTimeFromStart;

		[Output("Connected", IsToggle = true, IsSingle = true)]
		public ISpread<bool> FConnected;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		//Member variables
        private EmoEngine mEngine;
        private EmoState mEmoState;
        private bool mIsHeadsetOn = false;
        private EdkDll.EE_EEG_ContactQuality_t[] mCQ = new EdkDll.EE_EEG_ContactQuality_t[] { EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_NO_SIGNAL };
        private EdkDll.EE_SignalStrength_t mSignalStrength;
        private Int32 mBatteryCharge = 0;
        private Int32 mBatteryMaxCharge = 0;
        private Single mTimeFromStart = 0;
		private bool mIsConnected = false;

        private static object syncLock = new Object();
		
		//Contructor
		public DeviceEmotivEpocNode() {
			mEngine = EmoEngine.Instance;
			
			//Register event handler
            mEngine.EmoEngineConnected += new EmoEngine.EmoEngineConnectedEventHandler(EmoEngineConnectedCB);
            mEngine.EmoEngineDisconnected += new EmoEngine.EmoEngineDisconnectedEventHandler(EmoEngineDisconnectedCB);
            mEngine.EmoEngineEmoStateUpdated += new EmoEngine.EmoEngineEmoStateUpdatedEventHandler(EmoEngineEmoStateUpdatedCB);
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
				mEngine.RemoteConnect(iServer, 1726);
				break;
			}
		}


        //Handling internal connection status
        protected void EmoEngineConnectedCB(object sender, EmoEngineEventArgs e)
        {
            mIsConnected = true;
        }

        protected void EmoEngineDisconnectedCB(object sender, EmoEngineEventArgs e)
        {
            mIsConnected = false;
            
            //Reset device status
            lock(syncLock)
            {
                mIsHeadsetOn = false;
                mCQ = new EdkDll.EE_EEG_ContactQuality_t[] {EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_NO_SIGNAL};
                mSignalStrength = EdkDll.EE_SignalStrength_t.NO_SIGNAL;
                mBatteryCharge = 0;
                mBatteryMaxCharge = 0;
                mTimeFromStart = 0;
            }
        }

        protected void EmoEngineEmoStateUpdatedCB(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;

            //Update connexion status
            lock(syncLock)
            {
                mEmoState = es;
                mIsHeadsetOn = es.GetHeadsetOn() != 0;
                mCQ = es.GetContactQualityFromAllChannels();
                mSignalStrength = es.GetWirelessSignalStatus();
                es.GetBatteryChargeLevel(out mBatteryCharge, out mBatteryMaxCharge);
                mTimeFromStart = es.GetTimeFromStart();
            }
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
            if (mIsConnected)
            {
                lock (syncLock)
                {
                    if (mEmoState != null)
                    {
                        FEmoState.SliceCount = 1;
                        FEmoState[0] = mEmoState;
                    }
                    else
                        FEmoState.SliceCount = 0;
                    
                    FHeadsetOn.SliceCount = 1;
                    FHeadsetOn[0] = mIsHeadsetOn;
                    
                    FCQ.SliceCount = mCQ.Length;
                    for (int i = 0; i < mCQ.Length; ++i)
                        FCQ[i] = mCQ[i];
                    
                    FSignalStrength.SliceCount = 1;
                    FSignalStrength[0] = mSignalStrength;
                    
                    FBatteryCharge.SliceCount = 1;
                    FBatteryCharge[0] = mBatteryCharge;
                    
                    FBatteryMaxCharge.SliceCount = 1;
                    FBatteryMaxCharge[0] = mBatteryMaxCharge;

                    FTimeFromStart.SliceCount = 1;
                    FTimeFromStart[0] = mTimeFromStart;
                }
            }
			
			FConnected.SliceCount = 1;
			FConnected[0] = mIsConnected;
		}
	}
}
