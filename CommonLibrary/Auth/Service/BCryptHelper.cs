namespace CommonLibrary.Auth
{
    public static class BCryptHelper
    {
        public static string HashPassword(string password, int workfactor = 13)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, workfactor);
        }
        public static bool CheckPassword(string password, string hashpassword)
        {
            var a = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            return BCrypt.Net.BCrypt.Verify(password, hashpassword, true);
        }
    }
}
