using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordClient.Models
{
    /// <summary>
    /// username + encrypted password.
    /// In the password file we store username + encrypted password.
    /// The encrypted password is a byte array (cannot be written to the password file)
    /// This must be Base64 encoded (converted to a string) before written to the file 
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public String Username { get; set; }
        public String EntryptedPasswordBase64 { get; set; }
        public byte[] EntryptedPassword { get; set; }

        public UserInfo(String username, String entryptedPasswordBase64)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            if (entryptedPasswordBase64 == null)
            {
                throw new ArgumentNullException("entryptedPasswordBase64");
            }
            Username = username;
            EntryptedPasswordBase64 = entryptedPasswordBase64;
            EntryptedPassword = Convert.FromBase64String(entryptedPasswordBase64);
        }

        public override string ToString()
        {
            return Username + ":" + EntryptedPasswordBase64;
        }
    }
}
