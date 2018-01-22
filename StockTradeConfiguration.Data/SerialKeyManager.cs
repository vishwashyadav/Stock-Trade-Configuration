using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradeConfiguration.Data
{
    public static class SerialKeyManager
    {
        //ID-tillmonth-todaymonth-tillyy-todayyy-tilldd-todaydd
        static string format = "{0}-{1}-{2}-{3}-{4}-{5}-{6}";
        public static string GetKey(string userID,DateTime from,DateTime to)
        {
            string fromdd = GetAppendedString( from.Day.ToString(),userID);
            string frommm = GetAppendedString(from.Month.ToString(), userID);
            string fromyy = GetAppendedString(from.Year.ToString().Substring(2), userID);
            string todd = GetAppendedString( to.Day.ToString(),userID);
            string tomm = GetAppendedString( to.Month.ToString(), userID);
            string toyy = GetAppendedString(to.Year.ToString().Substring(2), userID);
            return string.Format(format, userID.ToByteString(), tomm.ToByteString(), frommm.ToByteString(), toyy.ToByteString(), fromyy.ToByteString(), todd.ToByteString(), fromdd.ToByteString());
        }

        public static string ToByteString(this string strValue)
        {
            var bytes = Encoding.ASCII.GetBytes(strValue);
            return string.Join("_", bytes.Select(s => s.ToString()));
        }

        public static string FromByteString(this string strValue)
        {
            byte[] bytes = strValue.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToByte(s)).ToArray();
            return Encoding.ASCII.GetString(bytes);
        }


        public static bool IsLicenseActive(string key,string userId,DateTime currentDate, out DateTime validTill)
        {
            string[] splitData = key.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var user = splitData[0].FromByteString();
            if(!user.Equals(userId))
            {
                throw new Exception("Given key is not for user '" + userId+"'");
            }

            var tomm = splitData[1].FromByteString().Replace(userId,"");
            var todd = splitData[5].FromByteString().Replace(userId, "");
            var toyy = currentDate.Date.Year.ToString().Substring(0,2)+splitData[3].FromByteString().Replace(userId, "");
            var date = new DateTime(Convert.ToInt32(toyy),Convert.ToInt32(tomm), Convert.ToInt32(todd));
            validTill = date;
            return currentDate <= date;
        }

        private static string GetAppendedString(string originalString, string toAppend)
        {
            int length = originalString.Length;
            string str = string.Empty;
            for (int i = 0; i < length; i++)
            {
                str += originalString[i] + toAppend;
            }
            return str;
        }
    }
}
