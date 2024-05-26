namespace ConsoleApp1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Azure.Identity; //nuget
    using Microsoft.Graph;//nuget

    using ConsoleApp1.Test;
    using ConsoleApp1.Tool;

    class Program
    {
        static async Task Main(string[] args)
        {
            bool isTestTeamsPost = false;
            bool isTestOneDriveUploadTest = true;

            bool useClientSecret = false;


            try
            {
                Azure.Core.TokenCredential credential;
                if (useClientSecret)
                {
                    credential = await GetClientSecretCredentialAsync();
                }
                else
                {
                    credential = await GetCredentialAsync();
                }

#if NETCOREAPP
                using (var http2Client = new System.Net.Http.HttpClient())
#else
                using (var http2Client = new System.Net.Http.HttpClient(new Http2CustomHandler())) // HTTP/2を使えるように(Windows8以降?)
#endif
                {

                    Microsoft.Graph.GraphServiceClient graphClient = new Microsoft.Graph.GraphServiceClient(http2Client, credential);

                    if (isTestTeamsPost)
                    {
                        await TeamsTest.PostMessageAsync(graphClient, "テストですよ");
                    }
                    if (isTestOneDriveUploadTest)
                    {
                        await OneDriveTest.FileUploadAsync(graphClient, (mail) => mail == TestSettings.OnedriveAccountEmail);
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("成功しました");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                Console.WriteLine("失敗しました");
                Environment.ExitCode = 1;
            }
            Console.ReadLine();
        }


        static Task<Azure.Core.TokenCredential> GetClientSecretCredentialAsync()
        {
            var options = new ClientSecretCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
            var clientSecretCredential = new ClientSecretCredential(TestSettings.TenantId, TestSettings.ClientId, TestSettings.ClientSecret, options);
            return Task.FromResult<Azure.Core.TokenCredential>(clientSecretCredential);
        }

        /// <summary>ブラウザを起動してアクセストークンを取得させる</summary>
        /// <returns></returns>
        static async Task<Azure.Core.TokenCredential> GetCredentialAsync(bool useCache = false)
        {
#if DEBUG
            if (useCache && TestCredentialStore.Load(out var store))
            {
                return store;
            }
#endif

            //ブラウザを起動してアクセストークンを取得させる
            var opt = new Azure.Identity.InteractiveBrowserCredentialOptions();
            opt.TenantId = TestSettings.TenantId;
            opt.ClientId = TestSettings.ClientId;
            opt.AuthorityHost = Azure.Identity.AzureAuthorityHosts.AzurePublicCloud;
            opt.RedirectUri = new Uri("http://localhost");

            Azure.Identity.InteractiveBrowserCredential interactive = new Azure.Identity.InteractiveBrowserCredential(opt);
            Azure.Core.AccessToken accesstoken = await interactive.GetTokenAsync(new Azure.Core.TokenRequestContext());

#if DEBUG
            TestCredentialStore.Save(accesstoken);
#endif
            return interactive;
        }


    }

    /*
    class TestSettings
    {
        /// <summary>Azureポータル->アプリの登録->概要->アプリケーション (クライアント) ID</summary>
        public static string ClientId = "";

        /// <summary>Azureポータル->アプリの登録->概要->ディレクトリ (テナント) ID</summary>
        public static string TenantId = "";

        /// <summary>Azureポータル->アプリの登録->管理->証明書とシークレット->クライアントシークレット->値</summary>
        public static string ClientSecret = "";

        /// <summary>Teamsで投稿先のID</summary>
        public static string TeamID = "";

        /// <summary>Onedriveのアカウントのメールアドレス</summary>
        public static string OnedriveAccountEmail = "";

    }
    */
}