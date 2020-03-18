using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using System.Windows;
using UserManagement.Common.Constants;
using UserManagement.Common.Enums;
using UserManagement.Entity;
using UserManagement.Manager;
using UserManagement.UI.Events;
using UserManagement.UI.ItemModels;

namespace UserManagement.UI.ViewModels
{
    public class EditUserAgeOrNeedlePopupPageViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowsManager _windowsManager;

        public EditUserAgeOrNeedlePopupPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IWindowsManager windowsManager) : base(regionManager)
        {
            _eventAggregator = eventAggregator;
            _windowsManager = windowsManager;

            this.VeryTerribleCheckedCommand = new DelegateCommand<string>((user) => ExecuteVeryTerribleCheckedCommand(user));
            this.VeryTerribleUncheckedCommand = new DelegateCommand<string>((user) => ExecuteVeryTerribleUncheckedCommand(user));
            this.CancelCommand = new DelegateCommand(() => ExecuteCancelCommand());
            this.SubmitCommand = new DelegateCommand(async () => await ExecuteSubmitCommand());
        }

        public DelegateCommand<string> VeryTerribleCheckedCommand { get; private set; }

        public DelegateCommand<string> VeryTerribleUncheckedCommand { get; private set; }

        private StoreUserEntity _selectedStoreUser;
        public StoreUserEntity SelectedStoreUser
        {
            get => _selectedStoreUser;
            set => SetProperty(ref _selectedStoreUser, value);
        }

        private bool _isUserTypeMobile = true;
        public bool IsUserTypeMobile
        {
            get => _isUserTypeMobile;
            set => SetProperty(ref _isUserTypeMobile, value);
        }

        private bool _isUserTypeNonMobile;
        public bool IsUserTypeNonMobile
        {
            get => _isUserTypeNonMobile;
            set => SetProperty(ref _isUserTypeNonMobile, value);
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

        private bool IsSelectedStoreUser = false;

        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand SubmitCommand { get; private set; }

        private void ExecuteCancelCommand()
        {
            this.RegionNavigationService.Journal.Clear();
            _eventAggregator.GetEvent<EditUserAgeOrNeedleSubmitEvent>().Publish(null);
        }

        private async Task ExecuteSubmitCommand()
        {
            if (!this.IsCheckedVeryGood && !this.IsCheckedIndifferent)
            {
                MessageBox.Show("You must make a selection for very Good or indifferent or both.", "Required.");
                return;
            }

            var reqEntity = new UpdateButtonsRequestEntity
            {
                Id = this.SelectedStoreUser.Id,
                UserId = Convert.ToInt32(this.SelectedStoreUser.UserId),
                SuperMasterId = Config.MasterStore.UserId,
                Action = this.IsSelectedStoreUser ? "update_buttons" : "update_buttons_archive"
            };

            reqEntity.Button1 = string.Empty;

            if (this.IsCheckedVeryGood)
            {
                reqEntity.Button1 = "Very Good";
            }

            reqEntity.Button2 = string.Empty;

            if (this.IsCheckedIndifferent)
            {
                reqEntity.Button2 = "Indifferent";
            }

            reqEntity.Button3 = string.Empty; //"Very Terrible";

            if (this.IsCheckedVeryTerribleNone)
            {
                reqEntity.Button3 = "Very Terrible";
            }
            else if (this.IsCheckedVeryTerribleNoneDeal)
            {
                reqEntity.Button3 = "No Deals";
            }
            else if (this.IsCheckedVeryTerribleTerribleService)
            {
                reqEntity.Button3 = "Terrible Service";
            }
            else if (this.IsCheckedVeryTerribleNoneDealTerribleService)
            {
                reqEntity.Button3 = "No deals & Terrible Service";
            }

            var result = await _windowsManager.UpdateButtons(reqEntity);

            if (result.StatusCode == (int)GenericStatusValue.Success)
            {
                if (Convert.ToBoolean(result.Status))
                {
                    this.RegionNavigationService.Journal.Clear();
                    _eventAggregator.GetEvent<EditUserAgeOrNeedleSubmitEvent>().Publish(new EditUserAgeOrNeedleItemModel());
                    SetUnsetProperties();
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
                this.IsCheckedVeryTerribleNone = this.IsCheckedVeryTerribleTerribleService == false && 
                    this.IsCheckedVeryTerribleNoneDealTerribleService == false;
            }
            else if (parameter == "2")
            {
                this.IsCheckedVeryTerribleNone = this.IsCheckedVeryTerribleNoneDeal == false &&
                    this.IsCheckedVeryTerribleNoneDealTerribleService == false;
            }
            else if (parameter == "3")
            {
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

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            SetUnsetProperties();

            if (navigationContext.Parameters[NavigationConstants.SelectedStoreUser] is StoreUserEntity selectedStoreUser)
            {
                SelectedStoreUser = selectedStoreUser;
            }

            if (navigationContext.Parameters[NavigationConstants.IsSelectedStoreUser] is bool isSelectedStoreUser)
            {
                IsSelectedStoreUser = isSelectedStoreUser;
            }

            this.IsCheckedVeryGood = SelectedStoreUser.Btn1.Equals("Very Good");
            this.IsCheckedIndifferent = SelectedStoreUser.Btn2.Equals("Indifferent");
            this.IsCheckedVeryTerrible = !string.IsNullOrWhiteSpace(SelectedStoreUser.Btn3);

            if (SelectedStoreUser.Btn3.Equals("Very Terrible"))
            {
                this.IsCheckedVeryTerribleNone = true;
            }
            else if (SelectedStoreUser.Btn3.Equals("No Deals"))
            {
                this.IsCheckedVeryTerribleNoneDeal = true;
            }
            else if (SelectedStoreUser.Btn3.Equals("Terrible Service"))
            {
                this.IsCheckedVeryTerribleTerribleService = true;
            }
            else if (SelectedStoreUser.Btn3.Equals("No deals "))
            {
                this.IsCheckedVeryTerribleNoneDealTerribleService = true;
            }
        }

        private void SetUnsetProperties()
        {
            this.IsCheckedIndifferent = false;
            this.IsCheckedVeryGood = false;
            this.IsCheckedVeryTerrible = false;
            this.IsCheckedVeryTerribleNone = false;
            this.IsCheckedVeryTerribleNoneDeal = false;
            this.IsCheckedVeryTerribleNoneDealTerribleService = false;
            this.IsCheckedVeryTerribleTerribleService = false;
            this.IsUserTypeMobile = true;
            this.IsUserTypeNonMobile = false;
            this.IsSelectedStoreUser = false;
            this.SelectedStoreUser = new StoreUserEntity();
        }
    }
}
