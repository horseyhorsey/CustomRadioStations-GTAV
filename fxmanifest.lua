fx_version 'bodacious'
game 'gta5'
author 'HorsePin, LfxB, crosire'
version '1.1.3'
description 'fivem custom internet radios wheels, client only'

files {
	'Client/wwwroot/**',	
	'Client/bin/Release/**/**/publish/*.dll',
}

client_script 'Client/bin/Release/**/publish/*.net.dll'
ui_page('Client/wwwroot/index.html');

fxdk_watch_command 'dotnet' {'watch', '--project', 'Client/CustomRadioStations.csproj', 'publish', '--configuration', 'Release'}