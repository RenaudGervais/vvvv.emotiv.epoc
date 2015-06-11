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
	[PluginInfo(Name = "Expressiv",
				Category = "EmoState",
				Help = "Exposes the Expressiv properties of an EmoState, i.e. facial expression",
				Tags = "Emotiv, Epoc, Expressiv, EmoState")]
	#endregion PluginInfo
	public class ExpressivEmoStateNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("EmoEngine", IsSingle = true)]
        public ISpread<EmoEngine> iEmoEngine;

        [Output("Blink", IsBang = true)]
        public ISpread<bool> FIsBlink;

        [Output("Left Wink", IsBang = true)]
        public ISpread<bool> FIsLeftWink;

        [Output("Right Wink", IsBang = true)]
        public ISpread<bool> FIsRightWink;

        [Output("Eyes Open", IsToggle = true)]
        public ISpread<bool> FIsEyesOpen;

        [Output("Looking Up", IsToggle = true)]
        public ISpread<bool> FIsLookingUp;

        [Output("Looking Down", IsToggle = true)]
        public ISpread<bool> FIsLookingDown;

        [Output("Looking Left", IsToggle = true)]
        public ISpread<bool> FIsLookingLeft;

        [Output("Looking Right", IsToggle = true)]
        public ISpread<bool> FIsLookingRight;

        [Output("Eyelid State LR")]
        public ISpread<Single> FEyeLidState;

        [Output("Eye Location XY")]
        public ISpread<Single> FEyeLocation;

        [Output("Eyebrow Extent")]
        public ISpread<Single> FEyebrowExtent;

        [Output("Smile Extent")]
        public ISpread<Single> FSmileExtent;

        [Output("Clench Extent")]
        public ISpread<Single> FClenchExtent;

        [Output("Upper Face Action")]
        public ISpread<EdkDll.EE_ExpressivAlgo_t> FUpperFaceAction;

        [Output("Upper Face Power")]
        public ISpread<Single> FUpperFacePower;

        [Output("Lower Face Action")]
        public ISpread<EdkDll.EE_ExpressivAlgo_t> FLowerFaceAction;

        [Output("Lower Face Power")]
        public ISpread<Single> FLowerFacePower;



        #endregion fields & pins

        #region vars
        //Synchronization semaphor
        private static object syncLock = new Object();

        private Boolean mIsBlink = false;
        private Boolean mIsLeftWink = false;
        private Boolean mIsRightWink = false;
        private Boolean mIsEyesOpen = false;
        private Boolean mIsLookingUp = false;
        private Boolean mIsLookingDown = false;
        private Boolean mIsLookingLeft = false;
        private Boolean mIsLookingRight = false;
        private Single mLeftEye = 0;
        private Single mRightEye = 0;
        private Single mX = 0;
        private Single mY = 0;
        private Single mEyebrowExtent = 0;
        private Single mSmileExtent = 0;
        private Single mClenchExtent = 0;
        private EdkDll.EE_ExpressivAlgo_t mUpperFaceAction = EdkDll.EE_ExpressivAlgo_t.EXP_NEUTRAL;
        private Single mUpperFacePower = 0;
        private EdkDll.EE_ExpressivAlgo_t mLowerFaceAction = EdkDll.EE_ExpressivAlgo_t.EXP_NEUTRAL;
        private Single mLowerFacePower = 0;

        private static EdkDll.EE_ExpressivAlgo_t[] mExpAlgoList = { 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_BLINK, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_CLENCH, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_EYEBROW, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_FURROW, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_HORIEYE, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_LAUGH, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_NEUTRAL, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_SMILE, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_LEFT, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_RIGHT, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_WINK_LEFT, 
                                                      EdkDll.EE_ExpressivAlgo_t.EXP_WINK_RIGHT
                                                      };
        private Boolean[] mIsExpActiveList = new Boolean[mExpAlgoList.Length];
        #endregion vars

        //Constructor
        public ExpressivEmoStateNode() 
        {
            //Register event handler
            iEmoEngine[0].ExpressivEmoStateUpdated +=
                new EmoEngine.ExpressivEmoStateUpdatedEventHandler(ExpressivEmoStateUpdated);
        }


        //Event handler for Expressiv event
        void ExpressivEmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;
            lock (syncLock)
            {
                mIsBlink = es.ExpressivIsBlink();
                mIsLeftWink = es.ExpressivIsLeftWink();
                mIsRightWink = es.ExpressivIsRightWink();
                mIsEyesOpen = es.ExpressivIsEyesOpen();
                mIsLookingUp = es.ExpressivIsLookingUp();
                mIsLookingDown = es.ExpressivIsLookingDown();
                mIsLookingLeft = es.ExpressivIsLookingLeft();
                mIsLookingRight = es.ExpressivIsLookingRight();
                mLeftEye = 0.0F;
                mRightEye = 0.0F;
                mX = 0.0F;
                mY = 0.0F;
                es.ExpressivGetEyelidState(out mLeftEye, out mRightEye);
                es.ExpressivGetEyeLocation(out mX, out mY);
                mEyebrowExtent = es.ExpressivGetEyebrowExtent();
                mSmileExtent = es.ExpressivGetSmileExtent();
                mClenchExtent = es.ExpressivGetClenchExtent();
                mUpperFaceAction = es.ExpressivGetUpperFaceAction();
                mUpperFacePower = es.ExpressivGetUpperFaceActionPower();
                mLowerFaceAction = es.ExpressivGetLowerFaceAction();
                mLowerFacePower = es.ExpressivGetLowerFaceActionPower();
                for (int i = 0; i < mExpAlgoList.Length; ++i)
                {
                    mIsExpActiveList[i] = es.ExpressivIsActive(mExpAlgoList[i]);
                }
            }
        }

        //Processing loop
        public void Evaluate(int SpreadMax)
        {
            lock(syncLock)
            { 
                //Output data to pins            
            }
        }
    }
}
