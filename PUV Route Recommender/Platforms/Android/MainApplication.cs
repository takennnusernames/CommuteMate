using Android.App;
using Android.Runtime;

namespace CommuteMate
{
    [Application]
    [MetaData("com.google.android.maps.v2.API_KEY",
            Value = "AIzaSyC_zmye1jCAnMGsWfevUPmN8UzlRz6mu_g")]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}