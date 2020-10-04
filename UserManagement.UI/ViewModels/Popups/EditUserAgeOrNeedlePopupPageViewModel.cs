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

        private bool _isCheckedCovid19Test;
        public bool IsCheckedCovid19Test
        {
            get => _isCheckedCovid19Test;
            set => SetProperty(ref _isCheckedCovid19Test, value);
        }

        private bool _isCheckedFluShot;
        public bool IsCheckedFluShot
        {
            get => _isCheckedFluShot;
            set => SetProperty(ref _isCheckedFluShot, value);
        }

        private bool _isCheckedShingles;
        public bool IsCheckedShingles
        {
            get => _isCheckedShingles;
            set => SetProperty(ref _isCheckedShingles, value);
        }

        private bool _isCheckedPneumococcus;
        public bool IsCheckedPneumococcus
        {
            get => _isCheckedPneumococcus;
            set => SetProperty(ref _isCheckedPneumococcus, value);
        }

        private bool _isCheckedOtherVaccination;
        public bool IsCheckedOtherVaccination
        {
            get => _isCheckedOtherVaccination;
            set => SetProperty(ref _isCheckedOtherVaccination, value);
        }

        private bool _isCheckedCovid19;
        public bool IsCheckedCovid19
        {
            get => _isCheckedCovid19;
            set => SetProperty(ref _isCheckedCovid19, value);
        }

        public bool IsVaccinationVisible
        {
            get => Config.MasterStore.FacilityType.Equals("Clinic");
        }

        private bool _isVaccinesEditable;
        public bool IsVaccinesEditable
        {
            get => _isVaccinesEditable;
            set => SetProperty(ref _isVaccinesEditable, value);
        }

        private double _vaccinesOpacity = 1.0;
        public double VaccinesOpacity
        {
            get => _vaccinesOpacity;
            set => SetProperty(ref _vaccinesOpacity, value);
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
            if (IsCheckedCovid19)
            {
                MessageBox.Show("Sorry, COVID-19 is not available at moment.", "Warning", MessageBoxButton.OK);
                return;
            }

            if (!this.IsCheckedVeryGood && !this.IsCheckedIndifferent && !IsCheckedCovid19Test &&
                !IsCheckedFluShot && !IsCheckedShingles && !IsCheckedPneumococcus && !IsCheckedOtherVaccination)
            {
                MessageBox.Show("Please make at least one selection", "Required.");
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
                reqEntity.Button1 = "New Case";
            }

            reqEntity.Button2 = string.Empty;

            if (this.IsCheckedIndifferent)
            {
                reqEntity.Button2 = "Follow Up";
            }

            reqEntity.Button3 = string.Empty;

            if (IsCheckedCovid19Test)
            {
                reqEntity.Button3 = "Covid19 Test";
            }

            reqEntity.Button4 = string.Empty;

            if (this.IsCheckedFluShot)
            {
                reqEntity.Button4 = "Flu Shot";
            }
            else if (this.IsCheckedShingles)
            {
                reqEntity.Button4 = "Shingles";
            }
            else if (this.IsCheckedPneumococcus)
            {
                reqEntity.Button4 = "Pneumococcus";
            }
            else if (this.IsCheckedOtherVaccination)
            {
                reqEntity.Button4 = "Other Vaccines";
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
                IsCheckedFluShot = false;
                IsCheckedShingles = false;
                IsCheckedPneumococcus = false;
                IsCheckedCovid19 = false;
                IsCheckedOtherVaccination = false;
            }
            else if (parameter == "1")
            {
                if (IsCheckedFluShot)
                    IsCheckedFluShot = IsCheckedShingles == false &&
                    IsCheckedPneumococcus == false &&
                    IsCheckedCovid19 == false &&
                    IsCheckedOtherVaccination == false;
            }
            else if (parameter == "2")
            {
                if (IsCheckedShingles)
                    IsCheckedShingles = IsCheckedFluShot == false &&
                    IsCheckedPneumococcus == false &&
                    IsCheckedCovid19 == false &&
                    IsCheckedOtherVaccination == false;
            }
            else if (parameter == "3")
            {
                if (IsCheckedPneumococcus)
                    IsCheckedPneumococcus = IsCheckedFluShot == false &&
                    IsCheckedShingles == false &&
                    IsCheckedCovid19 == false &&
                    IsCheckedOtherVaccination == false;
            }
            else if (parameter == "4")
            {
                if (IsCheckedCovid19)
                    IsCheckedCovid19 = IsCheckedFluShot == false &&
                    IsCheckedShingles == false &&
                    IsCheckedPneumococcus == false &&
                    IsCheckedOtherVaccination == false;
            }
            else if (parameter == "5")
            {
                if (IsCheckedOtherVaccination)
                    IsCheckedOtherVaccination = IsCheckedFluShot == false &&
                    IsCheckedShingles == false &&
                    IsCheckedCovid19 == false &&
                    IsCheckedPneumococcus == false;
            }
        }

        private void ExecuteVeryTerribleCheckedCommand(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                IsCheckedOtherVaccination = true;
                IsCheckedFluShot = false;
                IsCheckedShingles = false;
                IsCheckedPneumococcus = false;
                IsCheckedCovid19 = false;
            }
            else if (parameter == "1")
            {
                IsCheckedFluShot = true;
                IsCheckedShingles = false;
                IsCheckedPneumococcus = false;
                IsCheckedCovid19 = false;
                IsCheckedOtherVaccination = false;
            }
            else if (parameter == "2")
            {
                IsCheckedFluShot = false;
                IsCheckedShingles = true;
                IsCheckedPneumococcus = false;
                IsCheckedCovid19 = false;
                IsCheckedOtherVaccination = false;
            }
            else if (parameter == "3")
            {
                IsCheckedFluShot = false;
                IsCheckedShingles = false;
                IsCheckedPneumococcus = true;
                IsCheckedCovid19 = false;
                IsCheckedOtherVaccination = false;
            }
            else if (parameter == "4")
            {
                IsCheckedFluShot = false;
                IsCheckedShingles = false;
                IsCheckedPneumococcus = false;
                IsCheckedCovid19 = true;
                IsCheckedOtherVaccination = false;
            }
            else if (parameter == "5")
            {
                IsCheckedFluShot = false;
                IsCheckedShingles = false;
                IsCheckedPneumococcus = false;
                IsCheckedCovid19 = false;
                IsCheckedOtherVaccination = true;
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

            this.IsCheckedVeryGood = SelectedStoreUser.Btn1.Equals("New Case");
            this.IsCheckedIndifferent = SelectedStoreUser.Btn2.Equals("Follow Up");
            IsCheckedCovid19Test = !string.IsNullOrWhiteSpace(SelectedStoreUser.Btn3);
            this.IsCheckedVeryTerrible = !string.IsNullOrWhiteSpace(SelectedStoreUser.Btn4);

            if (SelectedStoreUser.Btn4.Equals("Vaccination") || SelectedStoreUser.Btn4.Equals("Flu Shot"))
            {
                this.IsCheckedFluShot = true;
            }
            else if (SelectedStoreUser.Btn4.Equals("Shingles"))
            {
                this.IsCheckedShingles = true;
            }
            else if (SelectedStoreUser.Btn4.Equals("Pneumococcus"))
            {
                this.IsCheckedPneumococcus = true;
            }
            else if (SelectedStoreUser.Btn4.Equals("Other Vaccines"))
            {
                this.IsCheckedOtherVaccination = true;
            }

            if (SelectedStoreUser.VersionForm != null && SelectedStoreUser.VersionForm.Count > 0)
            {
                IsVaccinesEditable = false;
                VaccinesOpacity = 0.2;
            }
        }

        private void SetUnsetProperties()
        {
            IsCheckedIndifferent = false;
            IsCheckedCovid19Test = false;
            IsCheckedVeryGood = false;
            IsCheckedVeryTerrible = false;
            IsCheckedFluShot = false;
            IsCheckedShingles = false;
            IsCheckedPneumococcus = false;
            IsCheckedCovid19 = false;
            IsCheckedOtherVaccination = false;
            IsUserTypeMobile = true;
            IsUserTypeNonMobile = false;
            IsSelectedStoreUser = false;
            IsVaccinesEditable = true;
            VaccinesOpacity = 1.0;
            SelectedStoreUser = new StoreUserEntity();
        }
    }
}
