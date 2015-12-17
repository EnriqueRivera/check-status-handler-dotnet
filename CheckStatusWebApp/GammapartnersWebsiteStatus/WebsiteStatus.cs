/*
    Plugin Name: Website status
    Plugin URI: https://github.com/gammapartners
    Description: Check the status of databases, write permissions and page content on your website
    Version: 1.0
    Author: Enrique Alonso Rivera Nevarez
    Author URI: https://github.com/EnriqueRivera/
    License:     GPL2
    License URI: https://www.gnu.org/licenses/gpl-2.0.html
*/

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Xml;
using System.Data.EntityClient;

namespace GammapartnersWebsiteStatus
{
    /// <summary>
    /// This class expose methods to check the status of different aspects of your website.
    /// You can check the status of databases, write permissions and page content.
    /// </summary>
    public class WebsiteStatus
    {
        /// <summary>
        /// status.config content as string
        /// </summary>
        private string _rawConfigFile = string.Empty;

        /// <summary>
        /// status.config content as XmlDocument
        /// </summary>
        private XmlDocument _xmlConfigFile = new XmlDocument();

        /// <summary>
        /// Initialize the object with status configuration file
        /// </summary>
        /// <param name="configFile">status.config content</param>
        public WebsiteStatus(string configFile)
        {
            _rawConfigFile = configFile;
            _xmlConfigFile.LoadXml(configFile);
        }

