namespace Sitecore.Support.Forms.Mvc.Controllers.Filters
{
  using Sitecore.Forms.Mvc;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security;
  using System.Security.Cryptography;
  using System.Text;
  using System.Web;
  using System.Web.Caching;
  using System.Web.Mvc;

  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
  internal class WffmLimitMultipleSubmits : FilterAttribute, IAuthorizationFilter
  {
    public string CalculateMD5Hash(string input)
    {
      byte[] buffer2 = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(input));
      StringBuilder builder = new StringBuilder();
      for (int i = 0; i < buffer2.Length; i++)
      {
        builder.Append(buffer2[i].ToString("X2"));
      }
      return builder.ToString();
    }

    public void OnAuthorization(AuthorizationContext filterContext)
    {
      Cache cache = filterContext.HttpContext.Cache;
      if (!string.IsNullOrEmpty(filterContext.HttpContext.Request.Headers["X-RequestVerificationToken"]))
      {
        int num = Settings.LimitMultipleSubmits_IntervalInSeconds;
        if (num > 0)
        {
          string input = filterContext.HttpContext.Request.Headers["X-RequestVerificationToken"];
          input = this.CalculateMD5Hash(input);
          if (cache[input] != null)
          {
            if (cache[input] == string.Empty)
            {
              cache.Remove(input);
              cache.Add(input, "attempted", null, DateTime.Now.AddSeconds((double)num), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
              throw new SecurityException("There was an attempt to do multiple submits within a time interval, specified in the \"WFM.LimitMultipleSubmits.IntervalInSeconds\" setting!");
            }
            filterContext.Result = new HttpUnauthorizedResult();
          }
          else
          {
            cache.Add(input, "", null, DateTime.Now.AddSeconds((double)num), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
          }
        }
      }
    }
  }
}