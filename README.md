# STMX - Stats on Tmux
Simple commandline utility to get systems stats.

> [!NOTE]
> Only supports 64bit Linux at the moment.

### Features
* CPU
  - Gets CPU utilization as a percentage
* Memory
  - Memory utilization as a percentage in CURRENT / TOTAL format.
  - Supports both base2 and base10 memory units.
* Battery
  - Charging or Discharging
  - Current capacity
* All commands also support displaying and icon using `-i` flag.

### Examples - Commandline
```sh
$ stmx cpu
0.78%
$ stmx cpu -i
 0.78%

$ stmx memory
6384252 / 32151268
$ stmx memory -i
 6385076 / 32151268
$ stmx memory -i -p
 19.83%
$ stmx memory -i -u GigaBytes
 7 / 32
$ stmx memory -i -u MegaBytes
 6366 / 32151

$ stmx battery -i -p -c
󰁾󱐋 59%
$ stmx battery -i -p
󰁾 59%
$ stmx battery -p
60%
$ stmx battery
60
```

### Examples - Tmux
I use [o0th/tmux-nova](https://github.com/o0th/tmux-nova) with following config to get an informative Tmux status bar.
```conf
set -g @plugin 'o0th/tmux-nova'

set -g @nova-segment-custom '#(stmx battery -i -p -c) | #(stmx memory -i -p) | #(stmx cpu -i) | %h %d %H:%M'
set -g @nova-segments-0-right "custom"
```

![Tmux screenshot](/images/tmux-status-bar-screenshot.png)

### Requirements
- The binaries are self-contained and are available as part of each release, but you will need [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) if you want to build it yourself.
- Icons use [Nerd Fonts](https://www.nerdfonts.com/)
