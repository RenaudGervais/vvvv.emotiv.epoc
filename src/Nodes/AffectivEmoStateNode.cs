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
    [PluginInfo(Name = "Affectiv",
                Category = "EmoState",
                Help = "Exposes the Affectiv properties of an EmoState, i.e. Excitement, Engagement, etc",
                Tags = "Emotiv, Epoc, Affectiv, EmoState")]
    #endregion PluginInfo
    class AffectivEmoStateNode : IPluginEvaluate
    {
        //Processing loop
        public void Evaluate(int SpreadMax)
        {

        }
    }
}
