using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class JwtUtils
    {
     
        /// <summary>
        /// 生成一个Token
        /// </summary>
        /// <param name="User">用户实体</param>
        /// <param name="UserLogOn">用户实体</param>
        /// <param name="text">文本</param>
        /// <returns>token</returns>
        public static string CreateToken(Sys_User User, Sys_UserLogOn UserLogOn, string text)
        {
            header header = new header
            {
                alg = "DES",
                typ = "JWT"
            };

            Payload Payload = new Payload
            {
                Account = User.F_Account,
                iat = DateTimeHelper.GetTotoalMilliseconds(),
                exp = DateTimeHelper.GetTotoalMilliseconds(DateTime.Now.AddDays(7))
            };
            //TODO User.F_Account最后要改成text
            string token = Base64Helper.Encry(header) + "." + Base64Helper.Encry(Payload) + "." + Base64Helper.Encry(DESEncrypt.Encrypt(User.F_Account, UserLogOn.F_UserSecretkey));
            return token;
        }
        public static bool DecryToken(string token)
        {
            string[] ss = token.Split('.');

            if (ss.Length == 3)
            {
                header h = Base64Helper.Decry<header>(ss[0]);
                Payload p = Base64Helper.Decry<Payload>(ss[1]);
                string s = Base64Helper.Decry(ss[2]);
                if (DateTimeHelper.GetTotoalMilliseconds(DateTime.Now) - p.exp > 0) //令牌过期
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
    public class header
    {
        public string alg { get; set; }
        public string typ { get; set; }
    }
    public class Payload
    {
        public string Account { get; set; }
        public long iat { get; set; }
        public long exp { get; set; }
    }
}
