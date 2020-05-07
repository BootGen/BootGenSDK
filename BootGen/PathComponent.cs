namespace BootGen
{
    public class PathComponent
    {
        public bool IsVariable => Parameter != null;
        public string Name { get; internal set; }
        public Parameter Parameter { get; internal set; }

        public override string ToString(){
            if (IsVariable) {
                return "{" + Name + "}";
            } else {
                return Name;
            }
        }
    }
}
