namespace Email2FAuth.Utils
{
    public class AuthHelper
    {
        public static string GenerateCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
