using RehabKiosk.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChildrensRehab
{
    public class RehabAssetManager
    {
        public const string ASSET_FILE_EXTENSION = ".JSON";
        private static string REHAB_FOLDER = "ChildrensRehab";
        private string _workingFolderName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), REHAB_FOLDER);
        private ObservableCollection<RehabAsset> _assets = new ObservableCollection<RehabAsset>();
        private int _currentAssetIndex = -1;


        public void Initialize()
        {

            // Create working folder
            Directory.CreateDirectory(_workingFolderName);

            // Enumerate files
            var dirInfo = new DirectoryInfo(_workingFolderName);
            var info = dirInfo.GetFiles("*" + ASSET_FILE_EXTENSION);

            // Load/add each file to collection
            foreach (var file in info)
            {
                var asset = RehabAsset.Load(file.FullName);
                if (asset != null)
                {
                    _assets.Add(asset);
                }
            }

        }



        public string WorkingFolder
        {
            get
            {
                return _workingFolderName;
            }
        }


        public RehabAsset GetNextAsset()
        {
            if (_assets.Count == 0)
                return null;

            if (_currentAssetIndex == -1)
                _currentAssetIndex = 0;
            else
                _currentAssetIndex = (_currentAssetIndex + 1 < _assets.Count) ? _currentAssetIndex + 1 : 0;
            return _assets[_currentAssetIndex];
        }



        public RehabAsset GetPrevAsset()
        {
            if (_assets.Count == 0)
                return null;
            if (_currentAssetIndex == -1)
                _currentAssetIndex = _assets.Count - 1;
            else
                _currentAssetIndex = (_currentAssetIndex == 0) ? _assets.Count - 1 : _currentAssetIndex - 1;
            return _assets[_currentAssetIndex];
        }


    }
}
