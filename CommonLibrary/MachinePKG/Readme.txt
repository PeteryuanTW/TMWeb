//create the table with ur db name (sql commands file in machineservice.sql)

//inject machine service as singleton instance
builder.AddMachineService("{your connection string in appsetting}");
e.g.
in this example type "aaa"
======
{
  "ConnectionStrings": {
    "aaa": "Data Source=localhost;Initial Catalog=TMWeb;User ID=sa;Password=p@ssw0rd;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
======

//if u want to run without injecting service (open browser page)
app.RunMachineService();