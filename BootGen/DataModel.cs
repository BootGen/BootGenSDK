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

        public DataModel()
        {
            ClassCollection = new ClassCollection();
            EnumCollection = new EnumCollection();
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