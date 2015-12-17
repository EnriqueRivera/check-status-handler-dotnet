# Check Website Status .NET

Check Website status is a library for .NET that permits you check essential aspects in your website, such as:

* **Database connection** - Execute queries and check the status of connections that you have defined on your web.config file.
* **Page status** - Monitor the state of pages on your website.
* **Write permissions on a specific Path** - Define paths and check if you are able to write on them.

## Install
* Add GammapartnersWebsiteStatus.dll as reference on your web application.
* Add CheckWebsiteStatusHandler.ashx on the root path.
* Create a new file called *status.config* on the root path.

## Getting Started
* Install Check Website Status .NET
* Add your preferred configuration on status.config file.

## status.config file
Here you define your configuration to check database, pages and paths.

The main structure of status.config file is as follow:
```xml
<?xml version="1.0"?>

<configuration>
  <queries>
    <!--Add your queries here-->
    <!--<query value="" connectionStringName="" isEntityModel="" onlyCheckConnection="" />-->
  </queries>
  <pages>
    <!--Add your pages here-->
    <!--<page requestUrl="" response="" containsResponse="" timeout=""/>-->
  </pages>
  <paths>
    <!--Add your paths here-->
    <!--<path filePath="" deleteTestFile="" />-->
  </paths>
</configuration>
```

### *query* tag attributes
  * **connectionStringName** (**required attribute**) - Name of the connection string defined on your web.config file.
  * **isEntityModel** (true/false) - Indicate if the connection is used by the Entity Framework. *False as default value*
  * **value** - Query to execute. *"SELECT 1 AS A" as default value*
  * **onlyCheckConnection** (true/false) - Indicate if only will be checked the connection. If this attribute is *false* the query will not executed. *False as default value*

### *page* tag attributes
  * **requestUrl** (**required attribute**) - URL to perform the request.
  * **response** - Indicate which value must or not be contained on the generated response by the request.
  * **containsResponse** (**required when *response* attribute is used**) (true/false) - Indicate if response must or not contains the string indicated as response. *False as default value*
  * **timeout** (true/false) - Time on milliseconds before the request times out. *10000 as default value*

### *path* tag attributes
  * **filePath** (**required attribute**) - Directory on which will be created a test file in order to check write permissions.
  * **deleteFile** (true/false) - Indicate if the created test file must or not be deleted after creation. *false as default value*

## Examples

The value of the next query will be executed because *onlyCheckConnection* attributes is false and it will use the *SiteSqlServer* connection defined on web.config file. You can note that the specified connection string is no for Entity Framework. The query will fail if product quantity is less or equal than 5000 products.
```xml
<query value="SELECT CASE WHEN COUNT(*) &gt; 5000 THEN 1 ELSE 100/0 END AS result FROM Products" connectionStringName="SiteSqlServer" isEntityModel="false" onlyCheckConnection="false" />
```

To only check an Entity Framework connection you can use the following code:
```xml
<query connectionStringName="MyDatabase_Entities" isEntityModel="true" onlyCheckConnection="true"/>
```

With the next line we will check that the text *© 2015 Microsoft* exists on MSDN home page:
```xml
<page requestUrl="https://msdn.microsoft.com/" response="&#169; 2015 Microsoft" containsResponse="true" timeout="5000"/>
```

If you want to check write permissions on a directory of your website you can use this code (note that the created test file will not be deleted):
```xml
<path filePath="C:\inetpub\wwwroot\OurWebsitePath" deleteTestFile="false" />
```
