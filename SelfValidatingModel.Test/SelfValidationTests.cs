using System.Collections.Generic;
using NUnit.Framework;

namespace SelfValidatingModel.Test
{
    public class SimpleModel : SelfValidatingModelBase
    {
        public string Property1 { get; set; }

        public int Property2 { get; set; }
        
        protected override void CreateValidationRules()
        {
            
            AddValidationRule("Property1", () => string.IsNullOrWhiteSpace(Property1), () => "Property1 should not be empty");
            AddValidationRule("Property2", () => Property2 == 0, () => "Property2 must not be 0");
        }
    }

    public class SimpleModelWithConstructor : SelfValidatingModelBase
    {
        public SimpleModelWithConstructor(string property1) : this()
        {
            Property1 = property1;
        }


        protected SimpleModelWithConstructor()
        {

        }

        public string Property1 { get; set; }

        public int Property2 { get; set; }

        protected override void CreateValidationRules()
        {

            AddValidationRule("Property1", () => string.IsNullOrWhiteSpace(Property1), () => "Property1 should not be empty");
            AddValidationRule("Property2", () => Property2 == 0, () => "Property2 must not be 0");
        }
    }

    [TestFixture]
    public class SelfValidationTests
    {
        [Test(Description = "Simple one layer model validation")]
        public void Should_Validate_Simple_Model_Pass_Or_Fail_Depending_On_Property_Values()
        {

            // Make it pass
            var _model = new SimpleModel {Property1 = "Something to pass validation", Property2 = 10};

            Assert.IsTrue(_model.IsValid);
            Assert.IsNull(_model.Exception);

            // make it fail with one error
            _model.Property1 = "";
            Assert.IsFalse(_model.IsValid);
            Assert.IsNotNull(_model.Exception);
            Assert.IsNotNull(_model.Exception.Data["Property1"]);
            Assert.IsNull(_model.Exception.Data["Property2"]);
            Assert.That(((IList<string>)(_model.Exception.Data["Property1"])).Count == 1);

            // make it fail with two errors
            _model.Property2 = 0;
            Assert.IsFalse(_model.IsValid);
            Assert.IsNotNull(_model.Exception);
            Assert.IsNotNull(_model.Exception.Data["Property1"]);
            Assert.IsNotNull(_model.Exception.Data["Property2"]);
            Assert.That(((IList<string>)(_model.Exception.Data["Property1"])).Count == 1);
            Assert.That(((IList<string>)(_model.Exception.Data["Property2"])).Count == 1);

        }

        [Test(Description = "Multiple layer model validation")]
        public void Should_Do_Everything_The_Same_As_Without_Constructor()
        {
            // Make it pass
            var _model = new SimpleModelWithConstructor("Something to pass validation") { Property2 = 10 };

            Assert.IsTrue(_model.IsValid);
            Assert.IsNull(_model.Exception);

            // make it fail with one error
            _model.Property1 = "";
            Assert.IsFalse(_model.IsValid);
            Assert.IsNotNull(_model.Exception);
            Assert.IsNotNull(_model.Exception.Data["Property1"]);
            Assert.IsNull(_model.Exception.Data["Property2"]);
            Assert.That(((IList<string>)(_model.Exception.Data["Property1"])).Count == 1);

            // make it fail with two errors
            _model.Property2 = 0;
            Assert.IsFalse(_model.IsValid);
            Assert.IsNotNull(_model.Exception);
            Assert.IsNotNull(_model.Exception.Data["Property1"]);
            Assert.IsNotNull(_model.Exception.Data["Property2"]);
            Assert.That(((IList<string>)(_model.Exception.Data["Property1"])).Count == 1);
            Assert.That(((IList<string>)(_model.Exception.Data["Property2"])).Count == 1);
        }


    }
}
