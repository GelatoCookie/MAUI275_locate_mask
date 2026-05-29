using MauiRfidSample.MVVM.ViewModels;

namespace MauiRfidSample.MVVM.Views
{
    public partial class AdminLogin : ContentPage
    {
        private AdminLoginViewModel viewModel;
        public AdminLogin()
        {
            InitializeComponent();
            BindingContext = viewModel = new AdminLoginViewModel();
        }

        private async void NavigateToChangePasswordCommand(object sender, EventArgs e)
        {
            var changePasswordViewModel = new ChangePassword();
            await Navigation.PushAsync(changePasswordViewModel);
        }

        private async void ShowPasswordCriteriaCommand(object sender, EventArgs e)
        {
            viewModel.ShowPasswordCriteria();
        }

    }
}