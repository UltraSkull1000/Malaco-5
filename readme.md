# Malaco-5 TTRPG Utility Bot
This bot was created as a passion project by McElroy Ruman (UltraSkull1000). Its main branch is compiled with .Net 8.0 and uses the Discord.Net library.

## Required libraries:
- [Discord.Net](https://github.com/discord-net/Discord.Net)
- [Oestus](https://github.com/UltraSkull1000/Oestus)
- [libopus & libsodium](https://github.com/discord-net/Discord.Net/tree/8b929690a9a89a9aa8a8b8e43ec0dd2e1ab685db/voice-natives)
- [ffmpeg](https://www.ffmpeg.org/download.html)

## To build
1. `git clone https://github.com/UltraSkull1000/Malaco-5 Malaco-5`
2. `cd Malaco-5`
3. `dotnet build Malaco\ 5.generated.sln -o build`

## To Run
1. Navigate to `Malaco-5/build`
2. Run `sudo dotnet Malaco\ 5.dll`

On first time setup, you will be asked to input your Discord Bot token. Copy and paste your token into the terminal.

If you would like to have the bot loop through a set of statuses, create a status.txt file in the build folder, and populate each line with a status. After you restart the bot, it will scroll through the list randomly. 