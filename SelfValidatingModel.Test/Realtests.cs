using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Faker;
using Faker.Extensions;
using NUnit.Framework;

namespace SelfValidatingModel.Test
{
    [TestFixture]
    public class BillingAddressValidationTests
    {
        private Fake<NewBillingAddress> newBillingAddress;
        private Fake<DeleteBillingAddress> deleteBillingAddress;
        private Fake<NewBillingProfile> newBillingProfile;
        private Fake<DeleteBillingProfile> deleteBillingProfile;
        private Fake<RenameBillingProfile> renameBillingProfile;
        private Fake<ReplaceBillingProfile> replaceBillingProfile;
        private Fake<CreditCardSettlementProviderInfo> creditCardProviderInfo;

        [TestFixtureSetUp]
        public void SetupTextFixture()
        {
            var randomNumberGen = new Random();
            var usStates = new[]
            {
                "AK", "AL", "AR", "AS", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "FM", "GA", "GU", "HI", "IA", "ID",
                "IL", "IN",
                "KS", "KY", "LA", "MA", "MD", "ME", "MH", "MI", "MN", "MO", "MP", "MS", "MT", "NC", "ND", "NE", "NH",
                "NJ", "NM",
                "NV", "NY", "OH", "OK", "OR", "PA", "PR", "PW", "RI", "SC", "SD", "TN", "TX", "UM", "UT", "VA", "VI",
                "VT", "WA",
                "WI", "WV", "WY"
            };


            newBillingAddress = new Fake<NewBillingAddress>();
            newBillingAddress.SetProperty(x => x.CountryCode,
                () => Convert.ToChar(randomNumberGen.Next((byte)'A', (byte)'Z')).ToString(CultureInfo.InvariantCulture) + Convert.ToChar(randomNumberGen.Next((byte)'A', (byte)'Z')).ToString(CultureInfo.InvariantCulture));

            deleteBillingAddress = new Fake<DeleteBillingAddress>();
            deleteBillingAddress.SetProperty(x => x.UserId, () => randomNumberGen.Next(1, Int32.MaxValue).ToString());
            deleteBillingAddress.SetProperty(x => x.BillingAddressId, () => randomNumberGen.Next(1, Int32.MaxValue));

            newBillingProfile = new Fake<NewBillingProfile>();
            newBillingProfile.SetProperty(x => x.BillingType, () => (byte)randomNumberGen.Next(1, 2));
            newBillingProfile.SetProperty(x => x.ExistingBillingAddressId, () => null);
            newBillingProfile.SetProperty(x => x.NewBillingAddress, () => newBillingAddress.Generate());
            newBillingProfile.SetProperty(x => x.CreditCardSettlementProviderInfo, () => creditCardProviderInfo.Generate());

            deleteBillingProfile = new Fake<DeleteBillingProfile>();
            deleteBillingProfile.SetProperty(x => x.UserId, () => randomNumberGen.Next(1, Int32.MaxValue).ToString(CultureInfo.InvariantCulture));
            deleteBillingProfile.SetProperty(x => x.BillingProfileId, () => randomNumberGen.Next(1, Int32.MaxValue));
            deleteBillingProfile.SetProperty(x => x.ReplaceWithBillingProfileId, () => randomNumberGen.Next(0, 1) == 0 ? null : (int?)randomNumberGen.Next(1, Int32.MaxValue));

            renameBillingProfile = new Fake<RenameBillingProfile>();
            renameBillingProfile.SetProperty(x => x.UserId, () => randomNumberGen.Next(1, Int32.MaxValue).ToString(CultureInfo.InvariantCulture));
            renameBillingProfile.SetProperty(x => x.BillingProfileId, () => randomNumberGen.Next(1, Int32.MaxValue));

            replaceBillingProfile = new Fake<ReplaceBillingProfile>();
            replaceBillingProfile.SetProperty(x => x.UserId, () => randomNumberGen.Next(1, Int32.MaxValue).ToString(CultureInfo.InvariantCulture));
            replaceBillingProfile.SetProperty(x => x.BillingProfileId, () => randomNumberGen.Next(1, Int32.MaxValue));
            replaceBillingProfile.SetProperty(x => x.ReplaceWithBillingProfileId, () => randomNumberGen.Next(1, Int32.MaxValue));

            creditCardProviderInfo = new Fake<CreditCardSettlementProviderInfo>();
            creditCardProviderInfo.SetProperty(x => x.CreditCardSettlementProvider, () => (byte)1);
        }

        [Test]
        public void Validate_NewBillingAddress_Model()
        {
            var newBillingAddressData = newBillingAddress.Generate();

            // Make sure that the country is not US and it is 2 character length:
            if (newBillingAddressData.CountryCode == "US")
                newBillingAddressData.CountryCode = "CA";

            Assert.IsTrue(newBillingAddressData.IsValid);
            Assert.IsNull(newBillingAddressData.Exception);

            // Now let's make all wrong and then count all errors:
            newBillingAddressData.Address = "";
            newBillingAddressData.City = "";
            newBillingAddressData.CountryCode = ""; // 2 errors
            newBillingAddressData.FullName = "";
            newBillingAddressData.PhoneNumber = "";
            newBillingAddressData.PostalCode = "";
            newBillingAddressData.StateProvince = "";
            newBillingAddressData.UserId = "";

            Assert.IsFalse(newBillingAddressData.IsValid);
            Assert.IsNotNull(newBillingAddressData.Exception);
            Assert.That(newBillingAddressData.Exception.Data.Count == 8);
            Assert.That(((IList<string>)(newBillingAddressData.Exception.Data["CountryCode"])).Count == 2);
        }

