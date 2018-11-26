using ConfigData;
using System.Net;
using System.Net.Sockets;

namespace ATP.SocketSearch
{
    class Atp2OtherSystem
    {
        public UdpClient client { get; private set; }

        public void CreateSocket(string systemName)
        {
            var ipItem = IPConfigure.FindIpList(systemName);
            client = new UdpClient(new IPEndPoint(ipItem.ATPIP, ipItem.ATPPort));
            client.Connect(ipItem.IP, ipItem.Port);
        }

        public virtual void Initialize()
        {
            throw new System.NotImplementedException();
        }
    }
}
