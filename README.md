 ![GitHub last commit](https://img.shields.io/github/last-commit/shells-dw/loupedeck-totalmix)
 [![Tip](https://img.shields.io/badge/Donate-PayPal-green.svg)]( https://www.paypal.com/donate?hosted_button_id=8KXD334CCEEC2) / [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y4CE9LH)


# Unofficial Loupedeck RME TotalMix FX Plugin

![Overview](/TotalMixPlugin/docs/images/overview.png)

## What Is This (and what does it do?)

It's a plugin for the [Loupedeck Live Consoles][Loupedeck] that triggers actions as well as individual channel actions on the [RME TotalMix FX][] application. Note: a RME audio interface/card is needed for TotalMix FX to work.

It utilizes the OSC protocol support which offers more functionality than MIDI commands, is more solid than MIDI and doesn't interfere with your already existing MIDI setup.

## Release / Installation

You can find the precompiled plugin lplug4 file in the [Releases][Releases]. Download and open it, your computer should already recognize this as a Loupedeck plugin file and offer to open it with Loupedeck Configuration Console - which will have the plugin available in the list then.

## Setup for OSC

Enable OSC in RME TotalMix FX' settings (let's call it TotalMix from here on for ease of typing) and have it listen to OSC commands.
Note that there are 4 OSC Remote Controllers available. If you already use one, set up two for the plugin specifically.
This plugin uses 2 of them, 1 and 2. 1 is used for the main actions, 2 is used for the background thread that mirrors TotalMix changes to the Loupedeck.

- Open "Options" -> "Settings..." in TotalMix, then open the tab "OSC".
- Make sure Remote Controller 1 has a checkmark next to "In Use". By default TotalMix will use the ports 7001 and 9001.

Note: on MacOS, you'll have to enter "127.0.0.1" in the Remote Controller Address textbox, otherwise it will not work.

- In the bottom, set the "Number of faders per bank" setting to reflect the amount of channels your interface offers (the plugin will offer channels based on that value).
- Then, do the same for Remote Controller 2. It will default to 7002 and 9002.

If you (have to) change these ports, make sure updating them in the plugin config as well!

Then, make sure to enable "Enable OSC control". Also link both Remote Controllers to the submix. 

![Setup TotalMix OSC](/TotalMixPlugin/docs/images/OSC_setup1.png) ![Setup TotalMix OSC 2](/TotalMixPlugin/docs/images/OSC_setup2.png)

No additional software is needed. In theory this should also be able to control a TotalMix instance running on a different computer than the Loupedeck is attached to - as long as you can reach this machine on the given port with UDP packets. 

Note: if you're using a (software) firewall on your PC and/or any firewall between the Loupedeck and the target PC - make sure to allow the plugin to communicate with the TotalMix port as well as allow TotalMix to listen to it. 

## settings.json
Windows: %localappdata%\Loupedeck\PluginData\TotalMix
MacOS: /Users/USERNAME/.local/share/Loupedeck/PluginData/TotalMix
contains the file settings.json (which is created with default values during the first start of the plugin and read during every start)

```json
{
  "host": "127.0.0.1",
  "port": "7001",
  "sendPort": "9001",
  "backgroundPort": "7002",
  "backgroundSendPort": "9002",
  "mirroringEnabled": "true",
  "skipChecks": "false"
}
```
where you can configure non-default values or the TotalMix connection.

Note: mirroring of TotalMix is on by default (when you change some values in TotalMix itself or by other means outside the Loupedeck it will be reflected on the Loupedeck plugin), but it has a slight delay as the plugin queries TotalMix. It can have a bit of a performance impact constantly querying the data, which shouldn't be noticeable on most machines, but just in case mirroring isn't needed or wanted, it can be turned off here.
Also note, mirroring only works for all the main functions, mute, solo, phantom power, volume, gain and pan.

## Connecting to a TotalMix instance on another host
To be able to use the plugin with another PC, three settings are mandatory:

1. Set the IP of the machine the StreamDeck is connected to in the Setup for OSC part inside TotalMix as described above. Configure as described, but make sure to put the IP address (or FQDN) of the machine the StreamDeck is connected to in the "Remote Controller Address" field. This is needed so TotalMix sends its OSC responses to the computer that runs the plugin.
2. Set the IP of the machine that runs TotalMix in the config file of this plugin (see above) in the value of interfaceIp, so the plugin knows where to send the commands.
3. Make sure both machines have their firewall configured in a way that allows UDP communication on the ports that are set in the configuration (by default 7001 and 9001 UDP inbound to the TotalMix PC/outbound of the StreamDeck-PC, 9002 UDP inbound of the StreamDeck PC and outbound the TotalMix PC).

## Usage
### General

You have the following options:

![Available Actions](/TotalMixPlugin/docs/images/LC_actions.png)

- Input/Output/Playback Channel Dials: Dial controls to change Volume, Pan and Gain

- Input/Output/Playback Channel Buttons: Trigger actions - mute, solo, phantom power, phase/phase right, hi-z, pad, M/S processing

- Snapshots: Load snapshot/mixes.

- Master Channel: Main Channel settings: global mute/solo, dim, speaker B, recall, mute FX, mono, ext. in, show TotalMix UI (Windows only currently)

## Actions

### General considerations about channels

It's important to understand that whenever you can select a channel in the dropdown selection in this plugin, this affects TotalMix channels as you see them in TotalMix mixer view. TotalMix combines a stereo channel to one channel strip. You will not have control over each individual mono channel that's part of a stereo channel.

What that means is that if you have, for example, a stereo output channel AN1/2, both combined channels will be output channel 1. AN3 will be output channel 2 then. However if you have AN1 and AN2 set to mono, AN3 is output channel 3 then.

Keep that in mind when you configure actions that are targeted to individual channels across multiple snapshots/mixes or when you change your channel layouts in regards to mono/stereo channels in TotalMix as this will likely break those actions on the Loupedeck and trigger actions on the wrong channels.

Sadly I can't do anything about that, it's just how TotalMix works currently.

# Limitations

- MIDI: I decided against MIDI support, MIDI functionality is already included in the Loupedeck and requires additional software (virtual MIDI loopback interface) to run, also I'm not sure anyone would actually benefit from supporting MIDI, so I consider it a lot of code for not so much value. Let me know if you desperately need it though.
- Actions are currently limited to somewhat basic functions I can test. I have a Fireface UC available which doesn't support DSP FX, so I can't really develop and test setting individual FX settings.

# I have an issue or miss a feature?

You can submit an issue or request a feature with [GitHub issues]. Please describe as good as possible what went wrong or doesn't act like you'd expect it to. 
As described above I developed this with a Fireface UC which is the only device I have at home and with that constant access to so debugging/developing for any other RME device might not be the the easiest task, but I'll see what I can do.

# Contribute

If you're interested in using this plugin but something you really need is missing, let me know. I naturally don't have access to all RME devices, so I can't really try things on the boxes themselves, but eventually we might find a way to work something out.

# Support

If you'd like to drop me a coffee for the hours I've spent on this:
[![Tip](https://img.shields.io/badge/Donate-PayPal-green.svg)]( https://www.paypal.com/donate?hosted_button_id=8KXD334CCEEC2), sponsor me on [GitHub](https://github.com/sponsors/shells-dw) 
or use Ko-Fi [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y4CE9LH)


# Changelog
## [1.8.3] - 2023-03-28
### Fixed
- Remote OSC instances (on another host) are working now. Please refer to the corresponding documentation, thanks for @roguedarkjedi for pointing that out.
### Improved/Changed
- Added documentation for remote host config.
## [1.8.2] - 2022-12-29
### Added
- added images for EQ, Comp, Autolevel #beautify
- 
<details><summary>Changelog History</summary><p>

## [1.8.1] - 2022-12-29
### Updated
- changed image name to Loupedeck's preferred naming scheme
## [1.8.0] - 2022-12-29
### Updated
- Reworked/finalized logging
## [1.7.1] - 2022-12-27
### Fixed
- Beautified/clearified error message displayed when no connection to TotalMix is possible
### Updated
- Clarifications in README.MD
## [1.7.0] - 2022-12-22
### Addded
- Logging, new config switch to enable/disable logging (note: Logging is for debugging only. It will significantly slow down Loupedeck overall as traces are gathered and written all over the place. DO NOT ENABLE logging unless you are asked to provide logs and disable it afterwards.)
### Fixed
- Custom config file values were overwritten with every plugin start (oops.)
- Channels >9 were not mirrored (but still functioned as actions)
### Fix attempt
- Rare crash caused by waiting for a callback on async reading a socket that was already disposed. I call it fix attempt as I did not yet find the reason why this happens in the first place, it has been a b[...] to track down as it could happen after several hours of plugin runtime or not at all for days on my machine, so for now the exception is caught and the plugin shouldn't crash from it anymore. Depending the actual root cause this may or may not lead to the mirroring getting stuck. Please report issues if it does. As said, I had a hard time to actually get that crash on my machine with my interface.
## [1.6.1] - 2022-12-18
### Fixed
- Master Volume value not updating correctly. Note: removing and replacing the Master Volume dial on the Loupedeck is required to apply the change.
## [1.6.0] - 2022-12-18
### Added
- Option to enable/disable Reverb and Echo
### Improved / fixed
- If TotalMix isn't sending heartbeats, don't try to listen for it. 
### Note
- Global Mute and Global Solo have been moved to new "Global Functions" folder. Existing buttons need to be replaced.
### Misc
- New plugin icon ;)
## [1.5.0] - 2022-12-14
### Added
- Custom Snapshot/Mix names are now read from local TotalMix config file (as they are not available via OSC) and shown during selection as well as on the action
### Improved
- Removed option to show/hide TotalMix UI on MacOS to avoid confusion for MacOS users (currently Win only function, MacOS TODO)
- Call sender improved (avoid unnecessary function call)
- Extended idle period between listenthread runs (should be a bit easier on the system without making things lots slower)
## [1.4.0] - 2022-12-05
### Improved
- switch to track optimised (less calls)
## [1.3.0] - 2022-12-03
### Added
- auto channel count recognition
## [1.2.0] - 2022-12-02
### Added
- multiple action images
## [1.1.0] - 2022-11-29
### Improved
- settings file checking made more robust (in case settings go missing)
- setting.json is now sourced from embedded ressources instead of created
### Added
- option to skip TotalMix availability check

## [1.0.0] - 2022-11-18
### Added
Initial release

</p></details>

<!-- Reference Links -->

[Loupedeck]: https://loupedeck.com "Loupedeck.com"
[Releases]: https://github.com/shells-dw/loupedeck-totalmix/releases "Releases"
[RME TotalMix FX]: https://www.rme-audio.de/totalmix-fx.html "RME's TotalmMix FX product page"
[GitHub issues]: https://github.com/shells-dw/loupedeck-totalmix/issues "GitHub issues link"

