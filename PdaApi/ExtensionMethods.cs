using System;

namespace PdaApi
{
    public static class ExtensionMethods
    {
        public static string GetExceptionMessage(this Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex.Message;
        }
    }
}
