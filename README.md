# Custom Radio Stations for GTA V

![](https://cdn.discordapp.com/attachments/522091797216755712/539716314806222878/Grand_Theft_Auto_V_Screenshot_2019.01.29_-_08.57.21.32.png)

## Just trying to install?
If you are looking for help on how to install and use this mod, please see [this thread](https://forums.gta5-mods.com/topic/22729/script-wip-custom-radio-stations-more-radio-wheels-configurable-tracklists-and-more) for info.

## Building / Contributing

* You will need to install ScriptHookVDotNet 3.6.0 via the NuGet package manager; it is already referenced in the project.
* You will need to download a version of irrKlang (ex: irrKlang-64bit-1.6.0) from [here](https://www.ambiera.com/irrklang/downloads.html) and copy the folder to match the relative path `\packages\irrKlang-64bit-1.6.0\bin\dotnet-4-64\irrKlang.NET4.dll`
* Assets are not included in this repo. You can grab them from the latest official download [here](https://www.gta5-mods.com/scripts/custom-radio-stations-net#description_tab).

## FiveM version

Not possible to use IrrKlang or many other libraries client side FiveM C# unless used a BlazorApp.
For servers it doesn't make a lot of sense to be sending down too much audio anyway so I decided to update just for internet radios.

The single player version scans directories and files to populate radio wheels and stations but it's not possible here.
Static files like radio icons, settings and config are stored in `wwwroot`, this is included in the `fxmanifest.lua`.
The config and settings are untouched from the SP version and still used in this.

### `wwwroot/radios`
`categories.json` holds the configuration for populating radios and more can be added here.
A texture will be created if a station has a matching `.png`.