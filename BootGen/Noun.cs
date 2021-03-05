namespace BootGen
{
    public class Noun
    {
        private static Pluralize.NET.Pluralizer Pluralizer = new Pluralize.NET.Pluralizer();
        public string Singular { get; set; }
        public string Plural { get; set; }
        public static implicit operator string(Noun noun) => noun.Singular;
        public static implicit operator Noun(string value) => new Noun { Singular = value, Plural = Pluralizer.Pluralize(value) };
        public override string ToString() => Singular;
    }
}
