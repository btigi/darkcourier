# ii.SimpleZip

A NET Standard 2.0 class library to create zip files using the STORE algorighm (i.e. without compression).

## Usage

Instantiate the SimpleZipFile class and call Create. The parameters are:
- The path to the directory to archive
- The path and filename to write the achive file to
- An estimate of the number of files to be archived (optional, defaults to 55,000)


## Download

No compiled download is available.


## Technologies

ii.SimpleZip is written in C# targetting NET Standard 2.0


## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/ii.SimpleZip

# Go into the repository
$ cd ii.SimpleZip

# Build  the app
$ dotnet build
```


## License

ii.SimpleZip is licensed under [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/)
 
The CRC32 implementation contained within iiSimpleZip is licenced from Steve McMahon under Creative Commons 1.0 (http://web.archive.org/web/20080409214945/http://creativecommons.org/licenses/by/1.0)
Original code available at http://web.archive.org/web/20080405143930/http://www.vbaccelerator.com/home/net/code/Libraries/CRC32/article.asp