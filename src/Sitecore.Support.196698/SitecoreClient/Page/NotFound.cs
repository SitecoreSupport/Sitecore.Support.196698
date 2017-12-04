namespace Sitecore.Support.SitecoreClient.Page
{
  using Sitecore;
  using Sitecore.Pipelines.HandlePageNotFound;
  using Sitecore.Sites;
  using Sitecore.Web;
  using System;
  using System.Web;
  using System.Web.SessionState;
  using System.Web.UI;
  using System.Web.UI.HtmlControls;
  using System.Web.UI.WebControls;

  public class NotFound : Page
  {
    protected HtmlGenericControl PageEditor;

    protected PlaceHolder RequestedUrl;

    protected PlaceHolder SiteName;

    protected PlaceHolder UserName;

    protected override void OnPreRender(EventArgs e)
    {
      base.OnPreRender(e);
      string[] values = new string[] { base.Request.QueryString["item"], "[unknown]" };
      string str = MainUtil.DecodeName(StringUtil.GetString(values));
      string[] textArray2 = new string[] { base.Request.QueryString["user"] };
      string str2 = StringUtil.GetString(textArray2);
      string[] textArray3 = new string[] { base.Request.QueryString["site"] };
      string str3 = StringUtil.GetString(textArray3);
      if(str == "[unknown]")
      {
        try
        {
          HttpSessionState aspSession = Context.Items["AspSession"] as HttpSessionState;
          str = aspSession["WFFM196698URL"].ToString();
          str2 = aspSession["WFFM196698USER"].ToString();
          str3 = aspSession["WFFM196698SiTE"].ToString();
        }
        catch (NullReferenceException) { }
      }
      str = WebUtil.SafeEncode(str);
      this.RequestedUrl.Controls.Add(new LiteralControl(str));
      this.UserName.Controls.Add(new LiteralControl(WebUtil.SafeEncode(str2)));
      this.SiteName.Controls.Add(new LiteralControl(WebUtil.SafeEncode(str3)));
      base.Response.StatusCode = 0x194;
      base.Response.TrySkipIisCustomErrors = true;
      base.Response.StatusDescription = "Not Found";
      bool flag = false;
      SiteContext site = Sitecore.Context.Site;
      if (site != null)
      {
        flag = site.DisplayMode == DisplayMode.Edit;
      }
      this.PageEditor.Visible = flag;
      HandlePageNotFoundPipeline.Run(new HandlePageNotFoundArgs(str));
    }
  }
}