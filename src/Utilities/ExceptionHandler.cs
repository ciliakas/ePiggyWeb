namespace ePiggyWeb.Utilities
{
    public static class ExceptionHandler
    {
        public static void Log(string error = "Undefined exception")
        {
            System.IO.File.AppendAllText(@"Errors.txt", error + "\n");
        }
    }
}
