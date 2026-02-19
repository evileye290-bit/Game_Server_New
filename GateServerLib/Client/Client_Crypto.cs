using Message.Client.Protocol.CGate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoUtility;
using System.Security.Cryptography;
using Message.Gate.Protocol.GateC;
using CommonUtility;

namespace GateServerLib
{
    public partial class Client
    {
        public static bool cryptoOpen = false;

        private static String privateKey;

        public static String PrivateKey
        {
            get { return Client.privateKey; }
            set { Client.privateKey = value; }
        }


        private static RNGCryptoServiceProvider keyProvider=new RNGCryptoServiceProvider();

        public RNGCryptoServiceProvider KeyProvider
        {
            get { return keyProvider; }
            set { keyProvider = value; }
        }

        static Client()
        {
            try
            {
                string filePath = Path.Combine(PathExt.FullPathFromServer(""), "PrivateKey.key");
                FileStream fsRead = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fsRead);
                privateKey = sr.ReadLine();

                sr.Close();
                fsRead.Close();
                Logger.Log.Write("PrivateKey Of Client Class set ok");

            }
            catch(Exception e)
            {
                Logger.Log.Write("PrivateKey Of Client Class set error:"+e);
            }
            
        }

        private string blowFishKey;

        public string BlowFishKey
        {
            get { return blowFishKey; }
             set { blowFishKey = value; }
        }

        private BlowFish myBlowfish;

        public BlowFish MyBlowfish
        {
            get { return myBlowfish; }
            set { myBlowfish = value; }
        }

        public void OnResponse_GetBlowFishKey(MemoryStream stream)
        {
            byte[] cipherKey=new byte[8];
            KeyProvider.GetBytes(cipherKey);
            BlowFishKey = BitConverter.ToString(cipherKey).Replace("-", "");
            MyBlowfish = new BlowFish(BlowFishKey);

            MSG_GC_BLOWFISHKEY msg = new MSG_GC_BLOWFISHKEY();

            string encryptData = RSAHelper.EncryptString(BlowFishKey, PrivateKey);
            msg.BlowfishKey = encryptData;
            Write(msg);

        }

    }
}
