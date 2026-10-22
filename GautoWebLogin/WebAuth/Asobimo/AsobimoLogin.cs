using GautoWebLogin.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GautoWebLogin.WebAuth.Asobimo
{
    public class AsobimoLogin
    {
        public static AsobimoToken Login(DeviceModel device)
        {
            var token = new AsobimoToken();
            var auth = new AsobimoAuth(device);

            if (string.IsNullOrEmpty(token.playerId) || string.IsNullOrEmpty(token.refreshToken))
            {
                token = SignUp(auth);
            }
            else
            {
                token = SignIn(auth);
            }

            return token;
        }

        private static AsobimoToken SignUp(AsobimoAuth auth)
        {
            var token = new AsobimoToken();

            var uuid = auth.create_auth();


            return token;
        }

        private static AsobimoToken SignIn(AsobimoAuth auth)
        {
            var token = new AsobimoToken();
            return token;
        }
    }
}
