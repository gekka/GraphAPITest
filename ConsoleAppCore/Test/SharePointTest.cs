using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity; //nuget
using Microsoft.Graph; //nuget
using Microsoft.Graph.Models;
using Microsoft.Graph.Drives.Item;
using Microsoft.IdentityModel.Tokens;
namespace ConsoleApp1.Test
{
    /// <summary>SharePointへファイルをアップロードするテスト</summary>
    /// <remarks>
    ///     Sites.Read.All
    /// 　　Files.ReadWrite
    ///</remarks>
    internal class SharePointTest
    {
        public static async Task FileUploadAsync(GraphServiceClient graphClient)
        {
            if (string.IsNullOrWhiteSpace(TestSettings.SharePointSiteID))
            {
                var sites = await graphClient.Sites.GetAsync(); // Sites.Read.All アプリケーション(クライアントシークレット)
                foreach (var site in sites.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"{site.DisplayName}\t{site.Id}");
                    if (site.DisplayName == TestSettings.SharePointSiteName && site.Id != null)
                    {
                        TestSettings.SharePointSiteID = site.Id;
                    }
                }
                if (string.IsNullOrWhiteSpace(TestSettings.SharePointSiteID))
                {
                    throw new ApplicationException("SharePointのサイトが特定できませんでした");
                }
            }


            if (string.IsNullOrEmpty(TestSettings.SharePointDriveID))
            {
                var drives = await graphClient.Sites[TestSettings.SharePointSiteID]
                    .Drives.GetAsync();
                foreach (var d in drives.Value)
                {
                    if (d.Id != null)
                    {
                        TestSettings.SharePointDriveID = d.Id;
                    }
                }
                if (string.IsNullOrEmpty(TestSettings.SharePointDriveID))
                {
                    throw new ApplicationException("SharePointのサイトの保存ドライブが見つかりませんでした");
                }
            }

            if (string.IsNullOrWhiteSpace(TestSettings.SharePointListID))
            {
                var list = await graphClient.Sites[TestSettings.SharePointSiteID].Lists.GetAsync();
                foreach (var item in list.Value)
                {
                    System.Diagnostics.Debug.WriteLine(item.Id);
                    if (item.Id != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"{item.DisplayName}\t{item.Id}");
                        TestSettings.SharePointListID = item.Id;
                    }
                }
                if (string.IsNullOrWhiteSpace(TestSettings.SharePointListID))
                {
                    throw new ApplicationException("SharePointのサイト内のリストを特定できませんでした");
                }
            }


            var listAccessor = graphClient.Sites[TestSettings.SharePointSiteID].Lists[TestSettings.SharePointListID];
            {
                var curentListItems = await listAccessor.Items.GetAsync();
                foreach (var id in curentListItems.Value.Select(_ => _.Id))
                {
                    var listItem = await graphClient
                        .Sites[TestSettings.SharePointSiteID]
                        .Lists[TestSettings.SharePointListID]
                        .Items[id].GetAsync();
                    System.Diagnostics.Debug.WriteLine(listItem.WebUrl);
                }
            }

            //ルートフォルダにあるファイルとフォルダの一覧を取得
            var root = await graphClient.Drives[TestSettings.SharePointDriveID].Root.GetAsync();
            if (root == null)
            {
                throw new ApplicationException("SharePointのサイトのドライブルートを取得できませんでした");
            }
            var driverootId = root.Id;

            //ファイルを保存
            var body = new Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession.CreateUploadSessionPostRequestBody();
            var session = await graphClient.Drives[TestSettings.SharePointDriveID]
                .Items[driverootId]
                .ItemWithPath($"Test.txt").CreateUploadSession.PostAsync(body);

            var filedata = new System.Text.UTF8Encoding(true)
                .GetBytes($"ファイルのアップロード {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            var dataStream = new System.IO.MemoryStream(filedata);
            var result = await new LargeFileUploadTask<DriveItem>(session, dataStream).UploadAsync();
            if (!result.UploadSucceeded)
            {
                throw new ApplicationException("ファイルアップロード失敗");

            }

            var newlistItems = await listAccessor.Items.GetAsync();
            foreach (var id in newlistItems.Value.Select(_ => _.Id))
            {
                var listItem = await graphClient
                    .Sites[TestSettings.SharePointSiteID]
                    .Lists[TestSettings.SharePointListID]
                    .Items[id].GetAsync();

                if (listItem?.WebUrl == result.ItemResponse.WebUrl)
                {
                    Console.WriteLine("SharePointへのアップロード成功");
                }
            }
        }
    }
}
