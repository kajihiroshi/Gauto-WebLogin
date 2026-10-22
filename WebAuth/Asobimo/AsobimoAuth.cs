using GautoWebLogin.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GautoWebLogin.WebAuth.Asobimo
{
    public class AsobimoAuth
    {
        public DeviceModel device;
        public AsobimoAuth(DeviceModel device) 
        {
            this.device = device;
        }

        public string create_auth()
        {
            var result = string.Empty;

            return result;
        }
    }
}
