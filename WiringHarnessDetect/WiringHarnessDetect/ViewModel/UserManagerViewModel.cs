using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WiringHarnessDetect.Model;

namespace WiringHarnessDetect.ViewModel
{
    public class UserManagerViewModel:ViewModelBase
    {
        #region  Property
        private User user;
        public User User
        {
            get
            {
                if (user == null)
                    user = new User();
                return user;
            }
            set
            {
                user = value;
                RaisePropertyChanged("User");
            }
        }

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get
            {
                if (users == null)
                    users = new ObservableCollection<User>();
                return users;
            }
            set
            {
                users = value;
                RaisePropertyChanged("Users");
            }
        }

        private string passWord;
        public string PassWord
        {
            get => passWord;
            set
            {
                passWord = value;
                RaisePropertyChanged("PassWord");
            }
        }
        #endregion

        #region Command
        public RelayCommand Add
        {
            get
            {
               return new RelayCommand(() =>
               {
                   if(SQliteDbContext.GetUser(this.User)!=null)
                   {
                       MessageBox.Show("用户ID不能重复!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                       return;
                   }
                   if(User.Password=="")
                   {
                       MessageBox.Show("用户密码不能为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                       return;
                   }
                   if (User.Access == 0)
                   {
                       MessageBox.Show("用户权限不能为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                       return;
                   }
                   if (User.Password != PassWord)
                   {
                       MessageBox.Show("两次密码输入不一致", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                       return;
                   }
                   SQliteDbContext.AddUser(this.User);
                   Users = new ObservableCollection<User>(SQliteDbContext.GetAllUser());
               }
                   );
            }
        }
        public RelayCommand Update
        {
            get
            {
                return new RelayCommand(() => {
                    if (SQliteDbContext.GetUser(this.User) != null)
                    {
                        MessageBox.Show("用户ID不能重复!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (User.Password == "")
                    {
                        MessageBox.Show("用户密码不能为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (User.Access == 0)
                    {
                        MessageBox.Show("用户权限不能为空!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (User.Password != PassWord)
                    {
                        MessageBox.Show("两次密码输入不一致", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    SQliteDbContext.UpdateUser(this.User);
                    Users = new ObservableCollection<User>(SQliteDbContext.GetAllUser());
                });
            }
        }

        public RelayCommand OnLoad
        {
            get
            {
                return new RelayCommand(() => 
                Users=new ObservableCollection<User>( SQliteDbContext.GetAllUser())
                );
            }
        }

        public RelayCommand<string> Delete
        {
            get
            {
                return new RelayCommand<string>(p =>
                {
                    int rs = SQliteDbContext.DeleteUser(p);
                    if (rs > 0)
                    {
                        MessageBox.Show("删除成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    Users = new ObservableCollection<User>(SQliteDbContext.GetAllUser());
                }
                );
            }
        }
        #endregion
    }
} 
