# AutoChart Toolchain
Automatic charting of Rock Band songs for use in [RhythmKata](https://github.com/hiptopjones/RhythmKata).

Going from video to playable song involves the following pipeline.

> NOTE: Use at your own risk, as it is unlikely that any of these directions are allowed by software EULAs or music copyright.

## Video Capture
Using a capture card (e.g. Elgato HD60 S), record a Rock Band 2 practice session of the full song at 100% speed.

Capture settings:

* 720p
* 60 fps

This will result in an MP4 on your hard drive.

## Frame Extraction
Run FrameExtractor.exe to extract an image from the video every N seconds. The image files are saved to the output directory in JPG format.

Example command-line:

```
FrameExtractor.exe --InputFilePath "NewKidInSchool\capture.mp4" --OutputDirectoryPath "NewKidInSchool\frames" --FrameIntervalInSeconds 0.20
```

## Image Rectification
The note runway in Rock Band is rendered in perspective view, which means notes at the top are shown smaller than at the bottom.  Run ImageCorrector.exe to flatten the runway perspective and crop the image down to just the region of interest.  Corrected images are saved to the output directory in the same format as they were input.

Example command-line:

```
ImageCorrector.exe --InputDirectoryPath "NewKidInSchool\frames" --OutputDirectoryPath "NewKidInSchool\corrected" --SkipFramesCount 45
```

## Sample Collection
Run SampleCollector.exe to perform a full-height vertical sweep of each lane on the runway.  The goal is to read a stripe of pixel values down the lane, which will later be used to determine what notes are present.  Sample data for each input file is saved to the output directory in a CSV format.

Example command-line:

```
SampleCollector.exe --InputDirectoryPath "NewKidInSchool\corrected" --OutputDirectoryPath "NewKidInSchool\samples"
```

## Timeline Generation
Run TimelineBuilder.exe to generate a timeline for the song that contains all detectable notes.  Various metadata about the song must be supplied to the tool so it knows how far notes travel from frame to frame, and therefore where to look when doing feature detection.  The timeline is written to the output file in a JSON format.

Example command-line:

```
TimelineBuilder.exe --InputDirectoryPath "NewKidInSchool\samples" --OutputFilePath "NewKidInSchool\timeline.json" --BeatIntervalInPixels 130 --FrameIntervalInSeconds 0.2 --BeatsPerMinute 139 --DivisionsPerBeat 4
```

## Table Creation
Run TableWriter.exe to convert the timeline into a table format.  Each row represents the smallest division of time in the song  Each column represents one piece of a standard drum kit (e.g. bass drum, snare drum, hi-hat, etc.).  The simple notes from the timeline are mapped to columns based on common conventions from Rock Band.  The table is written to the output file in a CSV forat.

The goal with this output file is that can be loaded into a spreadsheet program (e.g. Microsoft Excel) and missing notes or incorrect mappings can be easily fixed.  Detecting incorrect mappings or missing notes may require listening to a recording of the original song, watching a video of somebody playing the song on real drums, etc.

Example command-line:

 ```
TableWriter.exe --InputFilePath "NewKidInSchool\timeline.json" --OutputFilePath "NewKidInSchool\table.csv"
```

## Playable Chart
Run KataWriter.exe to create a playable chart file.  This chart file is the same basic format as the ones output by other charting programs (e.g. [Moonscraper](https://github.com/FireFox2000000/Moonscraper-Chart-Editor), but uses MIDI notes instead of lane indexes.

This file is playable with the game as-is, but some adjustment to the SyncTrack section will probably be required to sync up the start of the song to the start of the notes.

Example command-line:

```
KataWriter.exe --InputFilePath "NewKidInSchool\table_fixed.csv" --OutputFilePath "NewKidInSchool\song.kchart"
```

## Audio Extraction
Run AudioExtractor.exe to pull out the audio track out of the video file.  The audio wil lbe saved to the output file in MP3 format.

Example command-line:

```
AudioExtractor.exe --InputFilePath "NewKidInSchool\newkidinschool.mp4" --OutputFilePath "NewKidInSchool\newkidinschool.mp3"
```

Note that the audio in the captured practice session will not contain the drum track.  If you want audio that includes a drum track while playing, you will need to find another recording of the song, possibly on youtube.

A sketchy way to capture a song's audio from youtube, is to navigate to the video URL, and change "youtube.com" in the URL to "youtubepp.com".  This will enable you to download the audio to disk.

> Note that using a different version of the song for playalong may result in time synchronization issues.

## Limitations
This is a work in progress, so there are lots of warts and weak spots, including:

* Requires that a song has a single and consistent BPM.  If the drummer wasn't playing to a "click track" (meaning the BPM shifts over the course of the song) or if there are deliberate BPM changes in the song, this pipeline may not work properly.
* Tested with Rock Band 2's practice mode on an Xbox 360.  I have not yet tried it with other versions, so I don't know whether it will work.
* Not all settings are exposed as command-line arguments yet.  In the spirit of trying to get this working, some things ended up hardcoded. That will change over time, but may limit the ability to use this pipeline for other songs.
