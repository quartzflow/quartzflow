# JobScheduler

JobScheduler is a scheduling tool entirely based on Quartz.NET that is intended to execute applications on Windows and provide the following capabilities in some manner
	* job chaining on success/failure/completion
	* retries on failure
	* exclusion calendars (to cater for holidays etc)
	* multiple run times for a job

The intent is for there to eventually be three components
	* the actual scheduling engine 
	* a CLI to interact with the engine 
	* a web front-end to interact with the engine

## Getting Started

Coming soon...

### Prerequisites

.NET 4.6

### Installing

To reserve the specified URL for non-administrator users and accounts
netsh http add urlacl url="http://+:5000/" user="Everyone"

To remove the reservation
netsh http delete urlacl url=http://+:5000/





## Running the tests

The JobScheduler unit tests exist in the JobScheduler.Tests project - currently must be executed via VS.NET

## Deployment

Coming soon...

## Built With

* Quartz.NET
* Common.Logging
* Newtonsoft.Json
* Nlog
* NUnit
* RhinoMocks

## Contributing

N/A

## Versioning

Coming soon...

## Authors

* Travis Draper

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Everyone whos tools/libraries have been used - especially the maintainers of Quartz.NET