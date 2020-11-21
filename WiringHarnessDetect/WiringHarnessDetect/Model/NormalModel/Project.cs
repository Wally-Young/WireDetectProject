using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    [Serializable]
    public class Project:ValidateModelBase
    {
        #region  Property
        private string projectName;
        [Required]
        public string ProjectName
        {
            get => projectName;
            set
            {
                projectName = value;
                RaisePropertyChanged("ProjectName");
            }
        }
        private string projectNO;
        [Required]
        public string ProjectNO
        {
            get => projectNO;
            set
            {
                projectNO = value;
                RaisePropertyChanged("ProjectNO");
            }
        }

        private string author;
        [Required]
        public string Author
        {
            get => author;
            set
            {
                author = value;
                RaisePropertyChanged("Author");
            }
        }


        private string description;
       
        public string Description
        {
            get => description;
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }


        public int projectType;
        [Required]
        public int ProjectType
        {
            get => projectType;
            set
            {
                projectType = value;
                RaisePropertyChanged("ProjectType");
            }
        }
        #endregion
    }

  
  
}
