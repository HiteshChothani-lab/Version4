using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using UserManagement.Common.Constants;
using UserManagement.Common.Enums;
using UserManagement.Entity;
using UserManagement.Manager;
using UserManagement.Pushers.Events;
using UserManagement.UI.Converters;
using UserManagement.UI.Events;
using UserManagement.UI.ItemModels;
using UserManagement.UI.Views;

namespace UserManagement.UI.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowsManager _windowsManager;
        private readonly Timer UpdateExpressTime;
        private object locker = new object();

        public MainPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IWindowsManager windowsManager) : base(regionManager)
        {
            _eventAggregator = eventAggregator;
            _windowsManager = windowsManager;

            _eventAggregator.GetEvent<NonMobileUserUpdateEvent>().Subscribe(async (user) =>
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);

                if (user != null)
                {
                    if (user.IsNewRecord)
                    {
                        this.NonMobileUser = user;
                        this.FirstName = user.FirstName;
                        this.LastName = user.LastName;
                    }
                    else
                    {
                        await GetData();
                    }
                }
            });

            _eventAggregator.GetEvent<NonMobileUserEditEvent>().Subscribe(async (user) =>
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);

                if (user != null)
                {
                    await GetData();
                }

            });

            _eventAggregator.GetEvent<EditStoreUserSubmitEvent>().Subscribe(async (user) =>
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);

                if (user != null)
                {
                    await GetData();
                }
            });

            _eventAggregator.GetEvent<SetRoomNumberSubmitEvent>().Subscribe((room) =>
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);
            });

            _eventAggregator.GetEvent<MoveStoreUserSubmitEvent>().Subscribe(async (user) =>
                {
                    _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);

                    if (user != null)
                    {
                        await GetData();
                    }
                });

            _eventAggregator.GetEvent<MoveStoreUserToArchiveSubmitEvent>().Subscribe(async (user) =>
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);

                if (user != null)
                {
                    await GetData();
                }
            });

            _eventAggregator.GetEvent<EditUserAgeOrNeedleSubmitEvent>().Subscribe(async (user) =>
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);

                if (user != null)
                {
                    await GetData();
                }
            });

            _eventAggregator.GetEvent<ExpressTimeSubmitEvent>().Subscribe(async (expTime) =>
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(false);

                if (expTime != null)
                {
                    ExpressTime = expTime.FinalTime;
                    await ExecuteAddUserCommand();
                }
            });

            this.NonMobileUserCommand = new DelegateCommand(() => ExecuteNonMobileUserCommand());
            this.AddUserCommand = new DelegateCommand(async () => await ExecuteAddUserCommand());
            this.VeryTerribleCheckedCommand = new DelegateCommand<string>((user) => ExecuteVeryTerribleCheckedCommand(user));
            this.VeryTerribleUncheckedCommand = new DelegateCommand<string>((user) => ExecuteVeryTerribleUncheckedCommand(user));
            this.DeleteStoreUserCommand = new DelegateCommand<StoreUserEntity>(async (user) => await ExecuteDeleteStoreUserCommand(user));
            this.DeleteArchiveUserCommand = new DelegateCommand<StoreUserEntity>(async (user) => await ExecuteDeleteArchiveUserCommand(user));
            this.EditStoreUserCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteEditStoreUserCommand(user));
            this.SetFlagCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteFlagCommand(user));
            this.EditNonMobileStoreUserCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteEditNonMobileStoreUserCommand(user));
            this.EditAgeOrNeedleUserCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteEditAgeOrNeedleUserCommand(user));
            this.EditArchiveAgeOrNeedleUserCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteEditArchiveAgeOrNeedleUserCommand(user));
            this.EditNonMobileArchiveStoreUserCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteEditNonMobileArchiveStoreUserCommand(user));
            this.MoveStoreUserCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteMoveStoreUserCommand(user));
            this.LogoutCommand = new DelegateCommand(() => ExecuteLogoutCommand());
            this.RefreshDataCommand = new DelegateCommand(async () => await ExecuteRefreshDataCommand());
            this.ExpressUserCommand = new DelegateCommand(() => ExecuteExpressUserCommand());
            this.StoreIDCheckedCommand = new DelegateCommand<StoreUserEntity>(async (user) => await ExecuteStoreIDCheckedCommand(user));
            this.ArchiveIDCheckedCommand = new DelegateCommand<StoreUserEntity>(async (user) => await ExecuteArchiveIDCheckedCommand(user));
            this.UserDetailWindowCommand = new DelegateCommand<StoreUserEntity>((user) => ExecuteUserDetailWindowCommand(user));
            this.SetRoomNumberCommand = new DelegateCommand<StoreUserEntity>(ExecuteSetRoomNumberCommand);

            if (IsExpressEnable)
            {
                UpdateExpressTime = new Timer(60000); /* 60000 Millisecond = 1 Minute (Interval) */
                UpdateExpressTime.Elapsed += UpdateExpressTime_Elapsed;
            }

            #region Pusher Events Subscribe

            _eventAggregator.GetEvent<RefreshData>().Subscribe((model) =>
            {
                try
                {
                    lock (locker)
                    {
                        if (model.Action == PusherAction.Store)
                        {
                            if (model.EventName.Equals(PusherData.DeleteStoreUser))
                            {
                                Task.WaitAll(GetData());
                            }
                            else
                            {
                                Task.WhenAll(GetStoreUsers());
                            }
                        }
                        else if (model.Action == PusherAction.Archieve)
                        {
                            Task.WhenAll(GetArchieveStoreUsers());
                        }
                    }
                }
                catch (Exception) { }
            });

            #endregion
        }

        private void ExecuteLogoutCommand()
        {
            _windowsManager.Logout();
            if (IsExpressEnable)
            {
                UpdateExpressTime.Stop();
                UpdateExpressTime.Elapsed -= UpdateExpressTime_Elapsed;
                UpdateExpressTime.Dispose();
            }
            System.Windows.Application.Current.Shutdown();
        }

        private ObservableCollection<StoreUserEntity> _storeUsers = new ObservableCollection<StoreUserEntity>();
        public ObservableCollection<StoreUserEntity> StoreUsers
        {
            get => _storeUsers;
            set => SetProperty(ref _storeUsers, value);
        }

        private ObservableCollection<StoreUserEntity> _archieveStoreUsers = new ObservableCollection<StoreUserEntity>();
        public ObservableCollection<StoreUserEntity> ArchieveStoreUsers
        {
            get => _archieveStoreUsers;
            set => SetProperty(ref _archieveStoreUsers, value);
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _mobileNumber;
        public string MobileNumber
        {
            get => _mobileNumber;
            set => SetProperty(ref _mobileNumber, value);
        }

        private NonMobileUserItemModel _nonMobileUser;
        public NonMobileUserItemModel NonMobileUser
        {
            get => _nonMobileUser;
            set => SetProperty(ref _nonMobileUser, value);
        }

        private bool _isCheckedVeryGood;
        public bool IsCheckedVeryGood
        {
            get => _isCheckedVeryGood;
            set => SetProperty(ref _isCheckedVeryGood, value);
        }

        private bool _isCheckedIndifferent;
        public bool IsCheckedIndifferent
        {
            get => _isCheckedIndifferent;
            set => SetProperty(ref _isCheckedIndifferent, value);
        }

        private bool _isCheckedVeryTerrible;
        public bool IsCheckedVeryTerrible
        {
            get => _isCheckedVeryTerrible;
            set => SetProperty(ref _isCheckedVeryTerrible, value);
        }

        private bool _isCheckedVeryTerribleNoneDeal;
        public bool IsCheckedVeryTerribleNoneDeal
        {
            get => _isCheckedVeryTerribleNoneDeal;
            set => SetProperty(ref _isCheckedVeryTerribleNoneDeal, value);
        }

        private bool _isCheckedVeryTerribleTerribleService;
        public bool IsCheckedVeryTerribleTerribleService
        {
            get => _isCheckedVeryTerribleTerribleService;
            set => SetProperty(ref _isCheckedVeryTerribleTerribleService, value);
        }

        private bool _isCheckedVeryTerribleNoneDealTerribleService;
        public bool IsCheckedVeryTerribleNoneDealTerribleService
        {
            get => _isCheckedVeryTerribleNoneDealTerribleService;
            set => SetProperty(ref _isCheckedVeryTerribleNoneDealTerribleService, value);
        }

        private bool _isCheckedVeryTerribleNone;
        public bool IsCheckedVeryTerribleNone
        {
            get => _isCheckedVeryTerribleNone;
            set => SetProperty(ref _isCheckedVeryTerribleNone, value);
        }

        private bool _canTapAddCommand = true;
        public bool CanTapAddCommand
        {
            get => _canTapAddCommand;
            set => SetProperty(ref _canTapAddCommand, value);
        }

        private Visibility _loaderVisibility = Visibility.Collapsed;
        public Visibility LoaderVisibility
        {
            get => _loaderVisibility;
            set => SetProperty(ref _loaderVisibility, value);
        }

        private string _loaderMessage = string.Empty;
        public string LoaderMessage
        {
            get => _loaderMessage;
            set => SetProperty(ref _loaderMessage, value);
        }

        private string _expressTime;
        public string ExpressTime
        {
            get => _expressTime;
            set => SetProperty(ref _expressTime, value);
        }

        private int TotalStoreUsers
        {
            get
            {
                return StoreUsers == null || StoreUsers.Count <= 0 ? 0 : StoreUsers.Count;
            }
        }

        public bool IsVaccinationVisible
        {
            get => Config.MasterStore.FacilityType.Equals("Clinic");
        }

        public bool IsExpressEnable
        {
            get => Config.MasterStore.FacilityType.Equals("Clinic");
        }

        public bool IsAppointmentEnable
        {
            get => !Config.MasterStore.FacilityType.Equals("Test Center");
        }

        private UserDetailsPage userDetailsPage;

        public DelegateCommand NonMobileUserCommand { get; private set; }
        public DelegateCommand AddUserCommand { get; private set; }
        public DelegateCommand ExpressUserCommand { get; private set; }
        public DelegateCommand<string> VeryTerribleCheckedCommand { get; private set; }
        public DelegateCommand<string> VeryTerribleUncheckedCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> DeleteStoreUserCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> DeleteArchiveUserCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> EditStoreUserCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> SetFlagCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> EditNonMobileStoreUserCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> EditAgeOrNeedleUserCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> EditArchiveAgeOrNeedleUserCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> EditNonMobileArchiveStoreUserCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> MoveStoreUserCommand { get; private set; }
        public DelegateCommand LogoutCommand { get; private set; }
        public DelegateCommand RefreshDataCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> StoreIDCheckedCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> ArchiveIDCheckedCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> UserDetailWindowCommand { get; private set; }
        public DelegateCommand<StoreUserEntity> SetRoomNumberCommand { get; private set; }

        private void ExecuteNonMobileUserCommand()
        {
            _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
            var parameters = new NavigationParameters();
            this.RegionManager.RequestNavigate("PopupRegion", ViewNames.NonMobileUserPopup, parameters);
        }

        private void ExecuteVeryTerribleUncheckedCommand(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                this.IsCheckedVeryTerribleNone = false;
                this.IsCheckedVeryTerribleNoneDeal = false;
                this.IsCheckedVeryTerribleTerribleService = false;
                this.IsCheckedVeryTerribleNoneDealTerribleService = false;
            }
            else if (parameter == "1")
            {
                if (this.IsCheckedVeryTerribleNone)
                    this.IsCheckedVeryTerribleNone = this.IsCheckedVeryTerribleTerribleService == false &&
                    this.IsCheckedVeryTerribleNoneDealTerribleService == false;
            }
            else if (parameter == "2")
            {
                if (this.IsCheckedVeryTerribleNone)
                    this.IsCheckedVeryTerribleNone = this.IsCheckedVeryTerribleNoneDeal == false &&
                    this.IsCheckedVeryTerribleNoneDealTerribleService == false;
            }
            else if (parameter == "3")
            {
                if (this.IsCheckedVeryTerribleNone)
                    this.IsCheckedVeryTerribleNone = this.IsCheckedVeryTerribleNoneDeal == false &&
                    this.IsCheckedVeryTerribleTerribleService == false;
            }
        }

        private void ExecuteVeryTerribleCheckedCommand(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                this.IsCheckedVeryTerribleNone = true;
                this.IsCheckedVeryTerribleNoneDeal = false;
                this.IsCheckedVeryTerribleTerribleService = false;
                this.IsCheckedVeryTerribleNoneDealTerribleService = false;
            }
            else if (parameter == "1")
            {
                this.IsCheckedVeryTerribleNone = false;
                this.IsCheckedVeryTerribleNoneDeal = true;
                this.IsCheckedVeryTerribleTerribleService = false;
                this.IsCheckedVeryTerribleNoneDealTerribleService = false;
            }
            else if (parameter == "2")
            {
                this.IsCheckedVeryTerribleNone = false;
                this.IsCheckedVeryTerribleNoneDeal = false;
                this.IsCheckedVeryTerribleTerribleService = true;
                this.IsCheckedVeryTerribleNoneDealTerribleService = false;
            }
            else if (parameter == "3")
            {
                this.IsCheckedVeryTerribleNone = false;
                this.IsCheckedVeryTerribleNoneDeal = false;
                this.IsCheckedVeryTerribleTerribleService = false;
                this.IsCheckedVeryTerribleNoneDealTerribleService = true;
            }
        }

        private async Task ExecuteAddUserCommand()
        {
            this.CanTapAddCommand = false;
            bool isMobileUser = false;

            if (this.NonMobileUser == null)
            {
                isMobileUser = true;
                if (string.IsNullOrEmpty(this.FirstName))
                {
                    MessageBox.Show("First Name is required.", "Required");
                    return;
                }
                else if (string.IsNullOrEmpty(this.LastName))
                {
                    MessageBox.Show("Last Name is required.", "Required");
                    return;
                }
                else if (string.IsNullOrEmpty(this.MobileNumber))
                {
                    MessageBox.Show("Mobile Number is required.", "Required");
                    return;
                }
            }

            if (IsAppointmentEnable && !this.IsCheckedVeryGood && !this.IsCheckedIndifferent)
            {
                MessageBox.Show("You must make a selection for New Case or Follow Up or both.", "Required.");
                return;
            }

            var reqEntity = new SaveUserDataRequestEntity
            {
                Action = "master",
                FirstName = this.FirstName,
                LastName = this.LastName,
                CountryCode = Config.MasterStore.CountryCode,
                StoreId = Config.MasterStore.StoreId,
                SuperMasterId = Config.MasterStore.UserId
            };

            if (!isMobileUser)
            {
                reqEntity.Mobile = string.Empty;
                reqEntity.OrphanStatus = 1;
                reqEntity.PostalCode = this.NonMobileUser.PostalCode;
                reqEntity.HomePhone = this.NonMobileUser.HomePhone;
                reqEntity.Country = this.NonMobileUser.Country;
                reqEntity.City = this.NonMobileUser.City;
                reqEntity.State = this.NonMobileUser.State;
                reqEntity.Gender = this.NonMobileUser.Gender;
                reqEntity.DOB = this.NonMobileUser.DOB;
            }
            else
            {
                reqEntity.Mobile = this.MobileNumber;
                reqEntity.OrphanStatus = 0;
                reqEntity.PostalCode = string.Empty;
                reqEntity.HomePhone = string.Empty;
            }

            reqEntity.ExpressTime = this.ExpressTime;
            reqEntity.DeliverOrderStatus = TotalStoreUsers;
            reqEntity.FillStatus = 1;

            reqEntity.Button1 = string.Empty;

            if (this.IsCheckedVeryGood)
            {
                reqEntity.Button1 = "New Case";
            }

            reqEntity.Button2 = string.Empty;

            if (this.IsCheckedIndifferent)
            {
                reqEntity.Button2 = "Follow Up";
            }

            if (this.IsCheckedVeryTerribleNone)
            {
                reqEntity.Button3 = "Vaccination";
            }
            else if (this.IsCheckedVeryTerribleNoneDeal)
            {
                reqEntity.Button3 = "Flu Shot";
            }
            else if (this.IsCheckedVeryTerribleTerribleService)
            {
                reqEntity.Button3 = "Other Vaccines";
            }
            else if (this.IsCheckedVeryTerribleNoneDealTerribleService)
            {
                reqEntity.Button3 = "Flu Shot & Other Vaccines";
            }

            reqEntity.Button4 = string.Empty;

            SetLoaderVisibility("Adding user...");
            var result = await _windowsManager.SaveUserData(reqEntity, false);
            if (result.StatusCode == (int)GenericStatusValue.Success)
            {
                SetLoaderVisibility();
                if (Convert.ToBoolean(result.Status))
                {
                    ResetFields();
                    if (this.StoreUsers.Count < 4 || !string.IsNullOrEmpty(reqEntity.ExpressTime))
                        await GetData();
                }
                else
                {
                    MessageBox.Show(result.Messagee, "Unsuccessful");
                }
            }
            else if (result.StatusCode == (int)GenericStatusValue.NoInternetConnection)
            {
                SetLoaderVisibility();
                MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
            }
            else if (result.StatusCode == (int)GenericStatusValue.HasErrorMessage)
            {
                SetLoaderVisibility();
                MessageBox.Show(result.Message, "Unsuccessful");
            }
            else
            {
                SetLoaderVisibility();
                MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
            }

            SetLoaderVisibility();
            this.CanTapAddCommand = true;
            this.ExpressTime = string.Empty;
        }

        private async Task ExecuteDeleteStoreUserCommand(StoreUserEntity parameter)
        {
            var dialogResult = MessageBox.Show("Are you want to delete a user?", "Delete store user", MessageBoxButton.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                SetLoaderVisibility("Deleting user...");

                var result = await _windowsManager.DeleteStoreUser(new DeleteStoreUserRequestEntity()
                {
                    Id = parameter.Id,
                    MasterStoreId = parameter.MasterStoreId,
                    OrphanStatus = parameter.OrphanStatus,
                    UserId = parameter.UserId,
                    SuperMasterId = Config.MasterStore.UserId
                });

                if (result.StatusCode == (int)GenericStatusValue.Success)
                {
                    SetLoaderVisibility();
                    if (Convert.ToBoolean(result.Status))
                    {
                        //MessageBox.Show(result.Messagee, "Successful");
                        ResetFields();
                        await GetData();
                    }
                    else
                    {
                        MessageBox.Show(result.Messagee, "Unsuccessful");
                    }
                }
                else if (result.StatusCode == (int)GenericStatusValue.NoInternetConnection)
                {
                    SetLoaderVisibility();
                    MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
                }
                else if (result.StatusCode == (int)GenericStatusValue.HasErrorMessage)
                {
                    SetLoaderVisibility();
                    MessageBox.Show(result.Message, "Unsuccessful");
                }
                else
                {
                    SetLoaderVisibility();
                    MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
                }

                SetLoaderVisibility();
            }
        }

        private async Task ExecuteDeleteArchiveUserCommand(StoreUserEntity parameter)
        {
            if (parameter.OrphanStatus == "1")
            {
                if (parameter.IdrStatus == "0")
                {
                    if (parameter.RegisterType == "second")
                    {
                        var dialogResult = MessageBox.Show("Id has not been checked? (Select Yes if you want Id Checked)", "", MessageBoxButton.YesNo);
                        if (dialogResult == MessageBoxResult.Yes)
                        {
                            await ManageUser(parameter);
                        }
                    }
                    else
                    {
                        var dialogResult = MessageBox.Show("This person was not verified. If you delete them, they will be not be registered. Delete anyway?", "Delete", MessageBoxButton.YesNo);
                        if (dialogResult == MessageBoxResult.Yes)
                        {
                            await DeleteArchiveUser(parameter);
                        }
                    }
                }
                else
                {
                    var dialogResult = MessageBox.Show("Are you sure you want to delete?", "Delete", MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        await DeleteArchiveUser(parameter);
                    }
                }
            }
            else
            {
                var dialogResult = MessageBox.Show("Are you sure you want to delete?", "Delete", MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    await DeleteArchiveUser(parameter);
                }
            }
        }

        private async Task ManageUser(StoreUserEntity parameter)
        {
            SetLoaderVisibility("Deleting Archieve User...");

            var deleteResult = await _windowsManager.ManageUser(new ManageUserRequestEntity()
            {
                Id = parameter.Id,
                MasterStoreId = parameter.MasterStoreId
            });

            if (deleteResult.StatusCode == (int)GenericStatusValue.Success)
            {
                SetLoaderVisibility();
                await GetData();
            }
            else if (deleteResult.StatusCode == (int)GenericStatusValue.NoInternetConnection)
            {
                SetLoaderVisibility();
                MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
            }
            else if (deleteResult.StatusCode == (int)GenericStatusValue.HasErrorMessage)
            {
                SetLoaderVisibility();
                MessageBox.Show(deleteResult.Message, "Unsuccessful");
            }
            else
            {
                SetLoaderVisibility();
                MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
            }
        }

        private async Task DeleteArchiveUser(StoreUserEntity parameter)
        {
            SetLoaderVisibility("Deleting Archieve User...");

            var deleteResult = await _windowsManager.DeleteArchiveUser(new DeleteArchiveUserRequestEntity()
            {
                Id = parameter.Id,
                UserId = parameter.UserId,
                MasterStoreId = parameter.MasterStoreId,
                SuperMasterId = Config.MasterStore.UserId
            });

            if (deleteResult.StatusCode == (int)GenericStatusValue.Success)
            {
                SetLoaderVisibility();
                await GetData();
            }
            else if (deleteResult.StatusCode == (int)GenericStatusValue.NoInternetConnection)
            {
                SetLoaderVisibility();
                MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
            }
            else if (deleteResult.StatusCode == (int)GenericStatusValue.HasErrorMessage)
            {
                SetLoaderVisibility();
                MessageBox.Show(deleteResult.Message, "Unsuccessful");
            }
            else
            {
                SetLoaderVisibility();
                MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
            }
        }

        private void ExecuteEditStoreUserCommand(StoreUserEntity user)
        {
            _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
            var parameters = new NavigationParameters();
            parameters.Add(NavigationConstants.SelectedStoreUser, user);
            this.RegionManager.RequestNavigate("PopupRegion", ViewNames.EditUserPopupPage, parameters);
        }

        private async void ExecuteFlagCommand(StoreUserEntity user)
        {
            try
            {
                SetLoaderVisibility("Updating Flag...");

                var result = await _windowsManager.SetUnsetFlag(new SetUnsetFlagRequestEntity()
                {
                    Id = user.Id,
                    MasterStoreId = user.MasterStoreId,
                    RecentStatus = user.IsFlagSet ? 0 : 1
                });

                if (result.StatusCode == (int)GenericStatusValue.Success)
                {
                    if (Convert.ToBoolean(result.Status))
                    {
                        await GetStoreUsers();
                    }
                    else
                    {
                        MessageBox.Show(result.Messagee, "Unsuccessful");
                    }
                }
                else if (result.StatusCode == (int)GenericStatusValue.NoInternetConnection)
                {
                    MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
                }
                else if (result.StatusCode == (int)GenericStatusValue.HasErrorMessage)
                {
                    MessageBox.Show(result.Message, "Unsuccessful");
                }
                else
                {
                    MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unsuccessful");
            }
            finally
            {
                SetLoaderVisibility();
            }
        }

        private void ExecuteEditNonMobileStoreUserCommand(StoreUserEntity user)
        {
            if (user.OrphanStatus == "1")
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
                var parameters = new NavigationParameters();
                parameters.Add(NavigationConstants.SelectedStoreUser, user);
                parameters.Add(NavigationConstants.Action, "update_non_mobile");
                this.RegionManager.RequestNavigate("PopupRegion", ViewNames.UpdateNonMobileUserPopupPage, parameters);
            }
        }

        private void ExecuteEditAgeOrNeedleUserCommand(StoreUserEntity user)
        {
            if (IsAppointmentEnable)
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
                var parameters = new NavigationParameters
                {
                    { NavigationConstants.SelectedStoreUser, user },
                    { NavigationConstants.Action, "update_non_mobile" },
                    { NavigationConstants.IsSelectedStoreUser, true }
                };
                this.RegionManager.RequestNavigate("PopupRegion", ViewNames.EditUserAgeOrNeedlePopupPage, parameters);
            }
        }

        private void ExecuteEditArchiveAgeOrNeedleUserCommand(StoreUserEntity user)
        {
            if (IsAppointmentEnable)
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
                var parameters = new NavigationParameters
                {
                    { NavigationConstants.SelectedStoreUser, user },
                    { NavigationConstants.Action, "update_non_mobile" },
                    { NavigationConstants.IsSelectedStoreUser, false }
                };
                this.RegionManager.RequestNavigate("PopupRegion", ViewNames.EditUserAgeOrNeedlePopupPage, parameters);
            }
        }

        private void ExecuteExpressUserCommand()
        {
            if (this.NonMobileUser == null)
            {
                if (string.IsNullOrEmpty(this.FirstName))
                {
                    MessageBox.Show("First Name is required.", "Required");
                    return;
                }
                else if (string.IsNullOrEmpty(this.LastName))
                {
                    MessageBox.Show("Last Name is required.", "Required");
                    return;
                }
                else if (string.IsNullOrEmpty(this.MobileNumber))
                {
                    MessageBox.Show("Mobile Number is required.", "Required");
                    return;
                }
            }

            if (!this.IsCheckedVeryGood && !this.IsCheckedIndifferent)
            {
                MessageBox.Show("You must make a selection for New Case or Follow Up or both.", "Required.");
                return;
            }

            _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
            this.RegionManager.RequestNavigate("PopupRegion", ViewNames.ExpressTimePickerPopupPage);
        }

        private void ExecuteEditNonMobileArchiveStoreUserCommand(StoreUserEntity user)
        {
            if (user.OrphanStatus == "1")
            {
                _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
                var parameters = new NavigationParameters();
                parameters.Add(NavigationConstants.SelectedStoreUser, user);
                parameters.Add(NavigationConstants.Action, "update_non_mobile_archive");
                this.RegionManager.RequestNavigate("PopupRegion", ViewNames.UpdateNonMobileUserPopupPage, parameters);
            }
        }

        private void ExecuteUserDetailWindowCommand(StoreUserEntity user)
        {
            if (userDetailsPage != null)
            {
                userDetailsPage.Close();
                userDetailsPage = null;
            }

            userDetailsPage = new UserDetailsPage { DataContext = user };
            userDetailsPage.PostalCodeText.Text = user.IsZipCode ? "Zip Code :" : "Postal Code :";
            userDetailsPage.Show();
        }

        private void ExecuteMoveStoreUserCommand(StoreUserEntity user)
        {
            var reverseStoreUsers = this.StoreUsers.ToList();
            reverseStoreUsers.Reverse();

            if (reverseStoreUsers.Count >= 5)
            {
                var selectedIndex = reverseStoreUsers.IndexOf(user);

                if (selectedIndex > 3)
                {
                    var dialogResult = MessageBox.Show("Are you sure you want to move this entry's position?", "Moving user position", MessageBoxButton.YesNo);

                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
                        var parameters = new NavigationParameters();
                        parameters.Add(NavigationConstants.StoreUsers, reverseStoreUsers.ToList());
                        parameters.Add(NavigationConstants.SelectedIndex, selectedIndex);
                        this.RegionManager.RequestNavigate("PopupRegion", ViewNames.MoveUserPopupPage, parameters);
                    }
                }
                else
                {
                    MessageBox.Show("Sorry you can't move once in here.");
                }
            }
            else
            {
                MessageBox.Show("Sorry you can't move once in here.");
            }
        }

        private async Task ExecuteStoreIDCheckedCommand(StoreUserEntity parameter)
        {
            var dialogResult = MessageBox.Show("Id has not been checked? (Select Yes if you want Id Checked)", "ID Required", MessageBoxButton.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                SetLoaderVisibility("Updating IDR...");

                var idrResult = await _windowsManager.CheckIDRStoreUser(new ManageUserRequestEntity()
                {
                    Id = parameter.Id,
                    MasterStoreId = parameter.MasterStoreId
                });

                if (idrResult.StatusCode == (int)GenericStatusValue.Success)
                {
                    SetLoaderVisibility();
                    await GetData();
                }
                else if (idrResult.StatusCode == (int)GenericStatusValue.NoInternetConnection)
                {
                    SetLoaderVisibility();
                    MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
                }
                else if (idrResult.StatusCode == (int)GenericStatusValue.HasErrorMessage)
                {
                    SetLoaderVisibility();
                    MessageBox.Show(idrResult.Message, "Unsuccessful");
                }
                else
                {
                    SetLoaderVisibility();
                    MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
                }

                SetLoaderVisibility();
            }
        }

        private async Task ExecuteArchiveIDCheckedCommand(StoreUserEntity parameter)
        {
            var dialogResult = MessageBox.Show("Id has not been checked? (Select Yes if you want Id Checked)", "ID Required", MessageBoxButton.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                SetLoaderVisibility("Updating IDR...");

                var idrResult = await _windowsManager.CheckIDRArchiveUser(new ManageUserRequestEntity()
                {
                    Id = parameter.Id,
                    MasterStoreId = parameter.MasterStoreId
                });

                if (idrResult.StatusCode == (int)GenericStatusValue.Success)
                {
                    SetLoaderVisibility();
                    await GetData();
                }
                else if (idrResult.StatusCode == (int)GenericStatusValue.NoInternetConnection)
                {
                    SetLoaderVisibility();
                    MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
                }
                else if (idrResult.StatusCode == (int)GenericStatusValue.HasErrorMessage)
                {
                    SetLoaderVisibility();
                    MessageBox.Show(idrResult.Message, "Unsuccessful");
                }
                else
                {
                    SetLoaderVisibility();
                    MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
                }

                SetLoaderVisibility();
            }
        }

        private void ExecuteSetRoomNumberCommand(StoreUserEntity user)
        {
            _eventAggregator.GetEvent<PopupVisibilityEvent>().Publish(true);
            var parameters = new NavigationParameters { { NavigationConstants.SelectedStoreUser, user } };

            RegionManager.RequestNavigate("PopupRegion", ViewNames.SetRoomNumberPopUpPage, parameters);
        }

        private void SetLoaderVisibility(string message = "")
        {
            this.LoaderMessage = message;

            if (string.IsNullOrEmpty(message))
            {
                this.LoaderVisibility = Visibility.Collapsed;
            }
            else
            {
                this.LoaderVisibility = Visibility.Visible;
            }
        }

        private async Task ExecuteRefreshDataCommand()
        {
            SetLoaderVisibility("Loading data...");
            ResetFields();
            await GetData();
        }

        private void ResetFields()
        {
            this.NonMobileUser = null;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.MobileNumber = string.Empty;

            this.IsCheckedIndifferent = false;
            this.IsCheckedVeryGood = false;

            this.IsCheckedVeryTerribleNone = false;
            this.IsCheckedVeryTerrible = false;
            this.IsCheckedVeryTerribleNoneDeal = false;
            this.IsCheckedVeryTerribleTerribleService = false;
            this.IsCheckedVeryTerribleNoneDealTerribleService = false;
        }

        private async Task GetData()
        {
            try
            {
                this.LoaderVisibility = Visibility.Visible;

                if (IsExpressEnable && UpdateExpressTime.Enabled) UpdateExpressTime.Stop();

                await Task.WhenAll(GetStoreUsers());
                await Task.WhenAll(GetArchieveStoreUsers());
            }
            catch
            {
                if (IsExpressEnable && !UpdateExpressTime.Enabled) UpdateExpressTime.Start();
            }
            finally
            {
                this.LoaderVisibility = Visibility.Collapsed;
            }
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            try
            {
                await GetData();
            }
            catch (Exception ex)
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembl‌​y().Location);
                path += "exception.txt";
                using (var outputFile = new StreamWriter(path, false, Encoding.UTF8))
                {
                    outputFile.WriteLine(ex.StackTrace);
                }
            }
        }

        private async Task GetStoreUsers()
        {
            var result = await _windowsManager.GetStoreUsers(new GetStoreUsersRequestEntity()
            {
                StoreId = Config.MasterStore.StoreId,
                SuperMasterId = Config.MasterStore.UserId
            });

            this.StoreUsers = new ObservableCollection<StoreUserEntity>();

            if (result.StatusCode == (int)GenericStatusValue.Success)
            {
                if (Convert.ToBoolean(result.Status))
                {
                    result.Data = result.Data.Where(x => x != null && !string.IsNullOrEmpty(x.Id)).ToList();

                    if (result.Data.Count > 0)
                    {
                        //The bottom 4 rows of the table are yellow (like pouring of water).
                        result.Data.Skip(Math.Max(0, result.Data.Count() - 4)).ToList().ForEach(s => s.Column2RowColor = ColorNames.Yellow);

                        if (result.Data.Count > 4)
                        {
                            int takeSecontFour = result.Data.Count <= 8 ? result.Data.Count() - 4 : 4;

                            //And as you pour more water, rows 5, 6, 7, and 8 will be blue.
                            result.Data.Skip(Math.Max(0, result.Data.Count() - 8)).Take(takeSecontFour).ToList().ForEach(s => s.Column2RowColor = ColorNames.Blue);
                        }

                        //And as you pour more rows 9 and above all the way to infinity are green.
                        //NOTE: We have set the green color as a default color so no needed for code.
                    }

                    this.StoreUsers = new ObservableCollection<StoreUserEntity>(result.Data);
                }
            }
        }

        private async Task GetArchieveStoreUsers()
        {
            var result = await _windowsManager.GetArchiveStoreUsers(new GetStoreUsersRequestEntity()
            {
                StoreId = Config.MasterStore.StoreId,
                SuperMasterId = Config.MasterStore.UserId,
                TimeZone = Config.MasterStore.TimeZone
            });

            this.ArchieveStoreUsers = new ObservableCollection<StoreUserEntity>();

            if (result.StatusCode == (int)GenericStatusValue.Success)
            {
                if (Convert.ToBoolean(result.Status))
                {
                    result.Data = result.Data.Where(x => x != null && !string.IsNullOrEmpty(x.Id)).ToList();
                    this.ArchieveStoreUsers = new ObservableCollection<StoreUserEntity>(result.Data);
                }
            }

            if (IsExpressEnable)
            {
                if (this.ArchieveStoreUsers != null && this.ArchieveStoreUsers.Count > 0 &&
                            this.ArchieveStoreUsers.Any(a => a.RegType.Equals("Express")))
                {
                    if (this.ArchieveStoreUsers.Any(a => a.TimeDifference.Equals("early")) ||
                        this.ArchieveStoreUsers.Any(a => a.TimeDifference.Equals("ready")))
                    {
                        if (!UpdateExpressTime.Enabled) UpdateExpressTime.Start();
                    }
                    else
                    {
                        if (UpdateExpressTime.Enabled) UpdateExpressTime.Stop();
                    }
                }
            }
        }

        private async void UpdateExpressTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                UpdateExpressTime.Stop();
                if (this.ArchieveStoreUsers != null && this.ArchieveStoreUsers.Count > 0 &&
                        this.ArchieveStoreUsers.Any(a => a.RegType.Equals("Express")))
                {
                    if (this.ArchieveStoreUsers.Any(a => a.TimeDifference.Equals("early")) ||
                        this.ArchieveStoreUsers.Any(a => a.TimeDifference.Equals("ready")))
                    {
                        await Task.WhenAll(GetArchieveStoreUsers());
                    }
                }
            }
            catch
            {
                if (!UpdateExpressTime.Enabled) UpdateExpressTime.Start();
            }
        }
    }
}
