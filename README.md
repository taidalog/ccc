# ccc

Version 0.11.0

[Japanese README](README.ja.md)

ccc: customizable command countdown-timer.

## Synopsis

- A customizable command countdown-timer. Suitable for presentation.

## Usage

1. Visit [ccc](https://taidalog.github.io/ccc/).
1. Type or paste commands.
1. Press `Enter` to start the timer. Press `Escape` to stop the timer. Press `Delete` to reset the timer.

## Commands

| Command       | Feature    |
| ------------- | ---------- |
| `down <time>` | Countdown. |
| `up <time>`   | Count up.  |

## Options

| Options                        | Feature                                                                                                  |
| ------------------------------ | -------------------------------------------------------------------------------------------------------- |
| `--color\|-c <hex code>`       | Specifies text color. Default: `#333333`.                                                                |
| `--background\|-bg <hex code>` | Specifies background color. Default: `#ffffff`.                                                          |
| `--message\|-m <text>`         | Specifies message to show below the timer. Default: an empty string.                                     |
| `--pause\|-p`                  | Specifies that the timer should stop when the time spesified with the command has passed.                |
| `--alarm\|-a beep\|bell`       | Specifies that the timer should play an alarm sound when the time spesified with the command has passed. |

## Examples

```
down 5:00
```

Countdown for 5 minutes

```
up 5:00
```

Counts up for 5 minutes

```
down 5:00 down 1:30
```

Counts down for 5 minutes, then counts down for 1 minute and 30 seconds.

```
down 5:00 --message hey
```

Counts down for 5 minutes, with a message "hey" below the timer.

```
down 5:00 --color #222266 --background #aaccff --message hello
```

Counts down for 5 minutes, with the dark blue text, the pale blue background and a message "hello" below the timer.

```
down 5:00 --message Presentation. up 120 -bg #aaccff -m Questions and answers. -c #222266
```

Counts down for 5 minutes, with a message "Presentation." below the timer. Then counts up for 120 seconds (2 minutes), with the dark blue text and the pale blue background and a message "Questions and answers.".

```
down 5:00 --pause up 2:00
```

Counts down for 5 minutes, and then the timer pauses. Then starts counting up for 2 minutes when user restarts the timer.

```
down 5:00 --alarm bell up 2:00 -a beep
```

Counts down for 5 minutes, and then plays the tinkle of a bell. Then counts up for 2 minutes and plays a beep sound.

## Keys

| Key                            | Feature                                                                                     |
| ------------------------------ | ------------------------------------------------------------------------------------------- |
| `Enter`                        | Starts the timer.                                                                           |
| `Escape`                       | Stops the timer.<br>Pressing `Enter` key restarts the timer.                                |
| `Delete` (`Alt` + `Backspace`) | Stops the timer while the timer is running.<br>Resets the timer while the timer is stopped. |

## Recommended environment

- Mozilla Firefox 120.0 (64 bit) or later.
- Google Chrome 119.0.6045.160 (64 bit) or later.
- Microsoft Edge 119.0.2151.93 (64 bit) or later.

## Terms of Service

- The copyright is owned by the creator (I).
- The communications charge required for use of this site will be borne by the user.
- The creator is not responsible for any damage caused by computer viruses, data loss, or any other disadvantages caused by usinfg this site.
- You can use the source code, but please keep the copyright notice and license notice when redistributing.

## Known Issue

- The screen can't keep waking while the time is running if you use Firefox. This is because [Screen Wake Lock API](https://developer.mozilla.org/en-US/docs/Web/API/Screen_Wake_Lock_API) is not supported on Firefox.

## Breaking Changes

### 0.6.0

- The timer will not start when the input command is wrong.

## Release Notes

[Releases on GitHub](https://github.com/taidalog/ccc/releases)

## License

This application is licensed under MIT License.
