One of the best ways to take control of you what you eat is to keep a food diary. But, writing things down and tracking calories takes too long. 
And the actual act of recording is what's important, not what your record. 
The idea behing "All Your Plates" is to just let you take a picture of your food and it will automatically log it for you.


There are many projects like this already, but this one is mine.  

For development, the secrets are stored in the user secrets store. 

```powershell
dotnet user-secrets set "computerVisionAPIKey" "API KEY"
dotnet user-secrets set "computerVisionEndpoint" "API ENDPOING"
```

TODO:
Fix the mess with serilog
 - Test to make sure it works everywhere