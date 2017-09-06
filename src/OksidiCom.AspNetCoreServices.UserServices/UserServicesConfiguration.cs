namespace OksidiCom.AspNetCoreServices.UserServices
{
    public class UserServicesConfiguration
    {
        public class GoogleConfiguration
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
        public GoogleConfiguration Google { get; set; }

        public class JwtConfiguration
        {
            public string Authority { get; set; }
            public string Audience { get; set; }
        }

        public JwtConfiguration Jwt { get; set; } = new JwtConfiguration();
    }
}
