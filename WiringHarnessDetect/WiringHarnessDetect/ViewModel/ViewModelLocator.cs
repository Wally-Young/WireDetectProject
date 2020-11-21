/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:WiringHarnessDetect"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;

using System.Windows.Navigation;

namespace WiringHarnessDetect.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<PaperManagerViewModel>();
            SimpleIoc.Default.Register<PassiveViewModel>();
            SimpleIoc.Default.Register<ExcelManagerViewModel>();
            SimpleIoc.Default.Register<ParameterViewModel>();
            SimpleIoc.Default.Register<ExPassiveDetectViewModel>();
            SimpleIoc.Default.Register<UserManagerViewModel>();
            SimpleIoc.Default.Register<ActiveViewModel>();
        }


        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public LoginViewModel Login
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LoginViewModel>();
            }
        }

        public PaperManagerViewModel Paper
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PaperManagerViewModel>();
            }
           
        }
        public PassiveViewModel Passive
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PassiveViewModel>();
            }

        }

        public ExcelManagerViewModel ExcelPaper
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ExcelManagerViewModel>();
            }

        }

        public ExPassiveDetectViewModel ExcelPassive
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ExPassiveDetectViewModel>();
            }

        }
        public ParameterViewModel Parameter
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ParameterViewModel>();
            }

        }
        public UserManagerViewModel User
        {
            get
            {
                return ServiceLocator.Current.GetInstance<UserManagerViewModel>();
            }

        }
        public ActiveViewModel Atctive
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ActiveViewModel>();
            }

        }


        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}