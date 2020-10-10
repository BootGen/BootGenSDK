using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class DataModel
    {
        public List<ClassModel> Classes => ClassCollection.Classes;
        public List<EnumModel> Enums => EnumCollection.Enums;
        public List<ClassModel> StoredClasses => Classes.Where(s => s.Persisted).ToList();
        public List<ClassModel> ServerClasses => Classes.Where(p => p.Location != Location.ClientOnly).ToList();
        public List<ClassModel> ClientClasses => Classes.Where(p => p.Location != Location.ServerOnly).ToList();
        public List<ClassModel> CommonClasses => Classes.Where(p => p.Location == Location.Both).ToList();
        internal ClassCollection ClassCollection { get; }
        internal EnumCollection EnumCollection { get; }
        internal ResourceBuilder ResourceBuilder  { get; }
        internal TypeBuilder TypeBuilder  { get; }

        public DataModel()
        {
            ClassCollection = new ClassCollection();
            EnumCollection = new EnumCollection();
            TypeBuilder = new TypeBuilder(ClassCollection, EnumCollection);
            ResourceBuilder = new ResourceBuilder(TypeBuilder);
        }
    }
}