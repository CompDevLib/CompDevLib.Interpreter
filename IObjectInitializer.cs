namespace CompDevLib.Interpreter
{
    public interface IObjectInitializer
    {
        object CreateInstance();
        void SetField(object instance, string fieldName, object value);
    }
}