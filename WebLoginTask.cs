using GautoWebLogin.Device;
using GautoWebLogin.WebAuth.Asobimo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GautoWebLogin
{
    public class WebLoginTask
    {
        public static AsobimoToken Action()
        {
            var myDevice = new DeviceModel();

            var token = AsobimoLogin.Login(myDevice);
            return token;
        }
    }
}
