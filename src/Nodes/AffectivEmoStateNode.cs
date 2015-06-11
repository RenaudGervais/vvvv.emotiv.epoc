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
    public class AffectivEmoStateNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("EmoState")]
        public IDiffSpread<EmoState> FEmoState;
        #endregion fields & pins


        #region vars
        private EdkDll.EE_AffectivAlgo_t[] mAffAlgoList = { 
                                                      EdkDll.EE_AffectivAlgo_t.AFF_ENGAGEMENT_BOREDOM,
                                                      EdkDll.EE_AffectivAlgo_t.AFF_EXCITEMENT,
                                                      EdkDll.EE_AffectivAlgo_t.AFF_FRUSTRATION,
                                                      EdkDll.EE_AffectivAlgo_t.AFF_MEDITATION,
                                                      };
        private Single mLongTermExcitementScore = 0;
        private Single mShortTermExcitementScore = 0;
        private Single mMeditationScore = 0;
        private Single mFrustrationScore = 0;
        private Single mBoredomScore = 0;
        private double mRawScoreEc = 0, mRawScoreMd = 0, mRawScoreFt = 0, mRawScoreEg = 0;
        private double mMinScaleEc = 0, mMinScaleMd = 0, mMinScaleFt = 0, mMinScaleEg = 0;
        private double mMaxScaleEc = 0, mMaxScaleMd = 0, mMaxScaleFt = 0, mMaxScaleEg = 0;
        private double mScaledScoreEc = 0, mScaledScoreMd = 0, mScaledScoreFt = 0, mScaledScoreEg = 0;
        #endregion vars

        //Update data
        void AffectivEmoStateUpdated()
        {
            EmoState es = FEmoState[0];

            Single timeFromStart = es.GetTimeFromStart();

            Boolean[] isAffActiveList = new Boolean[mAffAlgoList.Length];

            mLongTermExcitementScore = es.AffectivGetExcitementLongTermScore();
            mShortTermExcitementScore = es.AffectivGetExcitementShortTermScore();
            for (int i = 0; i < mAffAlgoList.Length; ++i)
            {
                isAffActiveList[i] = es.AffectivIsActive(mAffAlgoList[i]);
            }

            mMeditationScore = es.AffectivGetMeditationScore();
            mFrustrationScore = es.AffectivGetFrustrationScore();
            mBoredomScore = es.AffectivGetEngagementBoredomScore();

            es.AffectivGetExcitementShortTermModelParams(out mRawScoreEc, out mMinScaleEc, out mMaxScaleEc);
            if (mMinScaleEc != mMaxScaleEc)
            {
                if (mRawScoreEc < mMinScaleEc)
                {
                    mScaledScoreEc = 0;
                }
                else if (mRawScoreEc > mMaxScaleEc)
                {
                    mScaledScoreEc = 1;
                }
                else
                {
                    mScaledScoreEc = (mRawScoreEc - mMinScaleEc) / (mMaxScaleEc - mMinScaleEc);
                }
            }

            es.AffectivGetEngagementBoredomModelParams(out mRawScoreEg, out mMinScaleEg, out mMaxScaleEg);
            if (mMinScaleEg != mMaxScaleEg)
            {
                if (mRawScoreEg < mMinScaleEg)
                {
                    mScaledScoreEg = 0;
                }
                else if (mRawScoreEg > mMaxScaleEg)
                {
                    mScaledScoreEg = 1;
                }
                else
                {
                    mScaledScoreEg = (mRawScoreEg - mMinScaleEg) / (mMaxScaleEg - mMinScaleEg);
                }
            }
            es.AffectivGetMeditationModelParams(out mRawScoreMd, out mMinScaleMd, out mMaxScaleMd);
            if (mMinScaleMd != mMaxScaleMd)
            {
                if (mRawScoreMd < mMinScaleMd)
                {
                    mScaledScoreMd = 0;
                }
                else if (mRawScoreMd > mMaxScaleMd)
                {
                    mScaledScoreMd = 1;
                }
                else
                {
                    mScaledScoreMd = (mRawScoreMd - mMinScaleMd) / (mMaxScaleMd - mMinScaleMd);
                }
            }
            es.AffectivGetFrustrationModelParams(out mRawScoreFt, out mMinScaleFt, out mMaxScaleFt);
            if (mMaxScaleFt != mMinScaleFt)
            {
                if (mRawScoreFt < mMinScaleFt)
                {
                    mScaledScoreFt = 0;
                }
                else if (mRawScoreFt > mMaxScaleFt)
                {
                    mScaledScoreFt = 1;
                }
                else
                {
                    mScaledScoreFt = (mRawScoreFt - mMinScaleFt) / (mMaxScaleFt - mMinScaleFt);
                }
            }
        }


        //Processing loop
        public void Evaluate(int SpreadMax)
        {
            if (FEmoState.IsChanged && FEmoState.SliceCount > 0)
                AffectivEmoStateUpdated();

            //Output data to pins
        }
    }
}
