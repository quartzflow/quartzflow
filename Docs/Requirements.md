JobScheduler
============

General requirements
--------------------

### MVP
Must be able to... 
* load all jobs/triggers/calendars on startup
* stop/start/pause/kill jobs
* support use of system variables
* support use of calendars
* write to std error file
* log error on failure

Not able to...
* add/change/remove jobs and associated items when they aren't running without stopping the host
* support max run alarm
* support termination run alarm


### Future
Must be able to... 
* add/change/remove jobs and associated items when they aren't running without stopping the host

