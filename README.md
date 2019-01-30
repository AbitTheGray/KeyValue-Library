# Key-Value Library

C# Library created for parsing [Valve](https://www.valvesoftware.com/en/)'s [KeyValue files](https://developer.valvesoftware.com/wiki/KeyValues) (mostly for [Dota 2](http://blog.dota2.com/?l=english)).

## Usage

This library may not be the fastest or most complex version available.
In some cases, [Perryvw's PHP Valve KV](https://github.com/Perryvw/PHPValveKV) is slightly faster (often not) but is capable of more things (like filtering by operating system). 

## Benchmarks

### Dota 2 Files

| File Name | Average Read Time | Average Write Time | Line Count | Character Count |
|-----------|------------------:|-------------------:|-----------:|----------------:|
| [npc_heroes.txt](https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/game/dota/scripts/npc/npc_heroes.txt) | 52.5028 ms | 16.7411 ms | 21,749 | 578,074 |
| [npc_abilities.txt](https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/game/dota/scripts/npc/npc_abilities.txt) | 91.8989 ms | 26.6094 ms | 60,430 | 2,102,075 |
| [npc_units.txt](https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/game/dota/scripts/npc/npc_units.txt) | 35.6965 ms | 6.8743 ms | 19,692 | 769,978 |
| [items.txt](https://raw.githubusercontent.com/dotabuff/d2vpk/master/dota_pak01/scripts/npc/items.txt) | 33.3005 ms | 7.3291 ms | 11,309 | 372,861 |
| [dota_english.txt](https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/game/dota/resource/dota_english.txt) | 101.8306 ms | 31.9313 ms | 28,800 | 2,486,907 |
| [dota_czech.txt](https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/game/dota/resource/dota_czech.txt) | 129.7638 ms | 41.4225 ms | 28,608 | 2,771,791 |
| [dota_russian.txt](https://raw.githubusercontent.com/SteamDatabase/GameTracking-Dota2/master/game/dota/resource/dota_russian.txt) | 115.8326 ms | 48.4875 ms | 28,726 | 2,741,267 |

## License

For license, look into [LICENSE.md](LICENSE.md) file.
