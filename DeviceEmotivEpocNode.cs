#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Emotiv;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "EmotivEpoc", Category = "Device", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class DeviceEmotivEpocNode : IPluginEvaluate, IDisposable
	{
		#region fields & pins
		[Input("Enabled", DefaultBoolean = false, IsToggle = true, IsSingle = true)]
		public ISpread<bool> FEnabled;

		[Output("Output")]
		public ISpread<double> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		private EmoEngine mEngine;
		private bool mIsConnected = false;
		
		//Contructor
		public DeviceEmotivEpocNode() {
			mEngine = EmoEngine.Instance;
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FEnabled[0]) {
				if(!mIsConnected) {
					//Use Connect() to connect to EmoEngine
//					mEngine.Connect();
					//Use RemoteConnect() to connect to Control Panel (port 3008) or EmoComposer (port 1726)
					mEngine.RemoteConnect("127.0.0.1", 1726);
					mIsConnected = true;
				}
			
			FOutput.SliceCount = SpreadMax;

//			for (int i = 0; i < SpreadMax; i++)
//				FOutput[i] = FInput[i] * 2;

			//FLogger.Log(LogType.Debug, "hi tty!");
			} else {
				mEngine.Disconnect();
				mIsConnected = false;
			}
		}
		
		//Destructor
		public void Dispose() {
			mEngine.Disconnect();
		}
	}
}
