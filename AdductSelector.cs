using BUDDY.FormulaData;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BUDDY
{
    class AdductSelector : IItemsSourceSelector
    {
        public StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

        public IEnumerable GetItemsSource(object record, object dataContext)
        {
            if (record == null)
                return null;

            Ms2Utility ms2info = record as Ms2Utility;
            string polarity = ms2info.Polarity;

            if(polarity == "P")
            {
                ObservableCollection<Adduct> adductList_Pos;
                using (Stream stream = File.Open(storageFolder.Path + @"\adductList_Pos.bin", FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    adductList_Pos = new ObservableCollection<Adduct>((IEnumerable<Adduct>)bformatter.Deserialize(stream));
                }
                List<string> adductList = new List<string>();
                adductList = adductList_Pos.Select(o => o.Formula).ToList();
                return adductList;
            }
            else
            {
                ObservableCollection<Adduct> adductList_Neg;
                using (Stream stream = File.Open(storageFolder.Path + @"\adductList_Neg.bin", FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    adductList_Neg = new ObservableCollection<Adduct>((IEnumerable<Adduct>)bformatter.Deserialize(stream));
                }
                List<string> adductList = new List<string>();
                adductList = adductList_Neg.Select(o => o.Formula).ToList();
                return adductList;
            }
            return null;
        }
    }
}
