<%@ WebHandler Language="C#" Class="CheckWebsiteStatusHandler" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;

/// <summary>
/// Handler to check the status of our website, usign Gammapartners library.
/// </summary>
public class CheckWebsiteStatusHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";

        try
        {
            using (StreamReader sr = new StreamReader(HostingEnvironment.ApplicationPhysicalPath + "status.config"))
            {
                string configFile = sr.ReadToEnd();
                string errorMessageForQueries, errorMessageForPages, errorMessageForPaths;
                
                GammapartnersWebsiteStatus.WebsiteStatus statusPage = new GammapartnersWebsiteStatus.WebsiteStatus(configFile);
                
                bool queriesStatus = statusPage.CheckQueriesStatus(out errorMessageForQueries);
                bool pagesStatus = statusPage.CheckPagesStatus(out errorMessageForPages);
                bool pathsStatus = statusPage.CheckPathsStatus(out errorMessageForPaths);

                if (queriesStatus && pagesStatus && pathsStatus)
                {
                    context.Response.Write("Everything is alright!");
                }
                else
                {
                    context.Response.Write("Queries status: " + (queriesStatus ? "OK" : "Error\n" + errorMessageForQueries));
                    context.Response.Write("\n\nPages status: " + (pagesStatus ? "OK" : "Error\n" + errorMessageForPages));
                    context.Response.Write("\n\nPaths status: " + (pathsStatus ? "OK" : "Error\n" + errorMessageForPaths));
                }
            }
        }
        catch (Exception e)
        {
            context.Response.Write("Error reading the cofig file: " + e.Message);
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}

