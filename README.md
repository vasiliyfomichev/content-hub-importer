Overview
===================================

Sitecore Content Hub Importer allows importing image assets from an Excel Asset file, online XML Sitemap, and a URL. For the XML Sitemap and a URL the tool crawls the page(s) and pulls images from the HTML source code, as well as from internal and external CSS and JavaScript.

![Sitecore SignalR Tools](http://www.cmsbestpractices.com/wp-content/uploads/2015/07/sitecore-signalr-tools-logo.png)

The importer is a console C# application built on .NET Core framework.


Usage Instructions
-----------------------------------------
Open the solution in Visual Studio 2019 and either run the application or create an executable, which can be run later. 

Since the application is built using .NET Core, it can be run from various platforms.


Data Source Formats
---------------------------------------

- Excel - an example of the excel file format is included in /data folder of the project
- XML Sitemap - this opion relies on the format provided by https://www.sitemaps.org/protocol.html
- Web URL - a URL to a web page


Contributing
----------------------
If you would like to contribute to this repository, please send a pull request.


License
------------
The project has been developed under the MIT license.


Related Sitecore Projects
--------------------------------
- [Solr for Sitecore](https://github.com/vasiliyfomichev/solr-for-sitecore) - pre-built Solr Docker images ready to be used with Sitecore out of the box.
- [Sitecore ADFS Authenticator Module](https://github.com/vasiliyfomichev/Sitecore-ADFS-Authenticator-Module) - Sitecore module for authenticating users using ADFS.
- [Sitecore Solr Term Highlighter](https://github.com/vasiliyfomichev/Sitecore-Solr-Search-Term-Highlight) - enables search term highlighting in Sitecore search results when used with Solr.



Copyright 2020 Vasiliy Fomichev
