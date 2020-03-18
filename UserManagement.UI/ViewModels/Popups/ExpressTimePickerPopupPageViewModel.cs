using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Windows;
using UserManagement.Common.Constants;
using UserManagement.Manager;
using UserManagement.UI.Events;
using UserManagement.UI.ItemModels;

namespace UserManagement.UI.ViewModels
{
    public class ExpressTimePickerPopupPageViewModel : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowsManager _windowsManager;

        public ExpressTimePickerPopupPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IWindowsManager windowsManager) : base(regionManager)
        {
            _eventAggregator = eventAggregator;
            _windowsManager = windowsManager;

            this.CancelCommand = new DelegateCommand(() => ExecuteCancelCommand());
            this.SubmitCommand = new DelegateCommand(() => ExecuteSubmitCommand());
            this.DayCheckedCommand = new DelegateCommand<string>((data) => ExecuteDayCheckedCommand(data));

            Hours = new List<string>();
            Minutes = new List<string>();

            for (int i = 0; i <= 60; i++)
            {
                if (i > 0 && i < 13)
                {
                    if (i.ToString().Length == 1)
                        Hours.Add($"0{i}");
                    else
                        Hours.Add($"{i}");
                }

                if (i.ToString().Length == 1)
                    Minutes.Add($"0{i}");
                else
                    Minutes.Add($"{i}");
            }

            TimeMode = new List<string> { "AM", "PM" };
        }

        private List<string> _hours;
        public List<string> Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }

        private string _hour;
        public string Hour
        {
            get => _hour;
            set { SetProperty(ref _hour, value); SetFinalTime(); }
        }

        private List<string> _minutes;
        public List<string> Minutes
        {
            get => _minutes;
            set => SetProperty(ref _minutes, value);
        }

        private string _minute;
        public string Minute
        {
            get => _minute;
            set { SetProperty(ref _minute, value); SetFinalTime(); }
        }

        private List<string> _timeMode;
        public List<string> TimeMode
        {
            get => _timeMode;
            set => SetProperty(ref _timeMode, value);
        }

        private string _mode;
        public string Mode
        {
            get => _mode;
            set { SetProperty(ref _mode, value); SetFinalTime(); }
        }

        private string _finalTime;
        public string FinalTime
        {
            get => _finalTime;
            set => SetProperty(ref _finalTime, value);
        }

        private string _finalTimeColor = ColorNames.BrightOrange;
        public string FinalTimeColor
        {
            get => _finalTimeColor;
            set => SetProperty(ref _finalTimeColor, value);
        }

        private bool _isCheckedCurrentDay;
        public bool IsCheckedCurrentDay
        {
            get => _isCheckedCurrentDay;
            set
            {
                if (!this.IsCheckedNextDay && !value) return;
                SetProperty(ref _isCheckedCurrentDay, value);
            }
        }

        private bool _isCheckedNextDay;
        public bool IsCheckedNextDay
        {
            get => _isCheckedNextDay;
            set
            {
                if (!this.IsCheckedCurrentDay && !value) return;
                SetProperty(ref _isCheckedNextDay, value);
            }
        }

        private string _currentDay;
        public string CurrentDay
        {
            get => _currentDay;
            set => SetProperty(ref _currentDay, value);
        }

        private string _nextDay;
        public string NextDay
        {
            get => _nextDay;
            set => SetProperty(ref _nextDay, value);
        }


        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand SubmitCommand { get; private set; }
        public DelegateCommand<string> DayCheckedCommand { get; private set; }

        private void ExecuteCancelCommand()
        {
            this.RegionNavigationService.Journal.Clear();
            _eventAggregator.GetEvent<ExpressTimeSubmitEvent>().Publish(null);
        }

        private void ExecuteSubmitCommand()
        {
            try
            {
                string fdate = this.FinalTime.Replace(" (Midnight)", string.Empty).Replace(" (Noon)", string.Empty);
                DateTime oDate = DateTime.ParseExact(fdate, "yyyy-MM-dd hh:mm tt", null);

                _eventAggregator.GetEvent<ExpressTimeSubmitEvent>().Publish(new ExpressTimeItemModel
                {
                    Hour = this.Hour,
                    Minute = this.Minute,
                    Mode = this.Mode,
                    FinalTime = oDate.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DateTime Parse Error");
            }
        }

        private void ExecuteDayCheckedCommand(string parameter)
        {
            if ("CurrentDay".Equals(parameter))
            {
                this.IsCheckedCurrentDay = true;
                this.IsCheckedNextDay = false;
            }
            else if ("NextDay".Equals(parameter))
            {
                this.IsCheckedNextDay = true;
                this.IsCheckedCurrentDay = false;
            }
            SetFinalTime();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            SetUnsetProperties();
        }

        private void SetUnsetProperties()
        {
            this.Hour = DateTime.Now.ToString("hh");
            this.Minute = DateTime.Now.ToString("mm");
            this.Mode = DateTime.Now.ToString("tt");
            this.CurrentDay = DateTime.Now.ToString("yyyy-MM-dd");
            this.NextDay = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            this.IsCheckedCurrentDay = true;
            this.IsCheckedNextDay = false;
            SetFinalTime();
        }

        private void SetFinalTime()
        {
            if (IsCheckedCurrentDay)
                this.FinalTime = $"{this.CurrentDay} {Hour}:{Minute} {Mode}";
            else if (IsCheckedNextDay)
                this.FinalTime = $"{this.NextDay} {Hour}:{Minute} {Mode}";

            if ("12".Equals(Hour) && "00".Equals(Minute))
            {
                this.FinalTime += "AM".Equals(Mode) ? " (Midnight)" : " (Noon)";
            }

            this.FinalTimeColor = "AM".Equals(Mode) ? ColorNames.BrightOrange : ColorNames.NavyBlue;
        }
    }
}
