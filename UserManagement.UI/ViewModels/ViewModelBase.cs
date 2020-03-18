using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.UI.ViewModels
{
    public class ViewModelBase : BindableBase, INavigationAware
    {
        protected IRegionNavigationService RegionNavigationService { get; private set; }
        protected IRegionManager RegionManager { get; private set; }

        public ViewModelBase(IRegionManager regionManager)
        {
            RegionManager = regionManager;
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            RegionNavigationService = navigationContext.NavigationService;
        }
    }
}
