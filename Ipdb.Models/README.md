# Ipdb.Models

C# models and utilities for working with IPDB (International Pinball Database) data.

## Installation

```bash
dotnet add package Ipdb.Models
```

## Usage

```csharp
using Ipdb.Models;
using Newtonsoft.Json;

// Load the IPDB database from JSON
var data = JsonConvert.DeserializeObject<IpdbDatabase>(
    File.ReadAllText(@"ipdbdatabase.json")
);

// Access pinball machine data
foreach (var machine in data.Machines)
{
    Console.WriteLine($"{machine.Title} - {machine.Manufacturer}");
}
```

## Features

- **IpdbDatabase**: Root container for the entire IPDB dataset
- **IpdbResult**: Represents a single pinball machine with complete metadata
- **IpdbUrl**: Represents URLs for ROMs, rule sheets, documentation, etc.
- **IpdbSystemType**: Enumeration of pinball system types

## Data

The IPDB database is refreshed monthly. Find the latest dataset here:
[IPDB Database (.json)](https://github.com/xantari/Ipdb.Database/raw/master/Database/ipdbdatabase.json)

