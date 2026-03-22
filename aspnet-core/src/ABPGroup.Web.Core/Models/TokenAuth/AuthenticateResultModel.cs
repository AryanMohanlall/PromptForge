namespace ABPGroup.Models.TokenAuth
{
    public class AuthenticateResultModel
    {
        public string AccessToken { get; set; }

        public string EncryptedAccessToken { get; set; }

        public int ExpireInSeconds { get; set; }

        public long UserId { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string EmailAddress { get; set; }

        public string[] RoleNames { get; set; }
        public int Role { get; set; }      // 0, 1, or 2
        public string RoleName { get; set; } // "Admin", "Developer", or "ProductBuilder"
    }
}
