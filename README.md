# CustomFeedProvider

A proof-of-concept for custom feeds in Windows Widgets.

![Screenshot of Widget's Custom Feeds feature](docs/resources/hero_image.png)

## Requirements
### Enabling the feature
- Velocity IDs 44353396 and 45393399 enabled.
- Device region must be a country within the European Economic Area.
  - To change your region, follow [this tutorial](https://www.neowin.net/guides/how-to-remove-microsoft-edge-from-windows-11-in-the-latest-eea-compliant-update/).
### Building the POC
- Visual Studio 2022
  - Required workflows:
    - Universal Windows Platform development
    - Desktop development with C++ (for the C++ sample)
    - .NET desktop development and Windows App SDK C# templates component (for the C# sample)
- Windows SDK version 22621
- [Windows App SDK 1.5 Experimental 1 framework package](docs/resources/MSIX) installed.