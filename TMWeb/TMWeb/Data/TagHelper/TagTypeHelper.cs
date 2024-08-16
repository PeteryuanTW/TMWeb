namespace TMWeb.Data.TagHelper
{
    public static class TagTypeHelper
    {
        public static Dictionary<int, Type> TypeCodeDict = new Dictionary<int, Type>()
        {
            { 1, typeof(bool) },
            { 2, typeof(ushort) },
            { 3, typeof(float) },
            { 4, typeof(string) },
            { 11, typeof(bool[]) },
            { 22, typeof(ushort[]) },
            { 33, typeof(float[]) },
            { 44, typeof(string[]) },

        };
        public static bool TagTypeMatchCode(int code, Type type)
        {
            if (TypeCodeDict.ContainsKey(code))
            {
                if (TypeCodeDict[code] == type)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
