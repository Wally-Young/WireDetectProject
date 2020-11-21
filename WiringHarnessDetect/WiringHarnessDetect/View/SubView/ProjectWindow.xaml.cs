using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommonServiceLocator;
using Panuon.UI.Silver;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;

namespace WiringHarnessDetect.View
{
    /// <summary>
    /// ProjectFrm.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectWindow : WindowX
    {
        public Project project;
        public bool IsModify;
        public ProjectWindow()
        {
            InitializeComponent();
            project = new Project();
            //project.Author = (App.Current.Resources["Locator"] as ViewModelLocator).Main.User.EmployeeNumbe;
            this.DataContext = project;

        }
        public ProjectWindow(Project project)
        {
            InitializeComponent();
            this.project = project;
            this.txtproject.IsEnabled = false;
            //project.Author = (App.Current.Resources["Locator"] as ViewModelLocator).Main.User.EmployeeNumbe;
            this.DataContext = project;

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Tag.ToString() == "confirm")
            {

                bool rs = CheckProject();
                if (!rs)
                    return;
                (App.Current.Resources["Locator"] as ViewModelLocator).Main.Projects.Add(project);
                SQliteDbContext.AddProject(project);
                string msg = IsModify ? "修改成功!" : "添加成功!";
                MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else if (button.Tag.ToString() == "cancel")
            {
                if (!IsModify)
                {
                    foreach (var item in grid.Children)
                    {
                        if (item is TextBox)
                        {
                            (item as TextBox).Text = "";
                        }
                    }
                }
                //project.Author = (App.Current.Resources["Locator"] as ViewModelLocator).Main.User.EmployeeNumbe;
            }

        }

        private bool CheckProject()
        {
            if (this.project.ProjectNO.Trim().Length == 0)
            {
                MessageBox.Show("工程编号不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.txtproject.Focus();
                return false;
            }
            bool isExist = (App.Current.Resources["Locator"] as ViewModelLocator).Main.Projects.ToList().Exists(x => x.ProjectNO == project.ProjectNO);
            if (isExist&&!IsModify)
            {
                MessageBox.Show("工程编号不能重复!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.txtproject.Focus();
                return false;
            }

            if (project.Author.Trim().Length == 0)
            {
                MessageBox.Show("创建者不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.txtproject.Focus();
                return false;
            }
            if (project.ProjectName.Trim().Length == 0)
            {
                MessageBox.Show("工程名称不能为空!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.txtproject.Focus();
                return false;
            }
            return true;
        }
    }

    public class TypeToIntConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {


            return (int)value == 1 ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            return (bool)value ? 1 : 0;
        }
    }


}
