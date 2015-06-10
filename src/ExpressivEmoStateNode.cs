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

        #region vars
        //Synchronization semaphor
        private static object syncLock = new Object();

        private Boolean mIsBlink;
        private Boolean mIsLeftWink;
        private Boolean mIsRightWink;
        private Boolean mIsEyesOpen;
        private Boolean mIsLookingUp;
        private Boolean mIsLookingDown;
        private Boolean mIsLookingLeft;
        private Boolean mIsLookingRight;
        private Single mLeftEye;
        private Single mRightEye;
        private Single mX;
        private Single mY;
        private Single mEyebrowExtent;
        private Single mSmileExtent;
        private Single mClenchExtent;
        private EdkDll.EE_ExpressivAlgo_t mUpperFaceAction;
        private Single mUpperFacePower;
        private EdkDll.EE_ExpressivAlgo_t mLowerFaceAction;
        private Single mLowerFacePower;

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
