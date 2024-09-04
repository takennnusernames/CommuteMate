namespace CommuteMate.Views;

public partial class SurveyPage : ContentPage
{
	public SurveyPage()
	{
		InitializeComponent();
        // Replace with your Google Forms URL
        string googleFormsUrl = "https://docs.google.com/forms/d/e/1FAIpQLSd07aXGubBsQFatUn8pH1mcoPUyywajwbhY0VfsGpsoQ6zqMw/viewform";
        SurveyWebView.Source = googleFormsUrl;
    }
}