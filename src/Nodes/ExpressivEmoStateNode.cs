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
    public class ExpressivEmoStateNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins
        [Input("EmoState")]
        public IDiffSpread<EmoState> FEmoState;

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

        [Output("Expressiv Algo List")]
        public ISpread<EdkDll.EE_ExpressivAlgo_t> FExpAlgoList;

        [Output("Expressiv Active Algo", IsToggle = true)]
        public ISpread<bool> FExpActiveAlgo;



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
        private Single mLeftEyelid = 0;
        private Single mRightEyelid = 0;
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

        }

        public void OnImportsSatisfied()
        {

        }


        //Event handler for Expressiv event
        void ExpressivEmoStateUpdated()
        {
            if (FEmoState[0] != null)
            {
                EmoState es = FEmoState[0];

                mIsBlink = es.ExpressivIsBlink();
                mIsLeftWink = es.ExpressivIsLeftWink();
                mIsRightWink = es.ExpressivIsRightWink();
                mIsEyesOpen = es.ExpressivIsEyesOpen();
                mIsLookingUp = es.ExpressivIsLookingUp();
                mIsLookingDown = es.ExpressivIsLookingDown();
                mIsLookingLeft = es.ExpressivIsLookingLeft();
                mIsLookingRight = es.ExpressivIsLookingRight();
                mLeftEyelid = 0.0F;
                mRightEyelid = 0.0F;
                mX = 0.0F;
                mY = 0.0F;
                es.ExpressivGetEyelidState(out mLeftEyelid, out mRightEyelid);
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
            if (FEmoState.IsChanged && FEmoState.SliceCount > 0)
                ExpressivEmoStateUpdated();

            //Output data to pins
            FIsBlink.SliceCount = 1;
            FIsBlink[0] = mIsBlink;

            FIsLeftWink.SliceCount = 1;
            FIsLeftWink[0] = mIsLeftWink;

            FIsRightWink.SliceCount = 1;
            FIsRightWink[0] = mIsRightWink;

            FIsEyesOpen.SliceCount = 1;
            FIsEyesOpen[0] = mIsEyesOpen;

            FIsLookingUp.SliceCount = 1;
            FIsLookingUp[0] = mIsLookingUp;

            FIsLookingDown.SliceCount = 1;
            FIsLookingDown[0] = mIsLookingDown;

            FIsLookingLeft.SliceCount = 1;
            FIsLookingLeft[0] = mIsLookingLeft;

            FIsLookingRight.SliceCount = 1;
            FIsLookingRight[0] = mIsLookingRight;

            FEyeLidState.SliceCount = 2;
            FEyeLidState[0] = mLeftEyelid;
            FEyeLidState[1] = mRightEyelid;

            FEyeLocation.SliceCount = 2;
            FEyeLocation[0] = mX;
            FEyeLocation[1] = mY;

            FEyebrowExtent.SliceCount = 1;
            FEyebrowExtent[0] = mEyebrowExtent;

            FSmileExtent.SliceCount = 1;
            FSmileExtent[0] = mSmileExtent;

            FClenchExtent.SliceCount = 1;
            FClenchExtent[0] = mClenchExtent;

            FUpperFaceAction.SliceCount = 1;
            FUpperFaceAction[0] = mUpperFaceAction;

            FUpperFacePower.SliceCount = 1;
            FUpperFacePower[0] = mUpperFacePower;

            FLowerFaceAction.SliceCount = 1;
            FLowerFaceAction[0] = mLowerFaceAction;

            FLowerFacePower.SliceCount = 1;
            FLowerFacePower[0] = mLowerFacePower;

            FExpAlgoList.SliceCount = mExpAlgoList.Length;
            for (int i = 0; i < mExpAlgoList.Length; ++i)
                FExpAlgoList[i] = mExpAlgoList[i];

            FExpActiveAlgo.SliceCount = mIsExpActiveList.Length;
            for (int i = 0; i < mIsExpActiveList.Length; ++i)
                FExpActiveAlgo[i] = mIsExpActiveList[i];
        }
    }
}