        /// <summary>
        /// Check all the queries of status.config fileC
        /// </summary>
        /// <param name="errorMessage">Error message if an exception is triggered</param>
        /// <returns>True if the queries were executed successfully, false if otherwise</returns>
        public bool CheckQueriesStatus(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                return this.CheckQueries(out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "-> Exception " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Check all the pages of status.config file
        /// </summary>
        /// <param name="errorMessage">Error message if an exception is triggered</param>
        /// <returns>True if the pages were requested successfully, false if otherwise</returns>
        public bool CheckPagesStatus(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                return this.CheckPages(out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "-> Exception " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Check all the paths of status.config file
        /// </summary>
        /// <param name="errorMessage">Error message if an exception is triggered</param>
        /// <returns>True if was possible to write on the specified paths, false if otherwise</returns>
        public bool CheckPathsStatus(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                return this.CheckPaths(out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = "-> Exception " + ex.Message;
                return false;
            }
        }

        private bool CheckQueries(out string errorMessage)
        {
            string defaultQuery = "SELECT 1 AS A";
            errorMessage = string.Empty;
            XmlNodeList xmlNodeList = _xmlConfigFile.DocumentElement.SelectNodes("//configuration/queries/query");
            
            for (int i = 0; i < xmlNodeList.Count; ++i)
            {
                try
                {
                    string connectionStringName = GetAttribute(xmlNodeList[i], "connectionStringName", true);
                    string isEntityModel = GetAttribute(xmlNodeList[i], "isEntityModel", false);
                    string onlyCheckConnection = GetAttribute(xmlNodeList[i], "onlyCheckConnection", false);
                    string value = GetAttribute(xmlNodeList[i], "value", false);

                    string cmdText = string.IsNullOrEmpty(value) ? defaultQuery : value;
                    ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

                    if (connectionStringSettings == null)
                    {
                        throw new Exception(string.Format("Connection string was not found in web.config file", connectionStringName));
                    }

                    string connectionString = connectionStringSettings.ConnectionString;
                    if (Convert.ToBoolean(isEntityModel))
                    {
                        EntityConnectionStringBuilder connectionStringBuilder = new EntityConnectionStringBuilder();
                        connectionStringBuilder.ConnectionString = connectionStringSettings.ConnectionString;

                        SqlConnectionStringBuilder connectionStringForEM = new SqlConnectionStringBuilder();
                        connectionStringForEM.ConnectionString = connectionStringBuilder.ProviderConnectionString;
                        connectionStringForEM.ConnectTimeout = 1;

                        connectionString = connectionStringForEM.ConnectionString;
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        if (Convert.ToBoolean(onlyCheckConnection) == false)
                        {
                            using (SqlDataReader sqlDataReader = new SqlCommand(cmdText, connection).ExecuteReader())
                            {
                                if (sqlDataReader.Read() == false)
                                {
                                    throw new Exception("No data found executing the query");
                                }
                            }
                        }

                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    string error = string.Format("-> Exception (Query #{0}): {1}", i + 1, ex.Message);
                    errorMessage += string.IsNullOrEmpty(errorMessage) ? error : "\n" + error;
                }
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        private bool CheckPages(out string errorMessage)
        {
            errorMessage = string.Empty;
            int timeout = 0;
            XmlNodeList xmlNodeList = _xmlConfigFile.DocumentElement.SelectNodes("//configuration/pages/page");

            for (int i = 0; i < xmlNodeList.Count; ++i)
            {
                try
                {
                    string requestUrl = GetAttribute(xmlNodeList[i], "requestUrl", true);
                    string timeoutAttribute = GetAttribute(xmlNodeList[i], "timeout", false);
                    string responseAttribute = GetAttribute(xmlNodeList[i], "response", false);

                    WebRequest webRequest = WebRequest.Create(requestUrl);
                    webRequest.Timeout = int.TryParse(timeoutAttribute, out timeout) ? timeout : 10000;

                    string response = string.Empty;
                    using (StreamReader streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                    {
                        response = streamReader.ReadToEnd();
                    }

                    if (string.IsNullOrEmpty(responseAttribute) == false)
                    {
                        string containsResponse = GetAttribute(xmlNodeList[i], "containsResponse", true);

                        if (Convert.ToBoolean(containsResponse))
                        {
                            if (response.Contains(responseAttribute) == false)
                            {
                                throw new Exception("Invalid response");
                            }
                        }
                        else if (response.Contains(responseAttribute))
                        {
                            throw new Exception("Invalid response");
                        }
                    }
                }
                catch (Exception ex)
                {
                    string error = string.Format("-> Exception (Page #{0}): {1}", i + 1, ex.Message);
                    errorMessage += string.IsNullOrEmpty(errorMessage) ? error : "\n" + error;
                }
            }
            return string.IsNullOrEmpty(errorMessage);
        }
        
        private bool CheckPaths(out string errorMessage)
        {
            errorMessage = string.Empty;
            XmlNodeList xmlNodeList = _xmlConfigFile.DocumentElement.SelectNodes("//configuration/paths/path");
            for (int i = 0; i < xmlNodeList.Count; ++i)
            {
                try
                {
                    string filePath = GetAttribute(xmlNodeList[i], "filePath", true);
                    string deleteTestFile = GetAttribute(xmlNodeList[i], "deleteTestFile", false);
                    

                    if (Directory.Exists(filePath) == false)
                    {
                        throw new Exception("The specified path does not exist");
                    }

                    string fullPath = System.IO.Path.Combine(filePath, string.Format("Test File {0}.txt", DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss-ffff")));

                    System.IO.File.Create(fullPath).Dispose();
                    using (TextWriter textWriter = (TextWriter)new StreamWriter(fullPath))
                    {
                        textWriter.WriteLine("Test");
                        textWriter.Close();
                    }

                    if (Convert.ToBoolean(deleteTestFile))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                catch (Exception ex)
                {
                    string error = string.Format("-> Exception (Path #{0}): {1}", i + 1, ex.Message);
                    errorMessage += string.IsNullOrEmpty(errorMessage) ? error : "\n" + error;
                }
            }
            return string.IsNullOrEmpty(errorMessage);
        }

        private string GetAttribute(XmlNode xmlNode, string attributeName, bool isRequired)
        {
            XmlAttribute attribute = xmlNode.Attributes[attributeName];
            if (attribute == null)
            {
                if (isRequired)
                {
                    throw new Exception(attributeName + " attribute not found");
                }

                return string.Empty;
            }

            return attribute.Value;
        }
    }
}
