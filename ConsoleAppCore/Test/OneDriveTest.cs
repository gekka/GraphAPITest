using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Identity; //nuget
using Microsoft.Graph; //nuget
using Microsoft.Graph.Models;

namespace ConsoleApp1.Test
{

    internal class OneDriveTest
    {
        /// <summary> Onedriveへファイルをアップロードするテスト</summary>
        /// <remarks>
        /// ClientSecretの場合はアプリケーションの許可で、InteractiveBrowserCredentialでトークン取得するなら委任で
        /// 　　Directory.Read.All
        /// 　　Files.ReadWrite
        /// 　　User.Read
        /// </remarks>
        public static async Task FileUploadAsync(Microsoft.Graph.GraphServiceClient graphClient, Func<string, bool> emailFilter)
        {
            var getuser = graphClient.Users.ToGetRequestInformation().URI;
            var users = await graphClient.Users.GetAsync();
            if (users?.Value == null || users.Value.Count == 0)
            {
                throw new ApplicationException("ユーザーアカウント一覧を取得できませんでした");
            }

            //System.Diagnostics.Debug.WriteLine(string.Join("\r\n", users.Value.Select(_ => _.Mail)));

            var selectedUsers = users.Value.Where(_ => _.UserPrincipalName != null && emailFilter(_.Mail)).ToArray();
            if (selectedUsers.Length == 0)
            {
                throw new ApplicationException("目当てのアカウント取得できませんでした");
            }
            if (selectedUsers.Length > 1)
            {
                throw new ApplicationException("アカウントを特定できませんでした");
            }

            var userid = selectedUsers[0].Id;

            var drives = await graphClient.Users[userid].Drives.GetAsync();
            var onedrives = drives?.Value?.Where(_ => _.Name == "OneDrive").ToArray();
            if (onedrives == null || onedrives.Length == 0)
            {
                throw new ApplicationException("ユーザーのOneDriveを取得できませんでした");
            }
            if (onedrives.Length > 1)
            {
                throw new ApplicationException("OneDriveが複数存在します");
            }
            var driveId = onedrives[0].Id;

            //ルートフォルダにあるファイルとフォルダの一覧を取得
            var root = await graphClient.Drives[driveId].Root.GetAsync();
            if (root == null)
            {
                throw new ApplicationException("OneDriveのルートを取得できませんでした");
            }
            var driverootId = root.Id;

            var rootChildren = await graphClient.Drives[driveId].Items[driverootId].Children.GetAsync();
            if (rootChildren?.Value != null)
            {
                System.Diagnostics.Debug.WriteLine("ルートのアイテム");
                foreach (DriveItem item in rootChildren.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"\t{item.Name}");
                }
            }

            //ファイルを保存
            var body = new Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession.CreateUploadSessionPostRequestBody();
            var session = await graphClient.Drives[driveId].Items[driverootId].ItemWithPath("TestFolder/Test.txt").CreateUploadSession.PostAsync(body);

            var filedata = new System.Text.UTF8Encoding(true).GetBytes($"ファイルのアップロード {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            var dataStream = new System.IO.MemoryStream(filedata);

            var result = await new LargeFileUploadTask<DriveItem>(session, dataStream).UploadAsync();

        }

    }
}
