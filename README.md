# GalleryServer

Gallery Server is a Digital Asset Management and Media Gallery application for sharing and managing photos, videos, audio, and other files over the web. Easily add thousands of files using one-click synchronization. Includes support for metadata reading/writing, watermarking, video and audio transcoding, security management, and more. 100% managed code written in C# and ASP.NET 4.5. 

## Getting Started

Clone this repo and open TIS.GSP.sln in Visual Studio 2015 or higher. Hit F5 to compile and run. The browser should start and prompt you to create an admin account. Pretty easy, huh?

By default the data will be stored in a SQL CE database file in the App_Data directory. To use SQL Server, which gives much better performance, open web.config and switch the connection string to the included SQL Server one. Gallery Server requires a database account with a minimum of db_owner permission to the gallery database. If the account does not have permission to create the database, use SSMS to create the empty database first. The next time you access the gallery in the browser, Gallery Server will configure the database with the tables and seed data.

See the deployment section for notes on how to deploy the project to a server.

### Prerequisites

To compile the code, use Visual Studio 2015 or higher with the [Bundler & Minifier](https://github.com/madskristensen/BundlerMinifier) extension.

To run the gallery on a web server, you need
* .NET Framework 4.5 or higher
* Internet Information Services (IIS) 7.0 or higher, or a compatible web server
* Optional: SQL Server 2008 or higher, including the free Express versions.
* Optional: SMTP server (for e-mail functionality).
* Optional: ffmpeg.exe (for video transcoding)
* Optional: Ghostscript (for generating thumbnails for certain file types)
* Optional: ImageMagick (for generating images for certain file types)

### Deploying to a Web Server

Instructions are in the [Admin Guide](https://github.com/rdogmartin/GalleryServer/blob/master/releases/GalleryServerAdminGuide.pdf)

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Roger Martin** - *Initial work* - [rdogmartin](https://github.com/rdogmartin)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc
