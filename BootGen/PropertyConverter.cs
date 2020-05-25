namespace BootGen
{
    public static class PropertyConverter
    {

        public static Parameter ConvertToParameter(this Property property)
        {
            var oasProp = new Parameter { Name = property.Name.ToSnakeCase() };
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    oasProp.Required = true;;
                    break;
                case BuiltInType.Int32:
                    oasProp.Required = true;
                    break;
                case BuiltInType.Int64:
                    oasProp.Required = true;
                    break;
                case BuiltInType.String:
                    oasProp.Required = false;
                    break;
                case BuiltInType.Object:
                    oasProp.Required = false;
                    break;
            }
            oasProp.BuiltInType = property.BuiltInType;
            oasProp.Schema = property.Schema;
            oasProp.IsCollection = property.IsCollection;

            return oasProp;
        }
    }
}