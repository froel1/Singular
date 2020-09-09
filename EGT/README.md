EGT Game Integration
===========================

EGT Game Integration integrates vendor's content/games into Singular iGaming Platform. It provides specific set of API methods required for this game integration. System also handles abstraction over several components of the iGaming Platform, including: Segmentation System, Rule Engine, Central Bonus System etc.


Documentation
-------------
* Document 1 title [link](https://document1.address)
* Document 2 title [link](https://document2.address)


Service dependencies
--------------------

Service        | Version  | Repo                                                          | Pipeline
---------------|----------|---------------------------------------------------------------|-----------------------------------
CBS            | >= 2.1.0 | [link](https://bitbucket.org/singulargroup/cbs.worker)        | [link](https://jenkins.singular.uk/blue/organizations/jenkins/COREPlatform%2Fcbs.worker/branches) 
Game Adapter   | >= 1.7.0 | [link](https://bitbucket.org/singulargroup/sis.core.adapter)  | [link](https://jenkins.singular.uk/blue/organizations/jenkins/COREPlatform%2FSingularIntegrationSystem%2Fgame-adapter/branches) 
Game Directory | >= 2.2.0 | [link](https://bitbucket.org/singulargroup/sis.gamedirectory) | [link](https://jenkins.singular.uk/blue/organizations/jenkins/COREPlatform%2Fsis.gamedirectory/branches) 
Persistence    | >= 2.5.0 | [link](https://bitbucket.org/singulargroup/core.persistence)  | [link](https://jenkins.singular.uk/blue/organizations/jenkins/COREPlatform%2Fcore/branches) 
Web API        | >= 1.4.0 | [link](https://bitbucket.org/singulargroup/sis.webapi)        | [link](https://jenkins.singular.uk/blue/organizations/jenkins/COREPlatform%2FSingularIntegrationSystem%2Fsis.webapi/branches) 


Configuration
-------------

### Environment variables
Following environment variables are **required** for service to start and function properly

Name               | Notes
-------------------|-----------
INTEGRATION_LABEL  | Label of the integration, set in Dockerfile
INTEGRATION_TYPE   | Type of the integration, set in Dockerfile
CONSUL_SERVICE_URL | Set in the following format: `{Scheme}://{ServiceHost}:{ServicePort}/`

### Consul
Service is configured from two Consul key/value pair sources

* core-platform/common
* core-platform/integrations/egt/

First one is used for configuring helper libraries and logging. *This source __shouldn't__ generally be altered*

Second source contains general integration configuration parameters as well as parameters specific to the integration. It is also possible to add configuration overrides for common configuration. *(e.g. set different logging levels for this service)*

**Configuration reloads on change, so it is possible to apply modified settings without restarting the service, but it is not possible to add new instance without restarting**

Configuration needs to be set for each integration instance in separate keys, structure can look like this:
```
core-platform/integrations/egt/instances/instance-name-1
core-platform/integrations/egt/instances/instance-name-2
core-platform/integrations/egt/instances/instance-name-n
core-platform/integrations/egt/logging
```

```
Note: Instance names must be host names that are used to access the integration, e.g. singularqa.uk
```

Configuration structure looks like this:
```json
{
  "ProviderId": "00000000-0000-0000-0000-000000000000", // Provider id from CORE system for the integration
  "ProviderSecret": "00000000-0000-0000-0000-000000000000", // Provider secret from CORE system for the integration
  "ServiceUrl": "http://game-egt", // Service Url, same as docker container name
  "ServicePath": "/api/", // Service path and WebApiActions are same for every integration and MUST NOT be generally changed
  "WebApiActions": [
    "init"
  ],
  "SupportedCurrencies": {
    "GEL": "GEL",
    "USD": "USD",
    "GBP": "GBP",
    "BTC": "XXA",
    "BCH": "XXB",
    "ETH": "XXC"
  }, // List of currencies directly supported by this integration in Platform:Provider format
  "CurrencyConversions": {
    "EUR": "USD",
    "CAD": "USD",
    "CHF": "USD"
  }, // List of additionally supported currencies by this integration in Platform:Provider format, where currency conversion will take place
  "IpWhiteList": [
      "127.0.0.1",
      "xxx.xxx.xxx.xxx",
      "*"
  ], // List of vendor IPs that are allowed access to the API, use asterisk(*) to grant unrestricted access ONLY on test environment
  "Extended": {
    "ApiSecret": "00000000-0000-0000-0000-000000000000", // Unique random UUID used for request validation from the vendor
    "LaunchUrl": "https://vendor.com/vendor-service-path?brand={BRAND}&currency={CURRENCY}&game_id={GAME_ID}&token={TOKEN}&language={LANGUAGE}&channel={CHANNEL}&demo_play={DEMO_PLAY}&exit_url={EXIT_URL}&cashier_url={CASHIER_URL}&award_id={AWARD_ID}", // URL provided by vendor used for game launch, query string parameters in braces are substituted runtime
    "TokenTtl": 300 // Authentication token lifetime in seconds (generally 5min.)
  }
}
```


Database
--------
N/A


Scheduled Jobs
--------------
N/A


Logging
-------
Service utilizes standard .NET Core [logging configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#configure-logging), which looks like this:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information", // MUST be set to "Information" on production environments
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information", // Used for logging servie start/stop
      "Steeltoe": "Warning",
      "System.Net.Http.HttpClient": "None" // Disables health check logging
    },
    "GELF": {
      "Host":"xxx.xxx.xxx.xxx" // Graylog IP address
    }
  }
}
```
Graylog source is: `game-egt`

Service logs following data:

Event              | LogLevel    | Notes
-------------------|-------------|-------
Service start/stop | Information | *Logger: Microsoft.Hosting.Lifetime*
Request/Response   | Information | *Includes http status code and payload*
Exception          | Warning     | *Includes full stack trace*
Forbidden access   | Warning     | *Format: IP not whitelisted: {IP}*

```
Note: Service startup errors are ONLY logged to console
```


Health Metrics
--------------
Service exposes following endpoints to measure health:

Address                                 | Notes
----------------------------------------|----------------
http://{Ip}:{Port}/actuator             | Lists active health endpoints
http://{Ip}:{Port}/actuator/health      | Status of integration and dependent services
http://{Ip}:{Port}/actuator/info        | Service metadata
http://{Ip}:{Port}/actuator/trace       | Trace logs
http://{Ip}:{Port}/actuator/metrics     | Lists tracked metrics
http://{Ip}:{Port}/actuator/prometheus  | Stats for Prometheus


Troubleshooting
---------------

### Problem: Service doesn't start
* Check console logs for specific error
* Environment variable **CONSUL_SERVICE_URL** is not set
* Can not connect to Consul
* Configuration keys are not setup in Consul
* Graylog host is not configured