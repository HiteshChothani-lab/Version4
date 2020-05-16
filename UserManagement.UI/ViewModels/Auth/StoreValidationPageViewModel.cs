using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using UserManagement.Common.Constants;
using UserManagement.Common.Enums;
using UserManagement.Manager;
using UserManagement.UI.ItemModels;

namespace UserManagement.UI.ViewModels
{
	public class StoreValidationPageViewModel : ViewModelBase
	{
		private readonly IWindowsManager _windowsManager;
		public StoreValidationPageViewModel(IRegionManager regionManager, IWindowsManager windowsManager) : base(regionManager)
		{
			_windowsManager = windowsManager;

			this.SubmitCommand = new DelegateCommand(async () => await ExecuteSubmitCommand(),
				() => !string.IsNullOrEmpty(this.Username) && !string.IsNullOrEmpty(this.AccessCode) && this.CanExecuteSubmitCommand)
				.ObservesProperty(() => this.Username)
				.ObservesProperty(() => this.AccessCode)
				.ObservesProperty(() => this.CanExecuteSubmitCommand);

			FacilityTypes = new List<FacilityType>()
			{
				new FacilityType() { DisplayName = "Clinic", Value = "Clinic", ID = 1 },
				new FacilityType() { DisplayName = "Hospital", Value = "Hospital", ID = 2 },
				new FacilityType() { DisplayName = "Test Center", Value = "Test Center", ID = 3 }
			};
		}

		private string _username;
		public string Username
		{
			get => _username;
			set => SetProperty(ref _username, value);
		}

		private string _accessCode;
		public string AccessCode
		{
			get => _accessCode;
			set => SetProperty(ref _accessCode, value);
		}

		private bool _canExecuteSubmitCommand = true;
		public bool CanExecuteSubmitCommand
		{
			get => _canExecuteSubmitCommand;
			set => SetProperty(ref _canExecuteSubmitCommand, value);
		}

		private List<FacilityType> _facilityTypes;
		public List<FacilityType> FacilityTypes
		{
			get => _facilityTypes;
			set => SetProperty(ref _facilityTypes, value);
		}

		private FacilityType _selectedFacilityType;
		public FacilityType SelectedFacilityType
		{
			get => _selectedFacilityType;
			set => SetProperty(ref _selectedFacilityType, value);
		}

		public DelegateCommand SubmitCommand { get; private set; }

		private async Task ExecuteSubmitCommand()
		{
			this.CanExecuteSubmitCommand = false;

			var result = await _windowsManager.ValidateUser(new Entity.ValidateUserRequestEntity()
			{
				Username = Username,
				AccessCode = AccessCode
			});

            if (result.StatusCode == (int)GenericStatusValue.Success)
			{
				if (Convert.ToBoolean(result.Status))
				{
					var parameters = new NavigationParameters();
					parameters.Add(NavigationConstants.MasterStoreModel, new MasterStoreItemModel() { UserId = result.UserId });
					this.RegionManager.RequestNavigate("ContentRegion", ViewNames.RegisterMasterStore1Page, parameters);
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

			this.CanExecuteSubmitCommand = true;
		}

		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			base.OnNavigatedTo(navigationContext);

#if DEBUG
			this.Username = "Store4";
			this.AccessCode = "111111";
#endif
		}

		public class FacilityType
		{
			public int ID { get; set; }
			public string DisplayName { get; set; }
			public string Value { get; set; }
		}
	}
}
