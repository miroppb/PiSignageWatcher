# PiSignageWatcher

PiSignage Watcher is an application that runs in the background, and does the following:
- Checks a Google Drive folder for any new videos
- If a video title starts with the word "LEFT" (for left TV)
    - Downloads it
    - Uploads to PiSignage, and processes it
    - Assigns video to the LEFT_TV Playlist
    - Deploys the LEFT Group (pushes changes to the TVs)
- If a video is present on PiSignage but not on the Google Drive, the video is removed from PiSignage

The login settings for PiSignage and file-tracking is kept in a sqlite database, and the API info/hashes are kept in a separate Config.cs file