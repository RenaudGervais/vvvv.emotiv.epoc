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

namespace VVVV.EmotivEpoch
{
    #region PluginInfo
	[PluginInfo(Name = "Expressiv",
				Category = "Emotiv EmoState",
				Help = "Exposes the Expressiv properties of an EmoState (facial expression)",
				Tags = "Emotiv, Epoc, Expressiv, EmoState",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class ExpressivEmoStateNode : IPluginEvaluate, IDisposable
    {
        #region fields & pins
        [Input("EmoEngine", IsSingle = true)]
        public ISpread<Emotiv.EmoEngine> iEmoEngine;
        #endregion fields & pins

        //Constructor
        public ExpressivEmoStateNode() 
        {
            //Register event handler
            iEmoEngine[0].ExpressivEmoStateUpdated +=
                new EmoEngine.ExpressivEmoStateUpdatedEventHandler(ExpressivEmoStateUpdated);
        }


        //Event handler for Expressiv event
        static void ExpressivEmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {

        }

        //Processing loop
        public void Evaluate(int SpreadMax)
        {

        }
    }
}
