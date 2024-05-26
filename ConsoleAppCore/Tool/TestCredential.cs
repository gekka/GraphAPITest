namespace ConsoleApp1.Tool
{
    using System;

    using System.Threading.Tasks;
    using Azure.Core;

    /// <summary>よそに保管しておいたAccessTokenを使うTokenCredential</summary>
    class TestCredentialStore : TokenCredential
    {
        public TestCredentialStore(AccessToken token)
        {
            this.token = token;
        }

        private readonly AccessToken token;

        public override AccessToken GetToken(TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken) => token;
        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken) => new ValueTask<AccessToken>(token);


        public static void Save(AccessToken accesstoken)
        {
            Console.WriteLine($"{accesstoken.ExpiresOn.DateTime}\r\n{accesstoken.Token}");
            System.IO.File.WriteAllLines("token.dat", new string[] { accesstoken.ExpiresOn.ToFileTime().ToString(), accesstoken.Token });
        }

        public static bool Load(out TokenCredential credential)
        {
            credential = null;
            if (System.IO.File.Exists("token.dat"))
            {
                var lines = System.IO.File.ReadAllLines("token.dat", new System.Text.UTF8Encoding(true));
                var expires = DateTimeOffset.FromFileTime(long.Parse(lines[0]));
                if (expires > DateTimeOffset.Now)
                {
                    var accessToken = new AccessToken(lines[1], expires);
                    credential = new TestCredentialStore(accessToken);
                    return true;
                }
            }

            return false;
        }
    }
}