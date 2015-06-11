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

        [Output("Long Term Excitement Score")]
        public ISpread<Single> FLTExcScore;

        [Output("Short Term Excitement Score")]
        public ISpread<Single> FSTExcScore;

        [Output("Short Term Excitement Model Params")]
        public ISpread<double> FSTExcModelParams;

        [Output("Short Term Excitement Scaled Score")]
        public ISpread<double> FSTExcScaled;

        [Output("Meditation Score")]
        public ISpread<Single> FMeditationScore;

        [Output("Meditation Model Params")]
        public ISpread<double> FMeditationModelParams;

        [Output("Meditation Scaled Score")]
        public ISpread<double> FMeditationScaled;

        [Output("Frustration Score")]
        public ISpread<Single> FFrustrationScore;

        [Output("Frustration Model Params")]
        public ISpread<double> FFrustrationModelParams;

        [Output("Frustration Scaled Score")]
        public ISpread<double> FFrustrationScaled;

        [Output("Boredom Score")]
        public ISpread<Single> FBoredomScore;

        [Output("Boredom Model Params")]
        public ISpread<double> FBoredomModelParams;

        [Output("Boredom Scaled Score")]
        public ISpread<double> FBoredomScaled;

        [Output("Affectiv Algo List")]
        public ISpread<EdkDll.EE_AffectivAlgo_t> FAffAlgoList;

        [Output("Affectiv Active Algo", IsToggle = true)]
        public ISpread<bool> FAffActiveAlgo;
        #endregion fields & pins


        #region vars
        private static EdkDll.EE_AffectivAlgo_t[] mAffAlgoList = { 
                                                      EdkDll.EE_AffectivAlgo_t.AFF_ENGAGEMENT_BOREDOM,
                                                      EdkDll.EE_AffectivAlgo_t.AFF_EXCITEMENT,
                                                      EdkDll.EE_AffectivAlgo_t.AFF_FRUSTRATION,
                                                      EdkDll.EE_AffectivAlgo_t.AFF_MEDITATION,
                                                      };
        private Boolean[] mIsAffActiveList = new Boolean[mAffAlgoList.Length];
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
            if (FEmoState[0] != null)
            {
                EmoState es = FEmoState[0];

                Single timeFromStart = es.GetTimeFromStart();

                mLongTermExcitementScore = es.AffectivGetExcitementLongTermScore();
                mShortTermExcitementScore = es.AffectivGetExcitementShortTermScore();
                for (int i = 0; i < mAffAlgoList.Length; ++i)
                {
                    mIsAffActiveList[i] = es.AffectivIsActive(mAffAlgoList[i]);
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
        }


        //Processing loop
        public void Evaluate(int SpreadMax)
        {
            if (FEmoState.IsChanged && FEmoState.SliceCount > 0)
                AffectivEmoStateUpdated();

            //Output data to pins 
            //  Long and Short term excitement scores
            FLTExcScore.SliceCount = 1;
            FLTExcScore[0] = mLongTermExcitementScore;

            FSTExcScore.SliceCount = 1;
            FSTExcScore[0] = mShortTermExcitementScore;

            FSTExcModelParams.SliceCount = 3;
            FSTExcModelParams[0] = mRawScoreEc;
            FSTExcModelParams[1] = mMinScaleEc;
            FSTExcModelParams[2] = mMaxScaleEc;

            FSTExcScaled.SliceCount = 1;
            FSTExcScaled[0] = mScaledScoreEc;

            //  Meditation scores
            FMeditationScore.SliceCount = 1;
            FMeditationScore[0] = mMeditationScore;

            FMeditationModelParams.SliceCount = 3;
            FMeditationModelParams[0] = mRawScoreMd;
            FMeditationModelParams[1] = mMinScaleMd;
            FMeditationModelParams[2] = mMaxScaleMd;

            FMeditationScaled.SliceCount = 1;
            FMeditationScaled[0] = mScaledScoreMd;

            //  Frustration scores
            FFrustrationScore.SliceCount = 1;
            FFrustrationScore[0] = mFrustrationScore;

            FFrustrationModelParams.SliceCount = 3;
            FFrustrationModelParams[0] = mRawScoreFt;
            FFrustrationModelParams[1] = mMinScaleFt;
            FFrustrationModelParams[2] = mMaxScaleFt;

            FFrustrationScaled.SliceCount = 1;
            FFrustrationScaled[0] = mScaledScoreFt;

            //  Boredom scores
            FBoredomScore.SliceCount = 1;
            FBoredomScore[0] = mBoredomScore;

            FBoredomModelParams.SliceCount = 3;
            FBoredomModelParams[0] = mRawScoreEg;
            FBoredomModelParams[1] = mMinScaleEg;
            FBoredomModelParams[2] = mMaxScaleEg;

            FBoredomScaled.SliceCount = 1;
            FBoredomScaled[0] = mScaledScoreEg;

            //  List of active Affectiv algo
            FAffAlgoList.SliceCount = mAffAlgoList.Length;
            for (int i = 0; i < mAffAlgoList.Length; ++i)
                FAffAlgoList[i] = mAffAlgoList[i];

            FAffActiveAlgo.SliceCount = mIsAffActiveList.Length;
            for (int i = 0; i < mIsAffActiveList.Length; ++i)
                FAffActiveAlgo[i] = mIsAffActiveList[i];
        }
    }
}
