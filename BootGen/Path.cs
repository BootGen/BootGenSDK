using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootGen
{
    public class Path : List<PathComponent>
    {
        public string Relative => this.ToString();

        public List<Parameter> Parameters => this.Select(c => c.Parameter).Where(p => p != null).ToList();

        public Path()
        {
        }

        public Path(Path path) : base(path)
        {
        }

        internal Path Adding(PathComponent pathComponent)
        {
            Path path = new Path(this);
            path.Add(pathComponent);
            return path;
        }

        public override string ToString(){
            StringBuilder builder = new StringBuilder();
            foreach (var item in this) {
                builder.Append("/");
                builder.Append(item.ToString());
            }
            return builder.ToString();
        }
    }
}
