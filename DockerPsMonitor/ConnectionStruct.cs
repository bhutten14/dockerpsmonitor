using System;

namespace DockerPsMonitor
{
    [Serializable()]
    public struct ConnectionStruct
    {
        public string Name;
        public string Address;
        public string UserName;
        public string Password;
    }
}
