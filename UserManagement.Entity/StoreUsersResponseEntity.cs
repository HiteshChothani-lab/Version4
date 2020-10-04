using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using UserManagement.Common.Constants;

namespace UserManagement.Entity
{
    public class StoreUsersResponseEntity : EntityBase
    {
        public List<StoreUserEntity> Data { get; set; }
        public string Status { get; set; }
        public new string Messagee { get; set; }
    }

    public class ArchieveStoreUsersResponseEntity : EntityBase
    {
        public long ArchieveSize { get; set; }
        public List<StoreUserEntity> Data { get; set; }
        public string Status { get; set; }
        public new string Messagee { get; set; }
    }

    public class StoreUserEntity : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string SuperMasterId { get; set; }
        public string MasterStoreId { get; set; }
        public string StoreId { get; set; }
        public string OrderId { get; set; }
        public string Btn1 { get; set; }
        public string Btn2 { get; set; }
        public string Btn3 { get; set; }
        public string Btn4 { get; set; }
        public string Btn5 { get; set; }
        public string BtnAB { get; set; }
        public string Note { get; set; }
        public string NoteColor { get; set; }
        public string Tag { get; set; }
        public string OrphanStatus { get; set; }
        public string RecentStatus { get; set; }
        public string DeliverOrderStatus { get; set; }
        public string IdrStatus { get; set; }
        public string AccountBlockStatus { get; set; }
        public string BadExpDesc { get; set; }
        public string Age { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string CountryCode { get; set; }
        public string Mobile { get; set; }
        public string Status { get; set; }
        public string HomePhone { get; set; }
        public string PostalCode { get; set; }
        public string RegisterType { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string TimeDifference { get; set; }
        public string ExpressTime { get; set; }
        public string RegType { get; set; }
        public int Covid19 { get; set; }
        public string RoomNumber { get; set; }
        public List<VersionForm> VersionForm { get; set; }

        public bool IsFlagSet
        {
            get { return RecentStatus == "1"; }
        }

        public bool IsZipCode
        {
            get { return !string.IsNullOrWhiteSpace(this.PostalCode) && Regex.IsMatch(this.PostalCode, "^[0-9]{5}$"); }
        }

        public string Column1TextDisplay
        {
            get => $"{this.Btn1} & {this.Btn2}".Trim().Trim('&');
        }

        public string Column1AgeDisplayImage
        {
            get
            {
                string path = string.Empty;

                if (int.TryParse(this.Age, out int age))
                {
                    if (age <= 16)
                    {
                        path = "/UserManagement.UI;component/Assets/icon_kid.png";
                    }
                    else if (age <= 64)
                    {
                        path = "/UserManagement.UI;component/Assets/icon_adult.png";
                    }
                    else
                    {
                        path = "/UserManagement.UI;component/Assets/icon_old.png";
                    }
                }

                return path;
            }
        }

        public string Column2FullNameDisplay
        {
            get
            {
                var str = $"{this.Firstname} {this.Lastname}".Trim(' ');
                return str.Length > 20 ? str.Insert(20, Environment.NewLine) : str;
            }
        }

        public string Column2IncompleteDisplay
        {
            get => string.IsNullOrEmpty(this.HomePhone) && string.IsNullOrEmpty(this.Mobile) ? "Incomplete" : string.Empty;
        }

        public string Column2DisplayRoomNumebr { get => string.IsNullOrWhiteSpace(RoomNumber) || "0".Equals(RoomNumber) ? "(NONE)" : $"({RoomNumber})"; }

        public string Column2IDRDisplayImage
        {
            get => this.IdrStatus == "1" && string.IsNullOrWhiteSpace(this.Mobile) ? "/UserManagement.UI;component/Assets/icon_check.png" : string.Empty;
        }

        public string Column2NewOrIdrDisplay
        {
            get => this.IdrStatus == "0" && string.IsNullOrWhiteSpace(this.Mobile) ? this.RegisterType == "first" ? "NEW" : "IDR" : string.Empty;
        }

        public string Column3PostalCode
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Mobile))
                    return this.Mobile;
                else if (!string.IsNullOrEmpty(this.PostalCode))
                {
                    return this.IsZipCode ? $"Z {this.PostalCode}" : this.PostalCode;
                }
                else
                    return "Override";
            }
        }

        private string _column3FontColor = ColorNames.DarkerBlue;
        public string Column3FontColor
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Mobile))
                {
                    _column3FontColor = this.IsFlagSet ? ColorNames.White : ColorNames.NavyBlue;
                }
                else if (!string.IsNullOrEmpty(this.PostalCode))
                {
                    _column3FontColor = this.IsFlagSet ? ColorNames.White : ColorNames.DarkerBlue;
                }
                else
                {
                    _column3FontColor = ColorNames.Red;
                }
                return _column3FontColor;
            }
            set
            {
                _column3FontColor = value;
                OnPropertyRaised("Column3FontColor");
            }
        }

        public string Column2RowColor { get; set; } = ColorNames.Green;

        public string Column3RowColor
        {
            get { return this.IsFlagSet ? ColorNames.Purple : ColorNames.White; }
        }

        public bool MobileNumberVisibility
        {
            get => !string.IsNullOrWhiteSpace(this.Mobile);
        }

        public bool HomePhoneNumberVisibility
        {
            get => !MobileNumberVisibility;
        }

        /*
         * [early - Yellow] [ready - Blue] [late - Red]
         */
        public string TimeDifferenceColor
        {
            get
            {
                if (RegType.Equals("Express") && Config.MasterStore.FacilityType.Equals("Clinic"))
                {
                    if ("late".Equals(TimeDifference))
                    {
                        return ColorNames.Red;
                    }
                    else if ("ready".Equals(TimeDifference))
                    {
                        return ColorNames.Blue;
                    }
                }
                return ColorNames.Yellow;
            }
        }

        public bool IsYellowNeedleVisible
        {
            get => Btn4.Contains("Shingles")
                 || Btn4.Contains("Pneumococcus")
                 || Btn4.Contains("Other Vaccines");
        }

        public bool IsGreenNeedleVisible
        {
            get => Btn4.Contains("Flu Shot") || Btn4.Contains("Vaccination");
        }

        public bool IsRedNeedleVisible
        {
            get => !string.IsNullOrWhiteSpace(Btn3);
        }

        public bool IsCovid19Visible { get => Covid19 >= 7; }

        public string RoomNumberColor
        {
            get { return string.IsNullOrWhiteSpace(RoomNumber) || "0".Equals(RoomNumber) ? ColorNames.Red : ColorNames.Purple; }
        }

        public bool IsItemMovable
        {
            get => Column2RowColor != ColorNames.Yellow;
        }

        public bool IsFluShotAdded { get => Btn4.Contains("Flu Shot"); }

        public string Column1StatusImage
        {
            get => IsFluShotAdded ? this.VersionForm != null && this.VersionForm.Count > 0 ? "/UserManagement.UI;component/Assets/status_complete.png" : "/UserManagement.UI;component/Assets/status_incomplete.png" : string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }

    public class VersionForm
    {
        public string Id { get; set; }
        public string SurveyId { get; set; }
        public string Type { get; set; }
        public string FormType { get; set; }
        public string Title { get; set; }
        public List<Answer> Answers { get; set; }
    }

    public class Answer
    {
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public string SurveyId { get; set; }
        public string AnsweredText { get; set; }
    }
}
