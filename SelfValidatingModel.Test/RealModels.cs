using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SelfValidatingModel.Test
{
    public class NewBillingAddress : SelfValidatingModelBase
    {
        [Microsoft.Build.Framework.Required]
        public string UserId { get; set; }

        [Microsoft.Build.Framework.Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Microsoft.Build.Framework.Required]
        public string Address { get; set; }

        [Microsoft.Build.Framework.Required]
        public string City { get; set; }

        [Microsoft.Build.Framework.Required]
        [Display(Name = "State/Province")]
        public string StateProvince { get; set; }

        [Microsoft.Build.Framework.Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Microsoft.Build.Framework.Required]
        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [Microsoft.Build.Framework.Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("UserId", () => String.IsNullOrWhiteSpace(UserId), () => String.Format("UserId can't be empty"));
            AddValidationRule("FullName", () => String.IsNullOrWhiteSpace(FullName), () => "Full Name can't be empty");
            AddValidationRule("Address", () => String.IsNullOrWhiteSpace(Address), () => "Address can't be empty");
            AddValidationRule("City", () => String.IsNullOrWhiteSpace(City), () => "City can't be empty");
            AddValidationRule("StateProvince", () => String.IsNullOrWhiteSpace(StateProvince), () => "State/Province can't be empty");
            AddValidationRule("PostalCode", () => String.IsNullOrWhiteSpace(PostalCode), () => "Postal Code can't be empty");
            AddValidationRule("CountryCode", () => String.IsNullOrWhiteSpace(CountryCode), () => "Country Code can't be empty");
            AddValidationRule("CountryCode", () => CountryCode != null && CountryCode.Length != 2, () => "Country code must be 2 character length");
            AddValidationRule("PhoneNumber", () => String.IsNullOrWhiteSpace(PhoneNumber), () => "Phone Number can't be empty");

            // More specific to US (for possible tax purposes)
            var _usStates = new[]
                    {
                        "AK", "AL", "AR", "AS", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "FM", "GA", "GU", "HI", "IA", "ID", "IL", "IN",
                        "KS", "KY", "LA", "MA", "MD", "ME", "MH", "MI", "MN", "MO", "MP", "MS", "MT", "NC", "ND", "NE", "NH", "NJ", "NM",
                        "NV", "NY", "OH", "OK", "OR", "PA", "PR", "PW", "RI", "SC", "SD", "TN", "TX", "UM", "UT", "VA", "VI", "VT", "WA",
                        "WI", "WV", "WY"
                    };

            AddValidationRule("StateProvince", () => CountryCode == "US" && !_usStates.Contains(StateProvince), () => String.Format("Invalid US state 2 letter abbreviation \"{0}\"", StateProvince));
        }
    }

    public class CreditCardSettlementProviderInfo : SelfValidatingModelBase
    {
        public byte CreditCardSettlementProvider { get; set; }
        public string Token { get; set; }
        public string CreditCardType { get; set; }
        public string CreditCardMaskedNumber { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("CreditCardSettlementProvider", () => !new byte[] { 1 }.Contains(CreditCardSettlementProvider), () => "Invalid Credit Card Settlement Provider");
            AddValidationRule("Token", () => String.IsNullOrWhiteSpace(Token), () => "Token can't be empty");
            AddValidationRule("CreditCardType", () => String.IsNullOrWhiteSpace(CreditCardType), () => "Credit Card Type can't be empty");
            AddValidationRule("CreditCardMaskedNumber", () => String.IsNullOrWhiteSpace(CreditCardMaskedNumber), () => "Credit Card Masked Number can't be empty");
        }
    }

    public class NewBillingProfile : SelfValidatingModelBase
    {

        [Microsoft.Build.Framework.Required]
        public string UserId { get; set; }

        [Microsoft.Build.Framework.Required]
        [Display(Name = "Billing Profile Name")]
        public string BillingProfileName { get; set; }
        public NewBillingAddress NewBillingAddress { get; set; }
        public int? ExistingBillingAddressId { get; set; }
        public byte BillingType { get; set; }
        public CreditCardSettlementProviderInfo CreditCardSettlementProviderInfo { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("UserId", () => String.IsNullOrWhiteSpace(UserId), () => String.Format("UserId can't be empty"));
            AddValidationRule("BillingProfileName", () => String.IsNullOrWhiteSpace(BillingProfileName), () => "Payment Profile Name can't be empty");
            AddValidationRule("NewBillingAddress, ExistingBillingAddressId", () => NewBillingAddress == null && !ExistingBillingAddressId.HasValue, () => "Either a New or Existing Billing Address must be supplied");
            AddValidationRule("NewBillingAddress, ExistingBillingAddressId", () => NewBillingAddress != null && ExistingBillingAddressId.HasValue, () => "Only one of New or Existing Billing Address must be supplied");
            AddValidationRule("BillingType", () => !new[] { 1, 2 }.Contains(BillingType), () => "Invalid Billing Type");
            AddValidationRule("BillingType", () => BillingType == 0, () => "Billing Type must not be null");
            AddValidationRule("CreditCardSettlementProviderInfo", () => CreditCardSettlementProviderInfo == null, () => "CreditCardSettlementProviderInfo must not be null");
            //            AddSelfValidatingProperty("NewBillingAddress", NewBillingAddress);
            //            AddSelfValidatingProperty("CreditCardSettlementProviderInfo", CreditCardSettlementProviderInfo);
        }
    }

    public class ReplaceBillingProfile : SelfValidatingModelBase
    {
        [Microsoft.Build.Framework.Required]
        public string UserId { get; set; }

        public int BillingProfileId { get; set; }
        public int ReplaceWithBillingProfileId { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("UserId", () => String.IsNullOrWhiteSpace(UserId), () => String.Format("UserId can't be empty"));
            AddValidationRule("BillingProfileId", () => BillingProfileId == 0, () => "Billing Profile Id can't be empty");
            AddValidationRule("ReplaceWithBillingProfileId", () => ReplaceWithBillingProfileId == 0, () => "Replace With Billing Profile Id can't be empty");
        }
    }


    public class DeleteBillingProfile : SelfValidatingModelBase
    {
        [Microsoft.Build.Framework.Required]
        public string UserId { get; set; }

        public int BillingProfileId { get; set; }

        // Can be null - in this case if any projects are associated with this profile, the delete will fail
        public int? ReplaceWithBillingProfileId { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("UserId", () => String.IsNullOrWhiteSpace(UserId), () => String.Format("UserId can't be empty"));
            AddValidationRule("BillingProfileId", () => BillingProfileId == 0, () => "Billing Profile Id can't be empty");
        }
    }

    public class RenameBillingProfile : SelfValidatingModelBase
    {
        [Microsoft.Build.Framework.Required]
        public string UserId { get; set; }

        [Microsoft.Build.Framework.Required]
        public int BillingProfileId { get; set; }

        [Microsoft.Build.Framework.Required]
        [Display(Name = "Billing Profile Name")]
        public string NewBillingProfileName { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("UserId", () => String.IsNullOrWhiteSpace(UserId), () => String.Format("UserId can't be empty"));
            AddValidationRule("NewBillingProfileName", () => String.IsNullOrWhiteSpace(NewBillingProfileName), () => "New Billing Profile Name can't be empty");
            AddValidationRule("BillingProfileId", () => BillingProfileId == 0, () => "Billing Profile Id can't be empty");
        }

    }

    public class DeleteBillingAddress : SelfValidatingModelBase
    {
        [Microsoft.Build.Framework.Required]
        public string UserId { get; set; }

        public int BillingAddressId { get; set; }

        protected override void CreateValidationRules()
        {
            AddValidationRule("UserId", () => String.IsNullOrWhiteSpace(UserId), () => String.Format("UserId can't be empty"));
            AddValidationRule("BillingAddressId", () => BillingAddressId == 0, () => "Billing Address Id can't be empty");
        }
    }

}
