namespace SelfValidatingModel.Samples
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
        public SimpleModelWithConstructor(string property1)
            : this()
        {
            Property1 = property1;
            Property3 = new SimpleModel() {Property1 = "kdjkdjkdj", Property2 = 10};
        }


        protected SimpleModelWithConstructor()
        {

        }

        public string Property1 { get; set; }

        public int Property2 { get; set; }
        public SimpleModel Property3 { get; set; }

        protected override void CreateValidationRules()
        {

            AddValidationRule("Property1", () => string.IsNullOrWhiteSpace(Property1), () => "Property1 should not be empty");
            AddValidationRule("Property2", () => Property2 == 0, () => "Property2 must not be 0");
            AddValidationRule("Property3", () => Property3 == null, () => "Property3 must not be null");
        }
    }

}
