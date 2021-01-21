using System.Collections.Generic;
using System.Linq;

namespace BootGen
{
    public class DataModel
    {
        public List<ClassModel> Classes => ClassCollection.Classes;
        public List<EnumModel> Enums => EnumCollection.Enums;
        public List<ClassModel> StoredClasses => Classes.Where(s => s.Persisted).ToList();
        public List<ClassModel> CommonClasses => Classes.Where(p => p.Location == PropertyType.Normal).ToList();
        internal ClassCollection ClassCollection { get; }
        internal EnumCollection EnumCollection { get; }
        internal ResourceBuilder ResourceBuilder  { get; }
        internal TypeBuilder NonPersistedTypeBuilder  { get; }
        internal TypeBuilder PersistedTypeBuilder  { get; }

        public DataModel()
        {
            ClassCollection = new ClassCollection();
            EnumCollection = new EnumCollection();
            PersistedTypeBuilder = new TypeBuilder(ClassCollection, EnumCollection, true);
            NonPersistedTypeBuilder = new TypeBuilder(ClassCollection, EnumCollection, false);
            ResourceBuilder = new ResourceBuilder(PersistedTypeBuilder);
        }

        public void AddClass(ClassModel c)
        {
            ClassCollection.Add(c);
        }
        public void AddEnum(EnumModel e)
        {
            EnumCollection.Add(e);
        }
    }
}