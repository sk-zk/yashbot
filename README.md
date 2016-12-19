**yashbot** is a ~~ball of glue code~~ tool for batch-processing YouTube videos for Audiosurf 2 faster and out-game. 

Note that Steam has to be open to use it and that you will be shown as playing Audiosurf 2 while it's running.

###Requirements:

BASS  
CSteamworks  
ffmpeg  
steam_api.dll  
Steamworks.NET  
TagLib#  
UnityMediaPlayer.dll  
youtube-dl  

###Installation:

1) Copy bass.dll, steam_api.dll and UnityMediaPlayer.dll from steamapps\common\Audiosurf 2 to the yashbot directory.

2) Download ffmpeg and place ffmpeg.exe and ffprobe.exe in the yashbot directory.

###Usage examples:

To process a single video:

    yashbot _BSBdUcOyW4
    
To process multiple videos:

    yashbot _BSBdUcOyW4 eYewBRULpec DKG13lYFWrg
    
You can also pass a text file containing video IDs (seperated by line breaks):

    yashbot lotsofvideos.txt
