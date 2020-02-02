using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace ContentHub.Importer.Providers
{
     public class ExcelService
    {

        public static List<Asset> GetAssets(string filePath)
        {
            var assets = new List<Asset>();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheet = result.Tables["M.Asset"];
                    var rows = sheet.Rows;
                    var rowCount = 0;
                    foreach (DataRow row in rows)
                    {
                        if (rowCount == 0)
                        {
                            rowCount++;
                            continue;
                        }
                        assets.Add(new Asset
                        {
                            OriginUrl = row.ItemArray[0] as string,
                            Description = row.ItemArray[1] as string,
                            MarketingDescription = row.ItemArray[2] as string,
                            AssetType = row.ItemArray[3] as string,
                            SocialMediaChannel = row.ItemArray[4] as string,
                            ContentSecurity = row.ItemArray[5] as string,
                            AssetSource = row.ItemArray[6] as string,
                            LifecycleStatus = "Created"
                        });
                        rowCount++;
                    }
                }
            }
            
            return assets;
        }
    }
}
