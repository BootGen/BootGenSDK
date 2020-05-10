namespace BootGen
{
    public static class PropertyConverter
    {
        public static T ConvertProperty<T>(this Property property) where T : IOASProperty, new()
        {
            var oasProp = new T { Name = property.Name.ToSnakeCase() };
            switch (property.BuiltInType)
            {
                case BuiltInType.Bool:
                    oasProp.Required = true;
                    oasProp.Type = "boolean";
                    break;
                case BuiltInType.Int32:
                    oasProp.Required = true;
                    oasProp.Type = "integer";
                    oasProp.Format = "int32";
                    break;
                case BuiltInType.Int64:
                    oasProp.Required = true;
                    oasProp.Type = "integer";
                    oasProp.Format = "int64";
                    break;
                case BuiltInType.String:
                    oasProp.Required = false;
                    oasProp.Type = "string";
                    break;
                case BuiltInType.Object:
                    oasProp.Reference = property.Schema.Name;
                    break;
            }
            oasProp.IsCollection = property.IsCollection;

            return oasProp;
        }
    }
}