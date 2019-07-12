using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;

namespace DockerPsMonitor
{
    public class ConnectionsRepository : IConnectionsRepository
    {
        public List<ConnectionItemViewModel> GetAllConnections()
        {
            var path = GetPath();
            if (!File.Exists(path) || File.ReadAllText(path).Length == 0)
            {
                return new List<ConnectionItemViewModel>();
            }
            var reader = new XmlSerializer(typeof(List<ConnectionStruct>));
            using (var file = new StreamReader(path))
            {
                var connections = (List<ConnectionStruct>)reader.Deserialize(file);
                return connections.Select(c => new ConnectionItemViewModel(this, ConnectionModeEnum.SSH, c.Name, c.Address)
                {
                    UserName = c.UserName,
                    Password = GetDecryptedPwd(c.Password)
                }).ToList();
            }
        }

        public bool SaveAllConnections(List<ConnectionItemViewModel> connections)
        {
            try
            {
                var writer = new XmlSerializer(typeof(List<ConnectionStruct>));
                var path = GetPath();
                using (var file = File.Open(path, FileMode.Create))
                {
                    var connectionStructs = connections.Where(c => c.Mode != ConnectionModeEnum.CMD).Select(c => new ConnectionStruct
                    {
                        Name = c.Name,
                        Address = c.Address,
                        UserName = c.UserName,
                        Password = GetEncryptedPwd(c.Password)
                    }).ToList();
                    writer.Serialize(file, connectionStructs);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private string GetEncryptedPwd(SecureString pwd)
        {
            var mac = NetworkInterface.GetAllNetworkInterfaces().OrderBy(n => n.Name).First()
                .GetPhysicalAddress().GetAddressBytes();
            var rsaCipher = new Arc4Cipher(mac, false);
            var pwdClear = new System.Net.NetworkCredential(string.Empty, pwd).Password;
            return Convert.ToBase64String(rsaCipher.Encrypt(Encoding.Unicode.GetBytes(pwdClear.ToCharArray())));
        }

        private SecureString GetDecryptedPwd(string pwd)
        {
            if (pwd == null)
            {
                return new SecureString();
            }
            var mac = NetworkInterface.GetAllNetworkInterfaces().OrderBy(n => n.Name).First()
                .GetPhysicalAddress().GetAddressBytes();
            var rsaCipher = new Arc4Cipher(mac, false);
            var encryptedData = Convert.FromBase64String(pwd);
            var decryptedChars = Encoding.Unicode.GetChars(rsaCipher.Decrypt(encryptedData));
            var secureString = new SecureString();
            foreach (var decryptedChar in decryptedChars)
            {
                secureString.AppendChar(decryptedChar);
            }
            return secureString;
        }

        private string GetPath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Connections.xml");
        }
    }
}