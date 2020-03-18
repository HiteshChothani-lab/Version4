using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Common.Constants;
using UserManagement.Common.Converters;
using UserManagement.Common.Enums;
using UserManagement.Common.Utilities;
using UserManagement.Entity;
using UserManagement.Manager.Mappers;
using UserManagement.WebServices;
using UserManagement.WebServices.DataContracts.Request;

namespace UserManagement.Manager
{
    public class WindowsManager : ManagerBase, IWindowsManager
    {
        private readonly IWindowsWebService _windowsWebService;

        public WindowsManager(IConnectivity connectivity, IServiceEntityMapper mapper, IWindowsWebService windowsWebService) : base(connectivity, mapper)
        {
            _windowsWebService = windowsWebService;
        }

        public async Task<ValidateUserResponseEntity> ValidateUser(ValidateUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new ValidateUserResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<ValidateUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.ValidateUser(reqContract);
            var respEntity = Mapper.Map<ValidateUserResponseEntity>(respContract);

            if (respEntity.StatusCode == (int)GenericStatusValue.Success)
            {
                respEntity.Username = reqEntity.Username;
                respEntity.AccessCode = reqEntity.AccessCode;

                string json = JsonConvert.SerializeObject(respEntity);
                json = CryptoEngine.Encrypt(json, Config.SymmetricKey);

                using (var outputFile = new StreamWriter(Config.FilePath + "validated-user.json", false, Encoding.UTF8))
                {
                    outputFile.WriteLine(json);
                }

                File.SetAttributes(Config.FilePath + "validated-user.json", FileAttributes.Hidden);
            }

            return respEntity;
        }

        public async Task<RegisterMasterStoreResponseEntity> RegisterMasterStore(RegisterMasterStoreRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new RegisterMasterStoreResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<RegisterMasterStoreRequestContract>(reqEntity);

            var respContract = await _windowsWebService.RegisterMasterStore(reqContract);
            var respEntity = Mapper.Map<RegisterMasterStoreResponseEntity>(respContract);

            if (respEntity.StatusCode == (int)GenericStatusValue.Success)
            {
                respEntity.TimeZone = reqEntity.SelectedTimeZone;
                string json = JsonConvert.SerializeObject(respEntity);
                json = CryptoEngine.Encrypt(json, Config.SymmetricKey);

                using (var outputFile = new StreamWriter(Config.FilePath + "master-store.json", false, Encoding.UTF8))
                {
                    outputFile.WriteLine(json);
                }

                File.SetAttributes(Config.FilePath + "master-store.json", FileAttributes.Hidden);
            }

            return respEntity;
        }

        public void Logout()
        {
            string userPath = Config.FilePath + "validated-user.json";
            if (File.Exists(userPath))
            {
                File.Delete(userPath);
            }

            string storePath = Config.FilePath + "master-store.json";
            if (File.Exists(storePath))
            {
                File.Delete(storePath);
            }
        }

        public async Task<DefaultResponseEntity> CheckStoreUser(CheckUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<CheckUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.CheckStoreUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> SaveUserData(SaveUserDataRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<SaveUserDataRequestContract>(reqEntity);

            var respContract = await _windowsWebService.SaveUserData(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<StoreUsersResponseEntity> GetStoreUsers(GetStoreUsersRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new StoreUsersResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<GetStoreUsersRequestContract>(reqEntity);

            var respContract = await _windowsWebService.GetStoreUsers(reqContract);
            var respEntity = Mapper.Map<StoreUsersResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<ArchieveStoreUsersResponseEntity> GetArchieveStoreUsers(GetStoreUsersRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new ArchieveStoreUsersResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<GetStoreUsersRequestContract>(reqEntity);

            var respContract = await _windowsWebService.GetArchieveStoreUsers(reqContract);
            var respEntity = Mapper.Map<ArchieveStoreUsersResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> DeleteStoreUser(DeleteStoreUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<DeleteStoreUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.DeleteStoreUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> ManageUser(ManageUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<ManageUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.ManageUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> CheckIDRArchiveUser(ManageUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<ManageUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.CheckIDRArchiveUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> CheckIDRStoreUser(ManageUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<ManageUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.CheckIDRStoreUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> DeleteArchiveUser(DeleteArchiveUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<DeleteArchiveUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.DeleteArchiveUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> EditStoreUser(EditStoreUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<EditStoreUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.EditStoreUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> UpdateNonMobileStoreUser(UpdateNonMobileStoreUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<UpdateNonMobileStoreUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.UpdateNonMobileUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> UpdateButtons(UpdateButtonsRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<UpdateButtonsRequestContract>(reqEntity);

            var respContract = await _windowsWebService.UpdateButtons(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> MoveStoreUser(MoveStoreUserRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<MoveStoreUserRequestContract>(reqEntity);

            var respContract = await _windowsWebService.MoveStoreUser(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }

        public async Task<DefaultResponseEntity> SetUnsetFlag(SetUnsetFlagRequestEntity reqEntity)
        {
            if (!Connectivity.IsInternetAvailable)
            {
                return new DefaultResponseEntity() { StatusCode = (int)GenericStatusValue.NoInternetConnection };
            }

            var reqContract = Mapper.Map<SetUnsetFlagRequestContract>(reqEntity);

            var respContract = await _windowsWebService.SetUnsetFlag(reqContract);
            var respEntity = Mapper.Map<DefaultResponseEntity>(respContract);

            return respEntity;
        }
    }
}
