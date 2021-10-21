# BitTorrentClient
A bittorrent client made in C# with the MonoTorrent library.

It have two versions of it: 
  * Core version (CLI)
  * GUI version (under development). 

Both are independent, they're just different interfaces for the program.

- [How to Install?](#how-to-install)
- [Usage (Core)](#usage-core)

## How to Install?
  * From source:
    ```
    $ git clone git@github.com:FallAngel1337/BitTorrentClient.git
    $ cd BitTorrentClient/
    ```
    * Compiling the Core app:
    ```
    $ cd Client.Core/
    $ dotnet publish -o <output_dir>
    $ cd -o <output_dir>/
    $ dotnet Client.Core.dll --help # For Linux
    $ ./Client.exe --help # For Windows
    ```
    
  * From binary:
    * Linux:
       * Download from [Releases](https://github.com/FallAngel1337/BitTorrentClient/releases)
       ```
       $ tar xvf linux-x64/
       $ cd linux-x64/
       $ dotnet Client.Core.dll --help 
       ```
    * Windows:
      * Download from [Releases](https://github.com/FallAngel1337/BitTorrentClient/releases)
      * Extract the tarball
      ```
      $ cd windows-x64/
      $ ./Client.Core.exe --help
      ```

## Usage (Core)
   **TODO: Add a help function for better output**
```./Client.Core --help
Client 1.0.0
Copyright (C) 2021 Client

ERROR(S):
  Required option 'l, load' is missing.

  -v, --verbose    Enable verbose mode

  -l, --load       Required. The torrent file/link/hash

  -d, --path       Download path

  --help           Display this help screen.

  --version        Display version information.

Could not parse the arguments!
```

  * Basic usage:
    
    `./Client.Core -l <path_to_torrent_file>`
