# GalleryServer

Gallery Server is a Digital Asset Management and Media Gallery application for sharing and managing photos, videos, audio, and other files over the web. Easily add thousands of files using one-click synchronization. Includes support for metadata reading/writing, watermarking, video and audio transcoding, security management, and more. 100% managed code written in C# and ASP.NET 4.5. 

## Getting Started

Clone this repo and open TIS.GSP.sln in Visual Studio 2015 or higher. Hit F5 to compile and run. The browser should start and prompt you to create an admin account. Pretty easy, huh?

By default the data will be stored in a SQL CE database file in the App_Data directory. To use SQL Server, which gives much better performance, open web.config and switch the connection string to the included SQL Server one. Gallery Server requires a database account with a minimum of db_owner permission to the gallery database. If the account does not have permission to create the database, use SSMS to create the empty database first. The next time you access the gallery in the browser, Gallery Server will configure the database with the tables and seed data.

See deployment for notes on how to deploy the project to a server.

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


```
Give examples
```

### Installing

A step by step series of examples that tell you how to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Billie Thompson** - *Initial work* - [PurpleBooth](https://github.com/PurpleBooth)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc
