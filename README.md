# AxiomSink
An unofficial Serilog sink that sends logs to Axiom.co

Supports configuration from appsetting:
```json
    "Serilog": {
        "Using": [
            "Serilog.Sinks.Console",
            "AxiomSink"
        ],
        "MinimumLevel": "Information",
        "WriteTo": [
            {
                "Name": "Console"
            },
            {
                "Name": "Axiom",
                "Args": {
                    "token": "<YOUR TOKEN>",
                    "orgID": "<YOUR ORG>",
                    "dataset": "<YOUR DATASET>",
                    "configuration": {
                        "removals": ["<ANY KEY THAT SHOULD BE REMOVED>"],
                        "renames": {
                            "<ANY KEY TO RENAME>": "<RENAMED KEY>"
                        }
                    }
                }
            }
        ],
        "Enrich": [
            "FromLogContext",
        ],
        "Properties": {
            "Application": "<YOUR APP NAME>"
        }
    }
```
