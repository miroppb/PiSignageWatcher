# PiSignageWatcher

PiSignage Watcher is an application that runs in the background, and does the following:
- Checks a Google Drive folder for any new videos
- For each "search" term:
    - Downloads video with fitting title
    - Uploads to PiSignage, and processes it
    - Assigns video to the appropriate Playlist
    - Deploys  each Group (pushes changes to the TVs)
- If a video is present on PiSignage but not on the Google Drive, the video is removed from PiSignage
- Option for schedule to turn off/on TVs
- Ability to view Player name/ID, and add to database
- API that allows you to control the application (mostly) through the web.
	- Force Re-check
	- Reboot
	- ReDeploy
	- Turn All Off/On
	- History from the log

The login settings for PiSignage and file-tracking is kept in a sqlite database, along with the API info/hashes

## Usage

Create a db.db SQLite database file where the .exe will reside.

```
CREATE TABLE IF NOT EXISTS 'settings' (
	'user'	TEXT,
	'pass'	TEXT,
	'api'	TEXT,
	'gdrive'	TEXT
);

CREATE TABLE IF NOT EXISTS 'schedule' (
	'id'	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	'tv'	TEXT,
	'day'	INTEGER,
	'time'	TEXT,
	'action'	INTEGER
);

CREATE TABLE IF NOT EXISTS 'playlists' (
	'search'	TEXT,
	'name'	TEXT
);

CREATE TABLE IF NOT EXISTS 'groups' (
	'name'	TEXT,
	'hex'	TEXT
);

CREATE TABLE IF NOT EXISTS 'files' (
	'filename'	TEXT,
	'playlist'	TEXT
);

CREATE TABLE 'tvs' (
	'name'	TEXT,
	'hex'	TEXT
);
```

Get the group ids from
```
GET /groups
```