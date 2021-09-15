using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RehabKiosk.Model
{
    public class RehabAsset
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string AssetImageFilename { get; set; }
        public string BarcodeImageFilename { get; set; }

        private void Initialize()
        {

        }



        static public RehabAsset Load(string filename)
        {
            try
            {
                if (filename != null)
                {
                    using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var ser = new DataContractJsonSerializer(typeof(RehabAsset));
                        RehabAsset asset = ser.ReadObject(fileStream) as RehabAsset;
                        asset.Initialize();
                        fileStream.Close();
                        return asset;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("PersistedArcItem::Load " + filename + " **EXCEPTION** " + e.ToString());
            }
            return null;
        }
    }

}
