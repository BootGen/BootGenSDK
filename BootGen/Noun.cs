namespace BootGen
{
    public class Noun
    {
        public string Singular { get; set; }
        public string Plural { get; set; }
        public static implicit operator string(Noun noun) => noun.Singular;
        public static implicit operator Noun(string value) => new Noun { Singular = value, Plural = $"{value}s" };
        public override string ToString() => Singular;
    }
}