        [Test]
        public void Validate_DeleteBillingAddress_Model()
        {
            var deleteBillingAddressData = deleteBillingAddress.Generate();

            Assert.IsTrue(deleteBillingAddressData.IsValid);
            Assert.IsNull(deleteBillingAddressData.Exception);

            // Now let's make all wrong and then count all errors:
            deleteBillingAddressData.UserId = "";
            deleteBillingAddressData.BillingAddressId = 0;

            Assert.IsFalse(deleteBillingAddressData.IsValid);
            Assert.IsNotNull(deleteBillingAddressData.Exception);
            Assert.That(deleteBillingAddressData.Exception.Data.Count == 2);
        }

        [Test]
        public void Validate_RenameBillingProfile_Model()
        {
            var renameBillingProfileData = renameBillingProfile.Generate();
            Assert.IsTrue(renameBillingProfileData.IsValid);
            Assert.IsNull(renameBillingProfileData.Exception);

            // Now let's make all wrong and then count all errors:
            renameBillingProfileData.UserId = "";
            renameBillingProfileData.BillingProfileId = 0;
            renameBillingProfileData.NewBillingProfileName = null;

            Assert.IsFalse(renameBillingProfileData.IsValid);
            Assert.IsNotNull(renameBillingProfileData.Exception);
            Assert.That(renameBillingProfileData.Exception.Data.Count == 3);
        }

        [Test]
        public void Validate_DeleteBillingProfile_Model()
        {
            var deleteBillingProfileData = deleteBillingProfile.Generate();
            Assert.IsTrue(deleteBillingProfileData.IsValid);
            Assert.IsNull(deleteBillingProfileData.Exception);

            // Now let's make all wrong and then count all errors:
            deleteBillingProfileData.UserId = "";
            deleteBillingProfileData.BillingProfileId = 0;
            deleteBillingProfileData.ReplaceWithBillingProfileId = null;

            Assert.IsFalse(deleteBillingProfileData.IsValid);
            Assert.IsNotNull(deleteBillingProfileData.Exception);
            Assert.That(deleteBillingProfileData.Exception.Data.Count == 2); // RelaceWith isn't validated
        }

        [Test]
        public void Validate_ReplaceBillingProfile_Model()
        {
            var model = replaceBillingProfile.Generate();
            Assert.IsTrue(model.IsValid);
            Assert.IsNull(model.Exception);

            // Now let's make all wrong and then count all errors:
            model.UserId = "";
            model.BillingProfileId = 0;
            model.ReplaceWithBillingProfileId = 0;

            Assert.IsFalse(model.IsValid);
            Assert.IsNotNull(model.Exception);
            Assert.That(model.Exception.Data.Count == 3);
        }

        [Test]
        public void Validate_NewBillingProfile_Model()
        {
            var model = newBillingProfile.Generate();
            Assert.IsTrue(model.IsValid);
            Assert.IsNull(model.Exception);
            Assert.IsTrue(model.CreditCardSettlementProviderInfo.IsValid);
            Assert.IsNull(model.CreditCardSettlementProviderInfo.Exception);
            Assert.IsTrue(model.NewBillingAddress.IsValid);
            Assert.IsNull(model.NewBillingAddress.Exception);


            // Now let's make all wrong and then count all errors:
            model.UserId = ""; //1
            model.BillingProfileName = ""; //2
            model.BillingType = 0; //3 (2 errors)
            model.CreditCardSettlementProviderInfo.CreditCardMaskedNumber = ""; //4
            model.CreditCardSettlementProviderInfo.CreditCardSettlementProvider = 0; //5
            model.CreditCardSettlementProviderInfo.CreditCardType = ""; //6
            model.CreditCardSettlementProviderInfo.Token = ""; //7
            model.ExistingBillingAddressId = new Random().Next(1, Int32.MaxValue); // both not null => 8

            Assert.IsFalse(model.IsValid);
            Assert.IsNotNull(model.Exception);
            Assert.That(model.Exception.Data.Count == 8);

            // Check if we remove CC Settlement Info -- -4 + 1 = 5 errors
            model.CreditCardSettlementProviderInfo = null;
            Assert.IsFalse(model.IsValid);
            Assert.IsNotNull(model.Exception);
            Assert.That(model.Exception.Data.Count == 5);

            // make both address fields null (same as before error)
            model.NewBillingAddress = null;
            model.ExistingBillingAddressId = null;
            Assert.IsFalse(model.IsValid);
            Assert.IsNotNull(model.Exception);
            Assert.That(model.Exception.Data.Count == 5);



        }
    }

}
