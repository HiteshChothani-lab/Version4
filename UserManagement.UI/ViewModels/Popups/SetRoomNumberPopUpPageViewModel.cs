using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using UserManagement.Common.Constants;
using UserManagement.Common.Enums;
using UserManagement.Entity;
using UserManagement.Manager;
using UserManagement.UI.Events;
using UserManagement.UI.ItemModels;

namespace UserManagement.UI.ViewModels
{
    public class SetRoomNumberPopUpPageViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowsManager _windowsManager;

        public SetRoomNumberPopUpPageViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IWindowsManager windowsManager) : base(regionManager)
        {
            _eventAggregator = eventAggregator;
            _windowsManager = windowsManager;

            CancelCommand = new DelegateCommand(ExecuteCancelCommand);
            ClearCommand = new DelegateCommand(() => ExecuteClearCommand());
            SubmitCommand = new DelegateCommand(async () => await ExecuteSubmitCommand());

            NumberClickCommand = new DelegateCommand<string>(ExecuteNumberClickCommand);

        }
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            if (navigationContext.Parameters[NavigationConstants.SelectedStoreUser] is StoreUserEntity selectedStoreUser)
                SelectedStoreUser = selectedStoreUser;

            if (navigationContext.Parameters[NavigationConstants.ArchieveStoreUsersRoomNumber] is List<string> archieveStoreUsersRoomNumber)
                ArchieveStoreUsersRoomNumber = archieveStoreUsersRoomNumber;

            Populatefields();
        }

        private void Populatefields()
        {
            var room = SelectedStoreUser.RoomNumber;
            FirstName = SelectedStoreUser.Firstname;
            LastName = SelectedStoreUser.Lastname;
            RoomNumber = string.IsNullOrWhiteSpace(room) || "0".Equals(room) ? string.Empty : room;
            UserFullName = FirstName + " " + LastName;
        }

        public StoreUserEntity SelectedStoreUser { get; set; }

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
        private string _userFullName;
        public string UserFullName
        {
            get => _userFullName;
            set => SetProperty(ref _userFullName, value);
        }
        private string _roomNumber;
        public string RoomNumber
        {
            get => string.IsNullOrEmpty(_roomNumber) ? "" : _roomNumber;
            set => SetProperty(ref _roomNumber, value);
        }

        public List<string> ArchieveStoreUsersRoomNumber { get; private set; }

        private void ExecuteClearCommand()
        {
            RoomNumber = "";
        }

        private void ExecuteNumberClickCommand(string num)
        {
            RoomNumber = $"{RoomNumber}{num}";
        }

        private async Task ExecuteSubmitCommand()
        {
            if (string.IsNullOrEmpty(RoomNumber))
                MessageBox.Show("Room number is required.", "Required");

            else if (SelectedStoreUser.RoomNumber == RoomNumber)
                MessageBox.Show($"Room Number [{RoomNumber}] is already associated with current patient.", "ALERT");

            else if (ArchieveStoreUsersRoomNumber != null && ArchieveStoreUsersRoomNumber.Any(a => a == RoomNumber))
                MessageBox.Show($"Room Number [{RoomNumber}] is already associated with another patient." +
                    $"{Environment.NewLine}{Environment.NewLine}Please select the other room number.", "NOT AVAILABLE",
                    MessageBoxButton.OK);

            else
            {
                var reqEntity = new ManageUserRequestEntity()
                {
                    RoomNumber = this.RoomNumber,
                    Id = SelectedStoreUser.Id,
                    MasterStoreId = SelectedStoreUser.MasterStoreId,
                };

                var result = await _windowsManager.SetRoomNumber(reqEntity);
                switch (result.StatusCode)
                {
                    case (int)GenericStatusValue.Success when Convert.ToBoolean(result.Status):

                        RegionNavigationService.Journal.Clear();
                        _eventAggregator.GetEvent<EditStoreUserSubmitEvent>().Publish(new EditStoreUserItemModel());
                        break;

                    case (int)GenericStatusValue.Success:
                        MessageBox.Show(result.Message, "Unsuccessful");
                        break;

                    case (int)GenericStatusValue.NoInternetConnection:
                        MessageBox.Show(MessageBoxMessage.NoInternetConnection, "Unsuccessful");
                        break;

                    case (int)GenericStatusValue.HasErrorMessage:
                        MessageBox.Show(((EntityBase)result).Message, "Unsuccessful");
                        break;

                    default:
                        MessageBox.Show(MessageBoxMessage.UnknownErorr, "Unsuccessful");
                        break;
                }
            }
        }

        private void ExecuteCancelCommand()
        {
            RegionNavigationService.Journal.Clear();
            _eventAggregator.GetEvent<SetRoomNumberSubmitEvent>().Publish(null);
            //SetUnsetProperties();
        }
        public DelegateCommand ClearCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand SubmitCommand { get; private set; }
        public DelegateCommand<string> NumberClickCommand { get; private set; }
    }
}
