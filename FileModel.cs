using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUDDY
{
    public class FileModel
    {
        public ObservableCollection<FileUtility> Files
        {
            get;
            private set;
        }
        public FileModel()
        {
            Files = new ObservableCollection<FileUtility>();
            //this.GetDetails();
        }

        //public void GetDetails()
        //{
        //    Files.Add(new FileUtility(false, "NA", "NA"));
        //}
    }
}
