using KiteConnect;
using StockTradeConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock_Trade_Configuration.Singleton
{
    public class KiteInstance
    {
        // Initialize key and secret of your app
        string MyAPIKey = string.Empty;
        string MySecret = string.Empty;
        string MyUserId = string.Empty;
        public string RequestToken { get; set; }
        // persist these data in settings or db or file
        string MyPublicToken = "e7ost8r9p8l1c330";
        string MyAccessToken = "e7ost8r9p8l1c330";

        private static KiteInstance _instance = new KiteInstance();
        public KiteConnect.Kite Kite { get; set; }
        public static KiteInstance Instance
        {
            get { return _instance; }
        }

        private KiteInstance()
        {
        }

        public void SetInfo(UserInfo userInfo)
        {
            MyAPIKey = userInfo.APIKey;

            MySecret = userInfo.SecretKey;
            MyUserId = userInfo.UserId;
        }

        public Kite GetKite(UserInfo userInfo)
        {
            var kite = new Kite(userInfo.APIKey, Debug: true);
            kite.SetSessionHook(onTokenExpire);
            Kite.SetAccessToken(MyAccessToken);
            var kites = kite.GetOrders();
            return kite;
        }

        public bool Login(string requestToken)
        {
            try
            {
                RequestToken = requestToken;
                User user = Kite.RequestAccessToken(requestToken, MySecret);
                MyPublicToken = user.PublicToken;
                MyAccessToken = user.AccessToken;
                Kite.SetAccessToken(MyAccessToken);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetLoginURL()
        {
            Kite = new Kite(MyAPIKey, Debug: true);
            Kite.SetSessionHook(onTokenExpire);
            var loginURL = Kite.GetLoginURL();
            return loginURL;
        }

        private void onTokenExpire()
        {
            
        }
    }
}
