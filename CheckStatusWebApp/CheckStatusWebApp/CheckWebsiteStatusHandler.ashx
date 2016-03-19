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
        string response = string.Empty;
        GammapartnersWebsiteStatus.WebsiteStatus statusPage = null;
        bool error = true;

        try
        {
            using (StreamReader sr = new StreamReader(HostingEnvironment.ApplicationPhysicalPath + "status.config"))
            {
                string configFile = sr.ReadToEnd();
                string errorMessageForQueries, errorMessageForPages, errorMessageForPaths;
                
                statusPage = new GammapartnersWebsiteStatus.WebsiteStatus(configFile);
                
                bool queriesStatus = statusPage.CheckQueriesStatus(out errorMessageForQueries);
                bool pagesStatus = statusPage.CheckPagesStatus(out errorMessageForPages);
                bool pathsStatus = statusPage.CheckPathsStatus(out errorMessageForPaths);

                if (queriesStatus && pagesStatus && pathsStatus)
                {
                    response = "Everything is alright!";
                    error = false;
                }
                else
                {
                    response = "Queries status: " + (queriesStatus ? "OK" : "Error\n" + errorMessageForQueries)
                                + "\n\nPages status: " + (pagesStatus ? "OK" : "Error\n" + errorMessageForPages)
                                + "\n\nPaths status: " + (pathsStatus ? "OK" : "Error\n" + errorMessageForPaths);
                }
            }
        }
        catch (Exception e)
        {
            response = "Error reading the cofig file: " + e.Message;
        }

        if (statusPage != null)
        {
            statusPage.WriteInLog(response, error);
        }
        
        context.Response.Write(response);
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}

