# Ipdb.Database
Ipdb Database in .json format.

IPDB doesn't offer an open API so in order to allow for lookups / information querying in pinball 
related programs this was created to have more information then the current .csv files various programs have.

Also there is no indication as to when those databases were last updated. This project aims to solve that and provide a more robust file format for interaction with IPDB data.

Database set to refresh weekly automatically. If however there are minimal changes this may switch to monthly instead. Git change log will indicate when a change occurs and you can view the diff from there.

# Latest Database

[IPDB Database (.json)](https://github.com/xantari/Ipdb.Database/raw/master/Ipdb.Database/Database/ipdbdatabase.json)

# Loading the database (C#)

```cs
var data = JsonConvert.DeserializeObject<IpdbDatabase>(File.ReadAllText(@"{pathToJson}\\ipdbdatabase.json"));
```