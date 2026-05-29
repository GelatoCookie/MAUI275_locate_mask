using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample.MVVM.Views
{
    public partial class ChangePassword : ContentPage
    {
        private ChangePasswordViewModel viewModel;
        
        public ChangePassword()
        {
            InitializeComponent();
            BindingContext = viewModel = new ChangePasswordViewModel();
        }
        private async void ShowPasswordCriteriaCommand(object sender, EventArgs e)
        {
            viewModel.ShowPasswordCriteria();
        }

    }
}