using System;
using System.Web.UI;

namespace UniSkillHub
{
    // One friendly page for every error. Only a fixed list of codes is understood, and the
    // text is written here (never taken from the URL), so the page cannot be used to show
    // attacker-written text.
    public partial class ErrorPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string heading, message;
            int status;

            switch (Request.QueryString["code"])
            {
                case "input":
                    status = 400;
                    heading = "That input is not allowed";
                    message = "Some of the text you entered (or the address you opened) contains characters that look like HTML tags, " +
                              "for example a \"<\" followed by a letter. Please remove them and try again.";
                    lnkBack.Visible = true;
                    break;
                case "csrf":
                    status = 400;
                    heading = "This form could not be verified";
                    message = "The form was opened a long time ago, in another browser, or from another website. " +
                              "For your safety nothing was changed. Please reload the page and try again.";
                    break;
                case "404":
                    status = 404;
                    heading = "Page not found";
                    message = "The page you are looking for does not exist or has been moved.";
                    break;
                default:
                    status = 500;
                    heading = "Something went wrong";
                    message = "Sorry, an unexpected error happened on our side. Please try again in a moment.";
                    lnkBack.Visible = true;
                    break;
            }

            litHeading.Text = heading;
            litMessage.Text = message;
            Response.StatusCode = status;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
