using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Panuon.UI;
using WiringHarnessDetect.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;

namespace WiringHarnessDetect.ViewModel
{

    public class LoginViewModel : ViewModelBase
    {
        private User user;
        

      
        public LoginViewModel()
        {
            user = new User();

         
           
        }



        #region Property
        [Required]
        [RegularExpression("[0-9]+", ErrorMessage = "ID只能由数字组成")]
        public string TxtUserID { get => user.UserID; set { user.UserID = value; RaisePropertyChanged("TxtUserID"); } }

        public  User  User
        {
            get => user;
            set
            {
                user = value;
                RaisePropertyChanged("User");
            }
        }


        #endregion

        #region Command

        public RelayCommand<string> SubmitCmd
        {
            get
            {
                return new RelayCommand<string>(p=>CheckLog(p));
            }

        }


        public RelayCommand Exit
        {
            get
            {
                return new RelayCommand(ExitWindow);

            }

        }
        #endregion

        #region  Method
        private void CheckLog(string txt)
        {
            try
            {
              
                var result = SQliteDbContext.GetUser(user);
                if (result != null)
                {
                    this.user = result;
                    Messenger.Default.Send<User>(this.user, "LogSuccess"); //注意：token参数一致
                    this.user.LastLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    SQliteDbContext.UpdateLoginTime(user);
                }
                else
                {
                    MultiMessageBox multiMessageBox = new MultiMessageBox("提示", "用户名或者密码不对!", MBoxType.Info);
                    multiMessageBox.ShowDialog();
                }
            }
            catch (Exception ex)
            {

                MultiMessageBox multiMessageBox = new MultiMessageBox("提示", "用户名或者密码不对!", MBoxType.Info);
                multiMessageBox.ShowDialog();
            }

        }


        private void ExitWindow()
        {
            MultiMessageBox multiMessageBox = new MultiMessageBox("问询", "您确定要退出吗!", MBoxType.Confirm);
            multiMessageBox.ShowDialog();
            if (multiMessageBox.DiaResult == true)
            {
                App.Current.Shutdown();
            }
        }


       

        #endregion
    }
}
