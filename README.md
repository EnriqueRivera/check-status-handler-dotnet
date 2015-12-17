# Check Website Status .NET

Check Website status is a library for .NET that permits you check essential aspects in your website, such as:

* **Database connection** - Execute queries and check the status of connections that you have defined on your web.config file.
* **Page status** - Monitor the state of pages on your website.
* **Write permissions on a specific Path** - Define paths and check if you are able to write on them.

## Install
* Add GammapartnersWebsiteStatus.dll as reference on your web application.
* Add CheckWebsiteStatusHandler.ashx on the root path.
* Create a new file called "status.config" on the root path.

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

### "query" tag attributes
  * connectionStringName (**required attribute**): Name of the connection string defined on your web.config file
  * isEntityModel:
  * value:
  * onlyCheckConnection:

### "page" tag attributes


### "path" tag attributes
