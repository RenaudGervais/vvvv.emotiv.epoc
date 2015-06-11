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
    [PluginInfo(Name = "Cognitiv",
                Category = "EmoState",
                Help = "Exposes the Cognitiv properties of an EmoState, i.e. mental imagery",
                Tags = "Emotiv, Epoc, Cognitiv, EmoState")]
    #endregion PluginInfo
    public class CognitivEmoStateNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("EmoState")]
        public IDiffSpread<EmoState> FEmoState;
        #endregion fields & pins


        #region vars
        private EdkDll.EE_CognitivAction_t mCogAction = EdkDll.EE_CognitivAction_t.COG_NEUTRAL;
        private Single mPower = 0;
        private Boolean mIsActive = false;
        #endregion vars

        public void Evaluate(int SpreadMax)
        {

        }
    }
}
