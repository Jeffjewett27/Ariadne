# Ariadne

A Hollow Knight mod for logging collision boxes and other data. I first designed it to gather labeled snapshots for an object detection dataset. I also added websocket functionality to give live game data.

I also extended the Debug mod to have new categories and to clean up overlapping terrain hitboxes.

## Settings:

- Show Hitboxes: Whether to show colored lines around objects and terrain.
- Logging Active: Whether to save collision data into JSON files.
- Log Folder: What folder to store the JSON logs.
- Logging Interval (MS): The number of milliseconds between log snapshots.
- Log Screenshots: Whether to capture screenshots alongside the JSON logs.
- Socket Active: Whether to open a web socket to give live collision data.
- Socket Port: What port to open the socket on.
- Socket Interval (MS): The number of milliseconds between socket snapshots.