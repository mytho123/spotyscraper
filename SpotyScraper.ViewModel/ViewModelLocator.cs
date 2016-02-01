using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.ViewModel
{
    public class ViewModelLocator
    {
        private ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (BaseVM.IsInDesignModeStatic)
            {
            }
            else
            {
            }

            SimpleIoc.Default.Register<MainViewModel>();
        }

        public static ViewModelLocator Instance { get; } = new ViewModelLocator();

        public MainViewModel Main
        {
            get { return SimpleIoc.Default.GetInstance<MainViewModel>(); }
        }
    }
}