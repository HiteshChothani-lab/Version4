using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Common.Constants;
using UserManagement.UI.ItemModels;

namespace UserManagement.UI.ViewModels
{
	public class RegisterMasterStore2PageViewModel : ViewModelBase
	{
		public RegisterMasterStore2PageViewModel(IRegionManager regionManager) : base(regionManager)
		{
			BackCommand = new DelegateCommand(() => ExecuteBackCommand());
			SubmitCommand = new DelegateCommand(() => ExecuteSubmitCommand(),
				() => !string.IsNullOrEmpty(StoreName) && !string.IsNullOrEmpty(PhoneNumber) &&
				!string.IsNullOrEmpty(Street) && !string.IsNullOrEmpty(Address))
				.ObservesProperty(() => StoreName)
				.ObservesProperty(() => SelectedFacilityType)
				.ObservesProperty(() => PhoneNumber)
				.ObservesProperty(() => Street)
				.ObservesProperty(() => Address);

			FacilityTypes = new List<FacilityType>()
			{
				new FacilityType() { DisplayName = "Clinic", Value = "Clinic", ID = 1 },
				new FacilityType() { DisplayName = "Hospital", Value = "Hospital", ID = 2 },
				new FacilityType() { DisplayName = "Test Center", Value = "Test Center", ID = 3 }
			};
		}

		private MasterStoreItemModel _masterStore;
		public MasterStoreItemModel MasterStore
		{
			get => _masterStore;
			set => SetProperty(ref _masterStore, value);
		}

		private string _storeName = string.Empty;
		public string StoreName
		{
			get => _storeName;
			set => SetProperty(ref _storeName, value);
		}

		private string _phoneNumber = string.Empty;
		public string PhoneNumber
		{
			get => _phoneNumber;
			set => SetProperty(ref _phoneNumber, value);
		}

		private string _street = string.Empty;
		public string Street
		{
			get => _street;
			set => SetProperty(ref _street, value);
		}

		private string _address = string.Empty;
		public string Address
		{
			get => _address;
			set => SetProperty(ref _address, value);
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

		public DelegateCommand BackCommand { get; private set; }
		public DelegateCommand SubmitCommand { get; private set; }

		private void ExecuteBackCommand()
		{
			RegionNavigationService.Journal.CurrentEntry.Parameters.Add(NavigationConstants.MasterStoreModel, MasterStore);
			RegionNavigationService.Journal.GoBack();
		}

		private void ExecuteSubmitCommand()
		{
			MasterStore.StoreName = StoreName;
			MasterStore.FacilityType = SelectedFacilityType.Value;
			MasterStore.PhoneNumber = PhoneNumber;
			MasterStore.Address = Address;
			MasterStore.Street = Street;

			var parameters = new NavigationParameters
			{
				{ NavigationConstants.MasterStoreModel, MasterStore }
			};
			RegionManager.RequestNavigate("ContentRegion", ViewNames.RegisterMasterStoreReviewPage, parameters);
		}

		public override void OnNavigatedTo(NavigationContext navigationContext)
		{
			base.OnNavigatedTo(navigationContext);

			if (navigationContext.Parameters.Any(x => x.Key == NavigationConstants.MasterStoreModel))
			{
				MasterStore = navigationContext.Parameters[NavigationConstants.MasterStoreModel] as MasterStoreItemModel;
			}
#if DEBUG
			StoreName = "Mac77";
			PhoneNumber = "7777777777";
			Street = "700";
			Address = "Street77";
#endif
		}
	}
}
