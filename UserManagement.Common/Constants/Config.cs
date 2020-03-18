using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using UserManagement.Common.Converters;

namespace UserManagement.Common.Constants
{
    public static class Config
    {
        public static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Version4-");
        public static readonly string SymmetricKey = "723FFEB59C2BB844";

        private static CurrentUser currentUser;
        public static CurrentUser CurrentUser
        {
            get
            {
                if (currentUser == null)
                    currentUser = GetLocalUser();
                return currentUser;
            }
        }

        private static MasterStore masterStore;
        public static MasterStore MasterStore
        {
            get
            {
                if (masterStore == null)
                    masterStore = GetLocalMasterStore();
                return masterStore;
            }
        }

        private static CurrentUser GetLocalUser()
        {
            string userPath = FilePath + "validated-user.json";
            if (File.Exists(userPath))
            {
                using (var reader = new StreamReader(userPath, Encoding.UTF8))
                {
                    string result = reader.ReadToEnd();
                    result = CryptoEngine.Decrypt(result, SymmetricKey);
                    return JsonConvert.DeserializeObject<CurrentUser>(result);
                }
            }
            return null;
        }

        private static MasterStore GetLocalMasterStore()
        {
            string masterPath = FilePath + "master-store.json";
            if (File.Exists(masterPath))
            {
                using (var reader = new StreamReader(masterPath, Encoding.UTF8))
                {
                    string result = reader.ReadToEnd();
                    result = CryptoEngine.Decrypt(result, SymmetricKey);
                    return JsonConvert.DeserializeObject<MasterStore>(result);
                }
            }
            return null;
        }
    }

    public class CurrentUser
    {
        public string Username { get; set; }
        public string AccessCode { get; set; }
        public string AppVersionName { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Status { get; set; }
        public string Messagee { get; set; }
    }

    public class MasterStore
    {
        public long SuperMasterId { get; set; }
        public long StoreId { get; set; }
        public long UserId { get; set; }
        public string StoreName { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public long CountryCode { get; set; }
        public string Status { get; set; }
        public string Messagee { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
    }
}
