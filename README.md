# AAX Audio Converter
Convert Audible aax files to mp3 and m4a/m4b

![](res/Cover.png?raw=true)

*More [Screenshots](res/Screenshots.md)*

## Main Features
- **Free** and **Open Source** software.
- Converts Audible proprietary .aax files to plain .mp3 or .m4a/.m4b. 
- Also offers basic support for older .aa files.
- Windows application, with all the classic features of the Windows eco-system.
- Requires activated Audible Manager/App or personal activation code.
- Processing Modes: 
  - One output file per input file.
  - Multiple output files per input file, divided by chapter.
  - Multiple output files per input file, divided by chapter and further split into shorter tracks of roughly equal length. 
  - Multiple output files per input file,  split into shorter tracks of roughly equal length, ignoring chapters.
- Creates additional playlist if more than one output file is created per book.
- Handles books with multiple parts.
- Manages and preserves all meta-tag information.
- Supports named chapters, for .aax files downloaded with the Audible App.
- Allows customization of output naming: files, folders and tags.
- Delegates all audio processing to powerful [FFmpeg](https://www.ffmpeg.org/), including DRM handling.
- High performance: Utilizes all available processor cores to run conversion work in parallel.
- Optionally copies original .aax file to a new location after the conversion, with a customizable name.
- Automatically launches default media player after conversion has completed.
- Log facility, optional, to record program activity, activated with program argument.
- Online update function: Will automatically scan the website for a new version, download and install it.
- *Technical*: 
  - .Net Framework application, written in C# with Windows Forms, the Task Parallel Library and other goodies from language and framework. 
  - Incorporating a number of snippets from the Open Source community. 

##  System Environment
AAX Audio Converter will run on Windows 7 and above.

The application requires .Net Framework 4.7.1 to be installed. On Windows 10 systems this should normally be the case, if the system is kept up to date. On older Windows versions, the Framework may have to be installed separately. AAX Audio Converter will detect the missing Framework and provide a link to the download, automatically opening the relevant Microsoft web page. 

AAX Audio Converter is configured to support high DPI monitors under Windows 10. It will scale properly when the user changes the DPI or scale factor. 

## Download

Windows setup package version 1.12, English and German, with manuals, plus FFmpeg executable:

**[AaxAudioConverter-1.12-Setup.exe](https://github.com/audiamus/AaxAudioConverter/releases/download/v1.12/AaxAudioConverter-1.12-Setup.exe)**

Manuals (also included in the setup package):

**[English](https://github.com/audiamus/AaxAudioConverter/releases/download/v1.12/AaxAudioConverter.pdf)**

**[German](https://github.com/audiamus/AaxAudioConverter/releases/download/v1.12/AaxAudioConverter.de.pdf)**


## Dependencies
### Audible account
AAX Audio Converter needs the user's personal Audible activation code to be able to process his/her Audible audiobooks.
The easiest way to obtain the Audible activation code is to install and activate “Audible Manager” or “Audible App”. Audible Manager/App can be downloaded from the Audible website, on the software page. With Audible Manager or App installed and activated, i.e. associated with the Audible account, AAX Audio Converter should be able to find the activation code automatically.

### FFmpeg
All audio processing in AAX Audio Converter, including DRM handling, is carried out by [FFmpeg](https://www.ffmpeg.org/). 
The AAX Audio Converter installation package comes pre-bundled with a suitable FFmpeg.exe. 

## Feedback
Comments and questions can be posted in the AAX Audio Converter chatroom on Gitter:

[![Gitter](https://badges.gitter.im/AaxAudioConverter/community.svg)](https://gitter.im/AaxAudioConverter/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

For more serious matters, like bug reports and other unexpected behavior, visit the [issue page](https://github.com/audiamus/AaxAudioConverter/issues) here on GitHub. For exceptions, please provide the call stack as shown by the program. For diagnosis of unexpected behavior you can run AAX Audio Converter with the log option activated. Zip-compress the log file and upload/attach it to the issue, but read the privacy note in the manual first.    

## Anti-Piracy Notice
Note that this software does not ‘crack’ the DRM or circumvent it in any other way. The application simply applies the user's own activation code (associated with his/her personal Audible account) to decrypt the audiobook in the same manner as the official audiobook playing software does. 

Please only use this application for gaining full access to your own audiobooks for archiving/conversion/convenience. De-DRMed audiobooks must not be uploaded to open servers, torrents, or other methods of mass distribution. No help will be given to people doing such things. Authors, retailers and publishers all need to make a living, so that they can continue to produce audiobooks for us to listen to and enjoy.

(*This blurb is borrowed from https://github.com/KrumpetPirate/AAXtoMP3 and https://apprenticealf.wordpress.com/*). 
