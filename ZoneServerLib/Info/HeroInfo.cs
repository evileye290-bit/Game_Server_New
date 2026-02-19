using CommonUtility;
using EnumerateUtility;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerModels
{
    public static class HeroInfoExtendClass
    {
        //private static List<NatureType> types = new List<NatureType>() { NatureType.PRO_POW, NatureType.PRO_CON, NatureType.PRO_EXP, NatureType.PRO_AGI };
        //public static Natures Calc4to9Natures(this HeroInfo info)
        //{
        //    Natures natures = info.Nature.Clone();

        //    int gC = info.GetGroVal();
        //    if (gC > 0)
        //    {
        //        //5->9
        //        natures.SetNatureBaseValue(NatureType.PRO_GRO_VAL, gC);
        //    }

        //    foreach (var nature4 in NatureLibrary.GetNature4To9List())
        //    {
        //        int nature4Value = natures.GetNatureValue(nature4.Key);
        //        foreach (var nature9 in nature4.Value)
        //        {
        //            int newValue = (int)(nature9.Value * gC * nature4Value);
        //            natures.AddNatureAddedValue(nature9.Key, newValue);
        //        }
        //    }

        //    //foreach (NatureType type in types)
        //    //{
        //    //    UpdateNature4to9(natures, type, 0, natures.GetNatureValue(type), info);
        //    //}
        //    return natures;
        //}

        //public static float GetGroVal(this HeroInfo heroInfo)
        //{
        //    float groC = 0;
        //    //成长值
        //    GroValFactorModel groValFactorModel = NatureLibrary.GetGroValFactorModel(heroInfo.AwakenLevel);
        //    if (groValFactorModel != null)
        //    {
        //        if (heroInfo.IsPlayer())
        //        {
        //            //成长值
        //            groC = groValFactorModel.PlayerAwakenC;
        //        }
        //        else
        //        {
        //            groC = groValFactorModel.AwakenC;
        //        }
        //    }
        //    return groC;
        //}

        ////天赋属性转基础属性
        //private static void UpdateNature4to9(Natures natures, NatureType type, int oldValue, int newValue, HeroInfo heroInfo)//天赋点数
        //{
        //    //List<NatureType> nature9List = NatureLibrary.GetNature9List(type);
        //    //if (nature9List == null)
        //    //{
        //    //    return;
        //    //}
        //    //int gC = heroInfo.GetGroVal();
        //    //if (gC > 0)
        //    //{
        //    //    //5->9
        //    //    natures.SetNatureValue(NatureType.PRO_GRO_VAL, gC);
        //    //}

        //    //float sC = 1;
        //    //GroValFactorModel stepsModel = NatureLibrary.GetGroValFactorModel(heroInfo.StepsLevel);
        //    //if (stepsModel != null)
        //    //{
        //    //    sC = stepsModel.StepsC;
        //    //}

        //    //foreach (var item in nature9List)//item:atk,hit
        //    //{
        //    //    // old 100
        //    //    int oldNature = natures.CalcNature4to9(item, oldValue, heroInfo, gC);
        //    //    // new 350
        //    //    int newNature = natures.CalcNature4to9(item, newValue, heroInfo, gC);
        //    //    natures.AddNatureValue(item, newNature - oldNature);
        //    //}
        //}

        //计算Y值
        //private static int CalcNature4to9(this Natures natures, NatureType type, int value, HeroInfo heroInfo, int gC )
        //{
        //    //if (gC == 0)
        //    //{
        //    //    gC = heroInfo.GetGroVal();
        //    //    if (gC > 0)
        //    //    {
        //    //        //5->9
        //    //        natures.SetNatureValue(NatureType.PRO_GRO_VAL, gC);
        //    //    }
        //    //}
        //    //CommonNatureParamModel commonNatureParamModel = NatureLibrary.GetCommonNatureParam();
        //    //BasicNatureFactorModel basicNatureFactorModel = NatureLibrary.GetBasicNatureFactor(type);
        //    //if (commonNatureParamModel == null || basicNatureFactorModel == null)
        //    //{
        //    //    return -1;
        //    //}
        //    //float basic = basicNatureFactorModel.B;
        //    //float a = basicNatureFactorModel.A;
        //    //int lMax = commonNatureParamModel.LMax;
        //    //int t = basicNatureFactorModel.T;
        //    //float pF = commonNatureParamModel.Pf;
        //    //int f = value;
        //    //int fMax = commonNatureParamModel.FMax;
        //    //float b = commonNatureParamModel.B;
        //    //float k = basicNatureFactorModel.K;
        //    //int cMax = commonNatureParamModel.CMax;
        //    ////Y = (B * Lmax ^ a + T) * Pf * (F / Fmax) ^ b * K * C / Cmax
        //    //float y = (basic * (float)Math.Pow(lMax, a) + t) * pF * (float)Math.Pow((f * 1.0) / (fMax * 1.0), b) * k * (float)((gC * 1.0) / (cMax * 1.0));

        //    //switch (switch_on)
        //    //{
        //    //    default:
        //    //}



        //    int natureValue = (int)(y + 0.5);
        //    return natureValue;
        //}
    }
}
