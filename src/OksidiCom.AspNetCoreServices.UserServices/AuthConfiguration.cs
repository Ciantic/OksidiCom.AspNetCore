﻿namespace OksidiCom.AspNetCoreServices.UserServices
{
    public class AuthConfiguration
    {
        public class GoogleConfiguration
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
        public GoogleConfiguration Google { get; set; }
    }
}
