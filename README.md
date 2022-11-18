 ![GitHub](https://img.shields.io/github/license/shells-dw/loupedeck-totalmix)
 ![GitHub last commit](https://img.shields.io/github/last-commit/shells-dw/loupedeck-totalmix)
  ![GitHub downloads](https://img.shields.io/github/downloads/shells-dw/loupedeck-totalmix/total)
 [![Tip](https://img.shields.io/badge/Donate-PayPal-green.svg)]( https://www.paypal.com/donate?hosted_button_id=8KXD334CCEEC2) / [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y4CE9LH)


# Unofficial Loupedeck RME TotalMix FX Plugin

![Overview](/docs/images/overview.png)

## What Is This (and what does it do?)

It's a plugin for the [Loupedeck Live Consoles][Loupedeck] that triggers actions as well as individual channel actions on the [RME TotalMix FX][] application.

It utilizes the OSC protocol support which offers more functionality than MIDI commands, is more solid than MIDI and doesn't interfere with your already existing MIDI setup.

## Release / Installation

You can find the precompiled plugin lplug4 file in the [Releases][Releases]. Download and open it, your computer should already recognize this as a Loupedeck plugin file and offer to open it with Loupedeck Configuration Console - which will have the plugin available in the list then.

## Setup for OSC

Enable OSC in RME TotalMix FX' settings (let's call it TotalMix from here on for ease of typing) and have it listen to OSC commands.
Note that there are 4 OSC Remote Controllers available. If you already use one, set up two for the plugin specifically.
This plugin uses 2 of them, 1 and 2. 1 is used for the main actions, 2 is used for the background thread that mirrors TotalMix changes to the Loupedeck.

Open "Options" -> "Settings..." in TotalMix, then open the tab "OSC".
Make sure Remote Controller 1 has a checkmark next to "In Use". By default TotalMix will use the ports 7001 and 9001.
Note: on MacOS, you'll have to enter "127.0.0.1" in the Remote Controller Address textbox, otherwise it will not work.
Please also set the "Number of faders per bank" to 16, or it will not fully work.
Then, do the same for Remote Controller 2. It will default to 7002 and 9002.
If you (have to) change these ports, make sure updating them in the plugin config as well!

Then, make sure to enable "Enable OSC control". Also link both Remote Controllers to the submix. 

![Setup TotalMix OSC](/docs/images/OSC_setup1.png) ![Setup TotalMix OSC 2](/docs/images/OSC_setup2.png)

No additional software is needed. In theory this should also be able to control a TotalMix instance running on a different computer than the StreamDeck is attached to - as long as you can reach this machine on the given port with UDP packets. 

Note: if you're using a (software) firewall on your PC and/or any firewall between the StreamDeck and the target PC - make sure to allow the plugin to communicate with the TotalMix port as well as allow TotalMix to listen to it. 

## settings.json
%localappdata%\Loupedeck\PluginData\TotalMix containts the file settings.json (which is created with default values during the first start of the plugin and read during every start)
```json
{"host":"127.0.0.1","port":"7001","sendPort":"9001","backgroundPort":"7002","backgroundSendPort":"9002","mirroringEnabled":"true"}
```
where you can configure non-default values or the TotalMix connection.
Note: mirroring of TotalMix is on by default (when you change some values in TotalMix itself or by other means outside the Loupedeck it will be reflected on the Loupedeck plugin), but it has a slight delay as the plugin queries TotalMix. It can have a bit of an performance impact constantly querying the data, which shouldn't be noticeable on most machines, but just in case mirroring isn't needed or wanted, it can be turned off here.
Also note, mirroring only works for all the main functions, mute, solo, phantom power, volume, gain and pan.

## Usage
### General

You have the following options:

![Available Actions](/docs/images/LC_actions.png)

- Input/Output/Playback Channel Dials: Dial controls to change Volume, Pan and Gain

- Input/Output/Playback Channel Buttons: Trigger actions - mute, solo, phantom power, phase/phase right, hi-z, pad, M/S processing

- Snapshots: Load snapshot/mixes.

- Master Channel: Main Channel settings: global mute/solo, dim, speaker B, recall, mute FX, mono, ext. in, show TotalMix UI (Windows only currently)

## Actions

### General

It's important to understand that whenever you can select a channel in the dropdown selection in this plugin, this affects TotalMix channel strips as you see them in the software. TotalMix combines a stereo channel to one channel strip. You will not have control over each individual mono channel that's part of a stereo channel.
What that means is that if you have, for example, a stereo output channel AN1/2, this will be output channel 1. AN3 will be output channel 2 then. However if you have AN1 and AN2 set to mono, AN3 is output channel 3 then. Bear that in mind when you configure actions that are targeted to individual channels across multiple snapshots/mixes or when you change your channel layouts in regards to mono/stereo channels in TotalMix as this will likely break those actions on the StreamDeck and trigger actions on the wrong channels.

Sadly I can't do anything about that, it's just how TotalMix works currently.

# Limitations

- MIDI: I decided against MIDI support, MIDI functionality is already included in the Loupedeck and requires additional software (virtual MIDI loopback interface) to run, also I'm not sure anyone would actually benefit from supporting MIDI, so I consider it a lot of code for not so much value. Let me know if you desperately need it though.
- Actions are currently limited to somewhat basic functions that trigger and I can test. I have a Fireface UC available which doesn't support FX, so I can't really test that. Let me know if that's something you need to plugin to support. Talkback is active as long as the button is pressed, which is something I have not yet implemented (as it's the only function that acts that way AFAIK). 

# I have an issue or miss a feature?

You can submit an issue or request a feature with [GitHub issues]. Please describe as good as possible what went wrong or doesn't act like you'd expect it to. 
As described above I developed this with a Fireface UC which is the only device I have at home and with that constant access to so debugging/developing for any other RME device might not be the the easiest task, but I'll see what I can do.

# Contribute

If you're interested in using this plugin but something you really need is missing, let me know. I naturally don't have access to all RME devices, so I can't really try things on the boxes themselves, but eventually we might find a way to work something out.

# Support

If you'd like to drop me a coffee for the hours I've spent on this: [![Tip](https://img.shields.io/badge/Donate-PayPal-green.svg)]( https://www.paypal.com/donate?hosted_button_id=8KXD334CCEEC2) or use Ko-Fi [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y4CE9LH)


# Changelog
## [1.0.0] - 2022-11-18
### Added
Initial release


<!-- Reference Links -->

[Loupedeck]: https://loupedeck.com "Loupedeck.com"
[Loupedeck]: https://github.com/shells-dw/loupedeck-totalmix/releases "Releases"
[RME TotalMix FX]: https://www.rme-audio.de/totalmix-fx.html "RME's TotalmMix FX product page"
[GitHub issues]: https://github.com/shells-dw/streamdeck-totalmix/issues "GitHub issues link"

