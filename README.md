# STMX - Stats on Tmux
Simple commandline utility to get systems stats.

> [!NOTE]
> Only supports 64bit Linux at the moment.

### Features
* CPU
  - Gets CPU utilization as a percentage
* Memory
  - Memory utilization as a percentage OR
  - CURRENT / TOTAL format in both base2 and base10 memory units.
* Battery
  - Current status - Charging or Discharging
  - Current capacity as percentage
* NERDFont icons!

### Examples
#### CPU
```bash
# print cpu usage with icons
$ stmx cpu -i -p
 0.79

# use custom CPU icon
$ stmx cpu -i 'C'
C 0.77

# use custom percent icon
$ stmx cpu -i -p '%'
 0.78%

# use custom CPU and percent icons
$ stmx cpu -i 'CPU' -p '%'
CPU 0.77%
```
#### Memory
```bash
# print memory usage with icons
$ stmx memory -i -p
 19.41

# use custom memory icon
$ stmx memory -i 'M' -p '%'
M 19.35%

# use other memory units
$ stmx memory -i -u GigaBytes
 7 / 32
$ stmx memory -i 'MEM' -u Megabytes
MEM 6219 / 32150
```
#### Battery
```bash
# print battery info with icons
$ stmx battery -i -s -p
󰂁󱐋 89

# use custom percent icon
$ stmx battery -i -s -p '%'
󰂁󱐋 88%
```

### Examples - Tmux
I use following config to get an informative Tmux status bar.
```conf
FG="#ffffff"
BG="#005f87"

set-option -g status-right "\
#[fg=${FG},bg=${BG}] \
#(stmx battery -ips) | \
#(stmx memory -ip) | \
#(stmx cpu -i) | \
%h %d %H:%M "
set -g status-right-length 100
```

![Tmux screenshot](/images/tmux-status-bar-screenshot.png)

### Requirements
- The binaries are self-contained and are available as part of each release, but you will need [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) if you want to build it yourself.
- Icons use [Nerd Fonts](https://www.nerdfonts.com/)
