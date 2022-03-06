# GalleryServer

Gallery Server is a Digital Asset Management and Media Gallery application for sharing and managing photos, videos, audio, and other files over the web. Easily add thousands of files using one-click synchronization. Includes support for metadata reading/writing, watermarking, video and audio transcoding, security management, and more. 100% managed code written in C# and ASP.NET 4.5.

For more information, visit [galleryserverpro.com](https://galleryserverpro.com).

## Getting Started

If you want to skip the source code and install or upgrade from the compiled binaries, download the desired one from the Releases tab. Refer to the Admin Guide for installation and upgrade instructions.

To get started from the source code, clone this repo and open TIS.GSP.sln in Visual Studio 2015 or higher. Hit F5 to compile and run. The browser should start and prompt you to create an admin account. Pretty easy, huh?

By default the data will be stored in a SQL CE database file in the App_Data directory. To use SQL Server, which has better performance, open web.config and switch the connection string to the included SQL Server one. Gallery Server requires a database account with a minimum of db_owner permission to the gallery database. If the account does not have permission to create the database, use SSMS to create the empty database first. The next time you access the gallery in the browser, Gallery Server will configure the database with the tables and seed data.

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

Instructions are in the [Admin Guide](https://github.com/rdogmartin/GalleryServer/releases/download/v4.5.0/GalleryServerAdminGuide.pdf)

## Author

**Roger Martin**
* GitHub: [https://github.com/rdogmartin](https://github.com/rdogmartin) 
* LinkedIn: [https://www.linkedin.com/in/rdmartin33/](https://www.linkedin.com/in/rdmartin33/)

## License

This project is licensed under the [GPL-3.0 License](https://github.com/rdogmartin/GalleryServer/blob/master/LICENSE).
